using APISeasonalTicket.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;

namespace APISeasonalTicket.Services
{
    public class MessageService : IMessageService
    {
        public GmailSettings _gmailSettings { get; }

        public MessageService(IOptions<GmailSettings> gmailSettings)
        {
            _gmailSettings = gmailSettings.Value;
        }

        public void SendEmail(string to, string subject, string body)
{
    try
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("El destinatario no puede estar vacío.");

        var fromEmail = _gmailSettings.Username;
        var password = _gmailSettings.Password;

        var message = new MailMessage();
        message.From = new MailAddress(fromEmail);
        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = true;

        var trimmedTo = to.Trim();
        message.To.Add(new MailAddress(trimmedTo));

        var smtpClient = new SmtpClient("smtp.gmail.com")
        {
            Port = _gmailSettings.Port,
            Credentials = new System.Net.NetworkCredential(fromEmail, password),
            EnableSsl = true,
        };

        smtpClient.Send(message);
    }
    catch (Exception ex)
    {
        throw new Exception("No se pudo enviar el mail", ex);
    }
}

    }
}
