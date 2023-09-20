using BestStoreApi.Models;

namespace BestStoreApi.Services;

public interface IEmailService
{
    public Task SendEmail(string subject, string toEmail, string username, string message);
}
