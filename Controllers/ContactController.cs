using Microsoft.AspNetCore.Mvc;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace PortfolioContactAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ContactController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ContactRequest req)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Portfolio", "nirmalwebsmithsolution@gmail.com"));
            email.To.Add(new MailboxAddress("Owner", "nirmalwebsmithsolution@gmail.com"));
            email.Subject = $"New Contact: {req.Name}";
            email.Body = new TextPart("plain")
            {
                Text = $"From: {req.Name} ({req.Email})\n\n{req.Message}"
            };

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                
                // Get password from configuration (Environment Variable on Render)
                string pass = "txad dcga donz mrmi";
                
                await smtp.AuthenticateAsync("nirmalwebsmithsolution@gmail.com", pass);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
                
                return Ok(new { status = "Success" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }

    public record ContactRequest(string Name, string Email, string Message);
}
