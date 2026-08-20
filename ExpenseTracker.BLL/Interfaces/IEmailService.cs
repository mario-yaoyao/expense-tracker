namespace ExpenseTracker.BLL.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toName, string toUsername, string toEmail, string subject, string textContent, string resetToken);
    }
}
