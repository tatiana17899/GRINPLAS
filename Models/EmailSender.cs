using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

public class EmailSender : IEmailSender
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _fromEmail;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;

    public EmailSender(string smtpServer, int smtpPort, string fromEmail, string smtpUsername, string smtpPassword)
    {
        _smtpServer = smtpServer;
        _smtpPort = smtpPort;
        _fromEmail = fromEmail;
        _smtpUsername = smtpUsername;
        _smtpPassword = smtpPassword;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("tucorreo@gmail.com", "tucontraseña"),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("tucorreo@gmail.com"),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
            Console.WriteLine("✅ Correo enviado a: " + email); // Verifica en la consola
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error al enviar correo: " + ex.Message);
            throw; // Esto mostrará el error en la página
        }
    }
}