using System.Net;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

// Add services for Swagger/OpenAPI for testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS - In production, you should replace "*" with your actual portfolio URL
builder.Services.AddCors(options =>
{
    options.AddPolicy("PortfolioPolicy", policy =>
    {
        policy.AllowAnyOrigin() // Change to your frontend URL later for extra security
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Enable Swagger in Development or if explicitly needed
if (app.Environment.IsDevelopment() || true) // Keeping true for testing purposes
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("PortfolioPolicy");

// Contact API Endpoint
app.MapPost("/api/contact", async (ContactRequest request, IConfiguration config) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { message = "All fields (Name, Email, Message) are required." });
    }

    try
    {
        var smtpSettings = config.GetSection("SmtpSettings");
        string fromEmail = smtpSettings["FromEmail"] ?? throw new Exception("FromEmail not configured.");
        string fromPassword = smtpSettings["FromPassword"] ?? throw new Exception("FromPassword not configured.");
        string smtpHost = smtpSettings["Host"] ?? "smtp.gmail.com";
        int smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
        bool enableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
        string toEmail = smtpSettings["ToEmail"] ?? fromEmail;

        using var message = new MailMessage();
        message.From = new MailAddress(fromEmail, "Portfolio Contact Form");
        message.To.Add(new MailAddress(toEmail));
        message.Subject = $"New Portfolio Message From: {request.Name}";
        message.Body = $"You received a new message from your portfolio site!\n\n" +
                       $"Name: {request.Name}\n" +
                       $"Email: {request.Email}\n\n" +
                       $"Message:\n{request.Message}";
        message.IsBodyHtml = false;

        using var smtpClient = new SmtpClient(smtpHost, smtpPort);
        smtpClient.EnableSsl = enableSsl;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Credentials = new NetworkCredential(fromEmail, fromPassword);
        
        await smtpClient.SendMailAsync(message);

        return Results.Ok(new { message = "Email sent successfully!" });
    }
    catch (Exception ex)
    {
        // Log the exception (using a real logger is better, but this is for simplicity)
        Console.WriteLine($"Error sending email: {ex.Message}");
        return Results.Problem("Sorry, there was an error sending your message. Please try again later.");
    }
})
.WithName("SendContactEmail")
.WithOpenApi();

app.Run();

public record ContactRequest(string Name, string Email, string Message);
