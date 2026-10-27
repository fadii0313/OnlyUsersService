using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OnlyUsersService.API.Common;
using OnlyUsersService.API.Data;
using OnlyUsersService.API.Models.ResponseModels.BaseModels;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OnlyUsersService.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register EF Core DbContext with SQL Server
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<OnlyUsersDbContext>(options =>
            {
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
            });

            // Register Memory Cache
            services.AddMemoryCache();

            // Register Core Domain Services & Repositories via Extension Methods
            services.RegisterServices();
            services.RegisterRepositories();
            services.AddHttpContextAccessor();

            // Configure Global Filters and MVC Controllers
            services.AddControllers(options =>
            {
                options.Filters.Add<GlobalActivityHandler>();
                options.Filters.Add<GlobalExceptionHandler>();
            }).AddJsonOptions(jsonOptions =>
            {
                jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                jsonOptions.JsonSerializerOptions.WriteIndented = true;
            });

            // Standardize model validation error responses with BaseResponseModel
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var response = new BaseResponseModel(false)
                    {
                        ResponseCode = ApiResponseCode.Validation,
                        ResponseMessage = "One or more validation errors occurred."
                    };

                    foreach (var keyState in context.ModelState)
                    {
                        foreach (var error in keyState.Value.Errors)
                        {
                            var errorMessage = string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Invalid field value." : error.ErrorMessage;
                            response.AddError(errorMessage, keyState.Key);
                        }
                    }

                    return new BadRequestObjectResult(response);
                };
            });


            // Endpoint route path in lower case
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = false;
            });

            // API Versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            // Swagger Configuration
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options =>
            {
                options.OperationFilter<SwaggerDefaultValues>();

                // Include XML Comments for detailed Swagger documentation
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            });

            // CORS Configuration
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // JWT Bearer Authentication & Authorization
            var jwtSecretKey = Configuration["SecuritySettings:JwtSecretKey"] ?? "P4a59nfuPoD7r5j9TO84z0iGVSjZK0VrmkHi4UeoYE8=";
            var jwtIssuer = Configuration["SecuritySettings:JwtIssuer"] ?? "OnlyUsersService";
            var jwtAudience = Configuration["SecuritySettings:JwtAudience"] ?? "OnlyUsersService.Client";
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = jwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authHeader.Substring("Bearer Bearer ".Length).Trim();
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole(AppRoles.Admin));
                options.AddPolicy("UserOrAdmin", policy => policy.RequireRole(AppRoles.Admin, AppRoles.User));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            if (!env.IsProduction())
            {
                app.UseDeveloperExceptionPage();
            }

            // Enable Static Files for Embedded Interactive Frontend SPA Dashboard
            app.UseDefaultFiles();
            app.UseStaticFiles();

            // Swagger available at /swagger
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in provider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"OnlyUsersService {description.GroupName.ToUpperInvariant()}");
                }
                options.RoutePrefix = "swagger";
                options.DisplayRequestDuration();
                options.DocumentTitle = "OnlyUsersService API Documentation";
            });

            app.UseRouting();
            app.UseCors("AllowAll");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
