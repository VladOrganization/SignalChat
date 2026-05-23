using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Validation.AspNetCore;
using Scalar.AspNetCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Database.Entities;
using SignalChat.Backend.Hubs;
using SignalChat.Backend.Middleware;
using SignalChat.Backend.Pipeline;
using SignalChat.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddLogging(options =>
{
    options.SetMinimumLevel(LogLevel.Debug);
});

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<ChatDbContext>(options => {
    options.UseNpgsql(builder.Configuration.GetConnectionString("ChatDb"));
    options.UseOpenIddict();
});

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ChatDbContext>()
    .AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme =
            IdentityConstants.ApplicationScheme;
    })
    .AddGoogle(options =>
    {
        options.ClientId =
            builder.Configuration["Authentication:Google:ClientId"]!;

        options.ClientSecret =
            builder.Configuration["Authentication:Google:ClientSecret"]!;

        options.CallbackPath = "/signin-google";

        options.SignInScheme =
            IdentityConstants.ExternalScheme;

        options.SaveTokens = true;
    });

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
        options.SetTokenEndpointUris("connect/token");
        
        // Включаем необходимые OAuth 2.0 потоки
        options.AllowPasswordFlow() // логин/пароль
            .AllowRefreshTokenFlow();       // обновление токенов
        
        options.AcceptAnonymousClients();
        
        // Регистрируем сертификаты для подписи и шифрования
        // ВАЖНО: Для разработки используйте AddDevelopmentEncryptionCertificate(),
        // для production замените на production сертификаты
        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        
        options
            .UseAspNetCore()
            .EnableTokenEndpointPassthrough();
        
        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
        options.SetRefreshTokenLifetime(TimeSpan.FromDays(30));

        options.UseReferenceRefreshTokens();

        options.DisableAccessTokenEncryption();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();

        options.UseAspNetCore();
    });

//builder.Services.AddHttpClient();
//builder.Services.AddHttpContextAccessor();

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

builder.Services.AddAuthorization();

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