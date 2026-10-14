using System.Collections.Generic;

namespace OnlyUsersService.API.Common
{
    public interface ICurrentUserContext
    {
        long UserId { get; }
        string Username { get; }
        string Email { get; }
        IReadOnlyList<string> Roles { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
    }
}
