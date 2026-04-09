using System.Net;
using System.Net.Mail;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

if (app.Environment.IsDevelopment() || true) 
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // REMOVED
app.UseCors("PortfolioPolicy");

app.MapGet("/", () => "Portfolio API is running! (V2 - Debug Mode)");

app.MapPost("/api/contact", async (ContactRequest request, IConfiguration config) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { message = "All fields are required." });
    }

    try
    {
        var smtpSettings = config.GetSection("SmtpSettings");
        
        // Priority: 1. Render Env Var, 2. appsettings.json
        string fromEmail = config["SmtpSettings__FromEmail"] ?? smtpSettings["FromEmail"] ?? "nirmalwebsmithsolution@gmail.com";
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
        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
        
        await smtpClient.SendMailAsync(message);

        return Results.Ok(new { message = "Email sent successfully!" });
    }
    catch (Exception ex)
    {
        // DETAILED LOGGING
        Console.WriteLine($"FULL ERROR: {ex.Message}");
        if (ex.InnerException != null) {
            Console.WriteLine($"INNER ERROR: {ex.InnerException.Message}");
        }
        return Results.Problem("Email sending failed. Check Render logs for 'FULL ERROR'.");
    }
})
.WithName("SendContactEmail")
.WithOpenApi();

app.Run();

public record ContactRequest(string Name, string Email, string Message);
