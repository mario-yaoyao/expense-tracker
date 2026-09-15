using ExpenseTracker.BLL.Interfaces;
using ExpenseTracker.BLL.Mappings;
using ExpenseTracker.BLL.Services;
using ExpenseTracker.Configurations;
using ExpenseTracker.DAL.Data;
using ExpenseTracker.DAL.Interfaces;
using ExpenseTracker.DAL.Repositories;
using ExpenseTracker.Models.Dtos.Responses;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var dbConnectionString = builder.Configuration.GetConnectionString("ExpenseTrackerDatabase");

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

builder.Services.AddAutoMapper(cfg => { }, typeof(ExpenseProfile).Assembly);
builder.Services.AddAutoMapper(cfg => { }, typeof(UserProfile).Assembly);
builder.Services.AddAutoMapper(cfg => { }, typeof(CategoryProfile).Assembly);
builder.Services.AddAutoMapper(cfg => { }, typeof(IncomeProfile).Assembly);
builder.Services.AddAutoMapper(cfg => { }, typeof(RecentUsersProfile).Assembly);

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IIncomeRepository, IncomeRepository>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

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

app.MapControllers();

app.Run();