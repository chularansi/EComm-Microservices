using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using System.Security.Claims;
using System.Text.Json;

namespace BuildingBlocks.Security
{
    public static class SecurityExtensions
    {
        public static IServiceCollection AddSharedAuthentication(
            this IServiceCollection services, 
            IConfiguration configuration,
            IWebHostEnvironment environment,
            bool isPublicService = false,
            string apiAccessScope = "")
        {
            if (environment.IsDevelopment())
            {
                // Bind settings from appsettings.json
                var authority = configuration["Security:Keycloak:Authority"];
                var audience = configuration["Security:Keycloak:Audience"];

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.Authority = authority;
                    options.Audience = audience;
                    options.RequireHttpsMetadata = false; // Set to true in production
                    options.TokenValidationParameters.RoleClaimType = EcommClaimTypes.Role;
                    // Custom mapping to read Keycloak global/resource roles into .NET Claims
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            if (context.Principal?.Identity is ClaimsIdentity identity)
                            {
                                // 1. Process and flatten the space-delimited scope string
                                var scopeClaim = identity.FindFirst(EcommClaimTypes.Scope);
                                if (scopeClaim is not null)
                                {
                                    var scopes = scopeClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                                    identity.RemoveClaim(scopeClaim);
                                    foreach (var item in scopes)
                                    {
                                        identity.AddClaim(new Claim(EcommClaimTypes.Scope, item));
                                    }
                                }

                                // 2. Critical Fix: Map Keycloak roles to .NET identity claims
                                var realmAccessClaim = identity.FindFirst("realm_access");
                                if (realmAccessClaim is not null)
                                {
                                    using var doc = JsonDocument.Parse(realmAccessClaim.Value);
                                    if (doc.RootElement.TryGetProperty("roles", out var rolesElement))
                                    {
                                        foreach (var role in rolesElement.EnumerateArray())
                                        {
                                            identity.AddClaim(new Claim(EcommClaimTypes.Role, role.GetString() ?? ""));
                                        }
                                    }
                                }
                            }
                            return Task.CompletedTask;
                        }
                        //OnTokenValidated = context =>
                        //{
                        //    var identity = context.Principal?.Identity as ClaimsIdentity;
                        //    var scopeClaim = identity?.FindFirst(EcommClaimTypes.Scope);

                        //    if (scopeClaim is null)
                        //    {
                        //        return Task.CompletedTask;
                        //    }

                        //    var scopes = scopeClaim.Value.Split(' ');
                        //    identity?.RemoveClaim(scopeClaim);
                        //    //identity?.AddClaim((Claim)scopes.Select(scope => new Claim("scope", scope)));
                        //    foreach (var item in scopes)
                        //    {
                        //        identity?.AddClaim(new Claim(EcommClaimTypes.Scope, item));
                        //    }
                        //    return Task.CompletedTask;
                        //}
                    };
                });
            }
            else
            {
                // 2. MICROSOFT ENTRA ID CONFIGURATION (PRODUCTION)
                // Expects "AzureAd" section structure standard to Microsoft.Identity.Web
                services.AddMicrosoftIdentityWebApiAuthentication(configuration, "Security:AzureAd");
            }

            var authBuilder = services.AddAuthorizationBuilder();

            if (!isPublicService)
            {
                authBuilder
                    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                        .RequireClaim(EcommClaimTypes.Scope, apiAccessScope)
                        .Build())
                    .AddPolicy(Policies.AdminAccess, authBuilder => authBuilder
                        .RequireClaim(EcommClaimTypes.Scope, apiAccessScope)
                        .RequireRole(Roles.Admin));
            }                

            return services;
        }
    }
}
