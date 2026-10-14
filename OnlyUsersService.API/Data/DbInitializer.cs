using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlyUsersService.API.Common;
using OnlyUsersService.API.Models.Entities;

namespace OnlyUsersService.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(OnlyUsersDbContext context, ILogger logger = null)
        {
            try
            {
                logger?.LogInformation("Checking database seed state...");

                // 1. Seed Roles: Admin and User
                if (!await context.Roles.AnyAsync())
                {
                    logger?.LogInformation("Seeding default roles (Admin, User)...");
                    var roles = new List<Role>
                    {
                        new Role
                        {
                            Name = AppRoles.Admin,
                            Description = "Administrator with full system privileges and user management access.",
                            CreatedAt = DateTime.UtcNow
                        },
                        new Role
                        {
                            Name = AppRoles.User,
                            Description = "Standard registered user with access to their own profile.",
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    await context.Roles.AddRangeAsync(roles);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Default roles seeded successfully.");
                }

                var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == AppRoles.Admin);
                var userRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == AppRoles.User);

                // 2. Seed Default Admin User if not exists
                if (!await context.Users.AnyAsync(u => u.Username == "admin"))
                {
                    logger?.LogInformation("Seeding default Administrator user 'admin'...");
                    var encryptionService = new EncryptionService();
                    var adminUser = new User
                    {
                        Username = "admin",
                        Email = "admin@onlyusers.local",
                        PasswordHash = encryptionService.HashPassword("Admin@123456"),
                        FirstName = "System",
                        LastName = "Administrator",
                        PhoneNumber = "+1-555-0100",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        Profile = new UserProfile
                        {
                            Department = "Information Technology",
                            Designation = "Chief Security Officer & Admin",
                            Address = "100 Enterprise Way",
                            City = "Metropolis",
                            Country = "United States",
                            Bio = "Default System Administrator account for OnlyUsersService microservice.",
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    await context.Users.AddAsync(adminUser);
                    await context.SaveChangesAsync();

                    if (adminRole != null)
                    {
                        context.UserRoles.Add(new UserRole
                        {
                            UserId = adminUser.Id,
                            RoleId = adminRole.Id,
                            AssignedAt = DateTime.UtcNow
                        });
                        await context.SaveChangesAsync();
                    }

                    logger?.LogInformation("Default admin user seeded successfully with username: admin (password: Admin@123456).");
                }

                // 3. Seed Default Standard User if not exists
                if (!await context.Users.AnyAsync(u => u.Username == "john_doe"))
                {
                    logger?.LogInformation("Seeding standard test user 'john_doe'...");
                    var encryptionService = new EncryptionService();
                    var standardUser = new User
                    {
                        Username = "john_doe",
                        Email = "john.doe@onlyusers.local",
                        PasswordHash = encryptionService.HashPassword("User@123456"),
                        FirstName = "John",
                        LastName = "Doe",
                        PhoneNumber = "+1-555-0199",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        Profile = new UserProfile
                        {
                            Department = "Software Engineering",
                            Designation = "Senior Developer",
                            Address = "456 Tech Park",
                            City = "Tech City",
                            Country = "United States",
                            Bio = "Standard registered user account for testing OnlyUsersService features.",
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    await context.Users.AddAsync(standardUser);
                    await context.SaveChangesAsync();

                    if (userRole != null)
                    {
                        context.UserRoles.Add(new UserRole
                        {
                            UserId = standardUser.Id,
                            RoleId = userRole.Id,
                            AssignedAt = DateTime.UtcNow
                        });
                        await context.SaveChangesAsync();
                    }

                    logger?.LogInformation("Standard user seeded successfully with username: john_doe (password: User@123456).");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error occurred during DbInitializer.SeedAsync: {Message}", ex.Message);
                throw;
            }
        }
    }
}
