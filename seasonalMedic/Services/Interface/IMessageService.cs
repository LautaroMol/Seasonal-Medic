using APISeasonalMedic.Models;

namespace APISeasonalMedic.Services.Interface
{
    public interface IMessageService
    {
        GmailSettings _gmailSettings { get; }

        void SendEmail(string subject, string body, string to);
    }
}
