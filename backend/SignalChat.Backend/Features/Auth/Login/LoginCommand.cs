using MediatR;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Auth.Login;

public record LoginCommand(string Code) : IRequest<AuthResponse>;
