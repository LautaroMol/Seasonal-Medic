using APISeasonalTicket.Models;

namespace APISeasonalTicket.Services
{
    public interface IMessageService
    {
        GmailSettings _gmailSettings { get; }

        void SendEmail(string subject, string body, string to);
    }
}