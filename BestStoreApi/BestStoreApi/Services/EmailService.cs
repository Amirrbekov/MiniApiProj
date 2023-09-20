using BestStoreApi.Models;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Http.HttpResults;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BestStoreApi.Services;

public class EmailService : IEmailService
{
    private readonly string apiKey;
    private readonly string fromEmail;
    private readonly string senderName;
    public EmailService(IConfiguration configuration)
    {
        apiKey = configuration["EmailSender:ApiKey"];
        fromEmail = configuration["EmailSender:FromEmail"];
        senderName = configuration["EmailSender:SenderName"];
    }
    public async Task SendEmail(string subject, string toEmail, string username, string message)
    {
        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, senderName);
        var to = new EmailAddress(toEmail, username);
        var plainTextContent = message;
        var htmlContent = "";
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);
    }
}
