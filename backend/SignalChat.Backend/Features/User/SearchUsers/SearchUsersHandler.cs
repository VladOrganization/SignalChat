using MediatR;
using Microsoft.EntityFrameworkCore;
using SignalChat.Backend.Database;
using SignalChat.Backend.Exceptions;
using SignalChat.Backend.Models;

namespace SignalChat.Backend.Features.User.SearchUsers;

public class SearchUsersHandler(ChatDbContext db)
    : IRequestHandler<SearchUsersQuery, List<UserListItem>>
{
    private const int MaxResults = 20;

    public async Task<List<UserListItem>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
       
        if (string.IsNullOrWhiteSpace(request.Search))
            return [];
        
        var search = request.Search.Trim();
        var pattern = $"%{search}%";
            
        var users = await db.Users
            .Where(u => u.Id != request.CurrentUserId&& EF.Functions.ILike(u.UserName, pattern))
            .OrderBy(u => u.UserName)
            .Take(MaxResults)
            .Select(u => new UserListItem(u.Id, u.UserName!))
            .ToListAsync(cancellationToken);



        return users;
    }
}
