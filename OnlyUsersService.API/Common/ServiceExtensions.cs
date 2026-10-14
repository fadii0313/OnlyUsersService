using Microsoft.Extensions.DependencyInjection;
using OnlyUsersService.API.Common;
using OnlyUsersService.API.Repositories.Implementation;
using OnlyUsersService.API.Repositories.Interface;
using OnlyUsersService.API.Services.Implementation;
using OnlyUsersService.API.Services.Interface;

namespace OnlyUsersService.API.Common
{
    public static class ServiceExtensions
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<IEncryptionService, EncryptionService>();
            services.AddScoped<ICurrentUserContext, CurrentUserContext>();
            services.AddScoped<IJwtAuthService, JwtAuthService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();

            return services;
        }

        public static IServiceCollection RegisterRepositories(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();

            return services;
        }
    }
}
