using Microsoft.AspNetCore.Http.Connections;
namespace test_signalAr
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();  //connect singlR in project
         
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins"; // suko cors

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                    policy =>
                    {
                        policy.WithOrigins("http://localhost:5173")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    });
            });

            var app = builder.Build();
            
            app.UseCors(MyAllowSpecificOrigins);

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapHub<ChatHub>("/chat", options =>
            {
                options.Transports = HttpTransportType.WebSockets;
                options.TransportSendTimeout = TimeSpan.FromSeconds(60);
            });
            app.Run();
        }
    }
}
