using FluentAssertions.Common;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Hubs;
using SignalChat.Backend.Middleware;
using SignalChat.Backend.Pipeline;
using SignalChat.Backend.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(o => o.SuppressModelStateInvalidFilter = true);
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<ChatDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("ChatDb"));
    options.UseOpenIddict();
});
//
builder.Services.AddOpenIddict()
    // 1. Регистрируем ядро и указываем EF Core для хранения
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
               .UseDbContext<ChatDbContext>();
    })
    // 2. Конфигурируем сервер OpenIddict
    .AddServer(options =>
    {
        options.SetUserInfoEndpointUris("connect/userinfo");
        options.UseAspNetCore().EnableUserInfoEndpointPassthrough();
        // Устанавливаем endpoint'ы для получения токена и информации о пользователе
        options.SetTokenEndpointUris("connect/token")
               .SetUserInfoEndpointUris("connect/userinfo");

        // Включаем необходимые OAuth 2.0 потоки
        options.AllowPasswordFlow()           // логин/пароль
               .AllowRefreshTokenFlow()       // обновление токенов
               .AllowClientCredentialsFlow(); // для сервис-аккаунтов

        // Регистрируем сертификаты для подписи и шифрования
        // ВАЖНО: Для разработки используйте AddDevelopmentEncryptionCertificate(),
        // для production замените на production сертификаты
        options.AddDevelopmentEncryptionCertificate()
               .AddDevelopmentSigningCertificate();

        // Интеграция с ASP.NET Core
        options.UseAspNetCore()
               .EnableTokenEndpointPassthrough(); // Позволяет обрабатывать /connect/token вручную
    });
//

builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        // Указываем URL нашего центрального auth-сервера
        options.SetIssuer("https://localhost:5001/");
        options.AddAudiences("my_api");
        options.UseLocalServer(); // Валидация будет использовать тот же сервер, если API и AuthServer в одном проекте
        options.UseAspNetCore();
    });
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ChatDbContext>()
    .AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(options =>
{
    // Отключаем все требования к паролю
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 1;          // Минимальная длина — 1 символ
    options.Password.RequiredUniqueChars = 1;
    options.User.AllowedUserNameCharacters = null;
});

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddSingleton<TokenService>();
builder.Services.AddSignalR();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });

string corsPolicyName = "CorsOptions";

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicyName,
        policyBuilder => policyBuilder
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    );
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

//app.UseCors(corsPolicyName);
app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseStaticFiles();

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();

public partial class Program
{
}