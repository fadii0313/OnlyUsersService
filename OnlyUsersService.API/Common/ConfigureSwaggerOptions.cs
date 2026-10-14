using System;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlyUsersService.API.Common
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;
        private readonly IWebHostEnvironment _environment;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IWebHostEnvironment environment)
        {
            _provider = provider;
            _environment = environment;
        }

        public void Configure(SwaggerGenOptions options)
        {
            // Add a swagger document for each discovered API version
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }

            // Configure JWT Bearer Security Scheme for Swagger UI testing
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Enter JWT Bearer token. Example: 'eyJhbGciOiJIUzI1Ni...'. Click Authorize to test authenticated endpoints.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Enable annotations
            options.EnableAnnotations();
        }

        private OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
        {
            var info = new OpenApiInfo
            {
                Title = "OnlyUsersService API",
                Version = description.ApiVersion.ToString(),
                Description = "Microservice for User Registration, Authentication, Profile Management, Role Assignment, and User CRUD operations."
            };

            if (description.IsDeprecated)
            {
                info.Description += " This API version has been deprecated.";
            }

            return info;
        }
    }
}
