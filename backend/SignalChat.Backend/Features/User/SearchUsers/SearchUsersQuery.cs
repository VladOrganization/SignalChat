using MediatR;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.User.SearchUsers;

/// <summary>
/// Поиск пользователей для выбора собеседника.
/// Текущий пользователь из результатов исключается.
/// </summary>
public record SearchUsersQuery(string CurrentUserId, string? Search) : IRequest<List<UserListItem>>;
