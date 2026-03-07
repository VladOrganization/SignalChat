using Microsoft.AspNetCore.SignalR;


namespace test_signalAr
{
    public class ChatHub:Hub
    {
        public record Message(string message, string userName);

        private static List<Message> Messages = [];
        
        public async Task Send(string message)
        {
            var newMessage = new Message(message, Context.ConnectionId);
            
            Messages.Add(newMessage);
            
            await Clients.All.SendAsync("Receive", newMessage);
        }

        public async Task Init()
        {
            await Clients.Caller.SendAsync("OnInit", Messages);
        }

        public async Task Clear()
        {
            Messages.Clear();
        }
        public override async Task OnConnectedAsync()
        {
            var context = Context.GetHttpContext();
            if (context is not null)
            {
                if (context.Request.Cookies.ContainsKey("name"))
                {
                    if (context.Request.Cookies.TryGetValue("name", out var userName))
                    {
                        Console.WriteLine($"{userName}");
                    }
                }
                
                // получаем юзер-агент
                Console.WriteLine($"UserAgent = {context.Request.Headers["User-Agent"]}");
                // получаем ip
                Console.WriteLine($"RemoteIpAddress = {context.Connection?.RemoteIpAddress?.ToString()}");
                await this.Clients.All.SendAsync("Notify", $"{DateTime.Now.Hour}:{DateTime.Now.Minute}   |{Context.ConnectionId} sign in chat");
                await base.OnConnectedAsync();
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await this.Clients.All.SendAsync("Notify", $"{DateTime.Now.Hour}:{DateTime.Now.Minute}   |{Context.ConnectionId} sign out");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
