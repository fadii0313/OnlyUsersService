using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace OnlyUsersService.API.Common
{
    public class CurrentUserContext : ICurrentUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal User => _httpContextAccessor?.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

        public long UserId
        {
            get
            {
                var claim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User?.FindFirst("uid")?.Value
                            ?? User?.FindFirst("sub")?.Value;

                return long.TryParse(claim, out var id) ? id : 0;
            }
        }

        public string Username => User?.FindFirst(ClaimTypes.Name)?.Value
                                  ?? User?.FindFirst("name")?.Value
                                  ?? User?.Identity?.Name;

        public string Email => User?.FindFirst(ClaimTypes.Email)?.Value
                               ?? User?.FindFirst("email")?.Value;

        public IReadOnlyList<string> Roles => User?.FindAll(ClaimTypes.Role)
                                                  .Select(c => c.Value)
                                                  .ToList() ?? new List<string>();

        public bool IsInRole(string role)
        {
            return User?.IsInRole(role) ?? false;
        }
    }
}
