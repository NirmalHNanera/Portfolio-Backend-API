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

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("PortfolioPolicy");

app.MapGet("/", () => "Portfolio API is running! (V2 - Live)");

app.MapPost("/api/contact", async (ContactRequest request, IConfiguration config) =>
{
    if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Message))
    {
        return Results.BadRequest(new { message = "All fields are required." });
    }

    try
    {
        var smtpSettings = config.GetSection("SmtpSettings");
        string fromEmail = "nirmalwebsmithsolution@gmail.com";
        string fromPassword = "txad dcga donz mrmi";
        
        using var message = new MailMessage();
        message.From = new MailAddress(fromEmail, "Portfolio Contact Form");
        message.To.Add(new MailAddress(fromEmail));
        message.Subject = $"New Portfolio Message From: {request.Name}";
        message.Body = $"Name: {request.Name}\nEmail: {request.Email}\n\n{request.Message}";

        using var smtpClient = new SmtpClient("smtp.gmail.com", 587);
        smtpClient.EnableSsl = true;
        smtpClient.UseDefaultCredentials = false;
        smtpClient.Credentials = new NetworkCredential(fromEmail, fromPassword);
        
        await smtpClient.SendMailAsync(message);
        return Results.Ok(new { message = "Email sent successfully!" });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"FULL ERROR: {ex.Message}");
        return Results.Problem($"Email sending failed: {ex.Message}");
    }
})
.WithName("SendContactEmail")
.WithOpenApi();

app.Run();

public record ContactRequest(string Name, string Email, string Message);
