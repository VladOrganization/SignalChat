using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalChat.Backend.Hubs;

[Authorize]
public class ChatHub : Hub;
