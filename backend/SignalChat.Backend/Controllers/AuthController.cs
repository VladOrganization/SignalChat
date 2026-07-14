using MediatR;
using Microsoft.AspNetCore.Mvc;
using SignalChat.Backend.Features.Auth.Login;
using SignalChat.Backend.Features.Auth.Refresh;
using SignalChat.Backend.Features.Auth.Register;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public Task<AuthResponse> Register(RegisterCommand command, CancellationToken ct) => sender.Send(command, ct);

    [HttpPost("login")]
    public Task<AuthResponse> Login(LoginCommand command, CancellationToken ct) => sender.Send(command, ct);

    [HttpPost("refresh")]
    public Task<AuthResponse> Refresh(RefreshCommand command, CancellationToken ct) => sender.Send(command, ct);
}
