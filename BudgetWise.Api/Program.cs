using BudgetWise.BLL.Interfaces;
using BudgetWise.BLL.Jobs;
using BudgetWise.BLL.Mappings;
using BudgetWise.BLL.Services;
using BudgetWise.Configurations;
using BudgetWise.DAL.Data;
using BudgetWise.DAL.Interfaces;
using BudgetWise.DAL.Repositories;
using BudgetWise.Models.Dtos.Responses;
using Confluent.Kafka;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var dbConnectionString = builder.Configuration.GetConnectionString("BudgetWiseDatabase");
var hangfireConnectionString = builder.Configuration.GetConnectionString("HangfireDatabase");
var kafkaConnectionString = builder.Configuration.GetConnectionString("KafkaServer");

var producerConfig = new ProducerConfig
{
    BootstrapServers = kafkaConnectionString,
    AllowAutoCreateTopics = true,
    Acks = Acks.All
};

builder.Services.AddSingleton<IProducer<string, string>>(
    _ => new ProducerBuilder<string, string>(producerConfig).Build());

builder.Services.AddSingleton<IProducerService, ProducerService>();

// comment on Update-Database execution
SerilogConfiguration.Configure(dbConnectionString!);
builder.Host.UseSerilog();

builder.Services.AddHttpContextAccessor();

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(x => new
                {
                    field = x.Key,
                    messages = x.Value!.Errors.Select(e => e.ErrorMessage)
                });

            return new BadRequestObjectResult(new ApiResDto<object>
            {
                Success = false,
                ValidationErrors = errors
            });
        };
    });    

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(dbConnectionString));

builder.Services.AddHangfire(configuration => configuration.
    SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(hangfireConnectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["AppSettings:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
        ValidateIssuerSigningKey = true
    };
});

builder.Services.Configure<CryptoOptions>(builder.Configuration.GetSection("Crypto"));

builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IIncomeService, IncomeService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICryptoService, CryptoService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IRecurringTransactionJob, RecurringTransactionJob>();

builder.Services.AddAutoMapper(cfg => { }, typeof(ExpenseProfile).Assembly);
builder.Services.AddAutoMapper(cfg => { }, typeof(UserProfile).Assembly);
builder.Services.AddAutoMapper(cfg => { }, typeof(CategoryProfile).Assembly);
builder.Services.AddAutoMapper(cfg => { }, typeof(IncomeProfile).Assembly);
builder.Services.AddAutoMapper(cfg => { }, typeof(RecentUsersProfile).Assembly);
builder.Services.AddAutoMapper(cfg => { }, typeof(TransactionProfile).Assembly);

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IRecurringTransactionRepository, RecurringTransactionRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins(builder.Configuration["ClientSettings:BaseUrl"]!)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowClient");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard();

RecurringJob.AddOrUpdate<IRecurringTransactionJob>(
    "monthly-recurring-transactions",
    x => x.ProcessRecurringTransactionsAsync(),
    Cron.Monthly(1)
);

app.MapControllers();

app.Run();