using MediatR;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Auth.Register;

public record RegisterCommand(string UserName) : IRequest<AuthResponse>;
