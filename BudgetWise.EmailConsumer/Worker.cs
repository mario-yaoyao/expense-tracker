using BudgetWise.BLL.Interfaces;
using BudgetWise.Shared.Contracts.Events;

namespace BudgetWise.EmailConsumer
{
    public class Worker(
        IConsumerService<EmailRequestedEvent> emailRequestedConsumer,
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var emailRequestedTask = emailRequestedConsumer.StartAsync(
                "budgetwise-email-notifications",
                "notification-group",
                async message =>
                {
                    logger.LogInformation(
                        "Sending password reset email to {Username}...",
                        message.Username);

                    using var scope = scopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                    await emailService.SendEmailAsync(
                        message.FullName,
                        message.Username,
                        message.Email,
                        message.Subject,
                        message.Message,
                        message.ResetToken);
                },
                stoppingToken);
        }
    }
}