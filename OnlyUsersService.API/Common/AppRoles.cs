namespace OnlyUsersService.API.Common
{
    /// <summary>
    /// Centralized role definitions for Role-Based Access Control (RBAC).
    /// </summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";

        /// <summary>
        /// Comma-separated list of administrative roles permitted to execute mutation actions.
        /// </summary>
        public const string AdminRoles = "Admin";

        /// <summary>
        /// Comma-separated list of all roles.
        /// </summary>
        public const string AllRoles = "Admin,User";
    }
}
