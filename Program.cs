using System.Net;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

// Add services for Swagger/OpenAPI for testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PortfolioPolicy", policy =>
    {
        policy.AllowAnyOrigin() 
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Enable Swagger
if (app.Environment.IsDevelopment() || true) 
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // REMOVED FOR CLOUD COMPATIBILITY
app.UseCors("PortfolioPolicy");

// Root endpoint for Render health check
app.MapGet("/", () => "Portfolio API is running!");

// Contact API Endpoint
app.MapPost("/api/contact", async (ContactRequest request, IConfiguration config) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { message = "All fields are required." });
    }

    try
    {
        var smtpSettings = config.GetSection("SmtpSettings");
        string fromEmail = smtpSettings["FromEmail"] ?? "nirmalwebsmithsolution@gmail.com";
        string fromPassword = config["SmtpSettings__FromPassword"] ?? smtpSettings["FromPassword"] ?? "";
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
        Console.WriteLine($"Error sending email: {ex.Message}");
        return Results.Problem("Error sending message. Check Render environment variables.");
    }
})
.WithName("SendContactEmail")
.WithOpenApi();

app.Run();

public record ContactRequest(string Name, string Email, string Message);
