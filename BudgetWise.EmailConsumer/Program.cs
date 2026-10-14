using BudgetWise.BLL.Factories;
using BudgetWise.BLL.Interfaces;
using BudgetWise.BLL.Services;
using BudgetWise.EmailConsumer;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(typeof(IConsumerService<>), typeof(ConsumerService<>));

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSingleton<IKafkaConsumerFactory, KafkaConsumerFactory>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
