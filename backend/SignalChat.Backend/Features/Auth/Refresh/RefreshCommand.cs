using MediatR;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.Auth.Refresh;

public record RefreshCommand(string RefreshToken) : IRequest<AuthResponse>;
