using Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;

namespace Api.Minimal.Extensions;

public static partial class AAAAuthenticationExtensions
{
    public static WebApplicationBuilder AddAAAAuthentication(this WebApplicationBuilder builder, IConfiguration config)
    {
        builder
            .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = config["AppSettings:OidcConfiguration:Url"];
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidTypes = ["at+jwt"];
            });
        ;
        builder.Services.AddAuthorization(options =>
        {
            // Define policies for roles
            options.AddPolicy(
                Policy.AUTH,
                policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", config["AppSettings:OidcConfiguration:Scope"] ?? "");
                }
            );
            options.AddPolicy(Policy.ADMIN, policy => policy.RequireRole("ADMINr"));
        });

        return builder;
    }

    public static IApplicationBuilder UseAAAAuthentication(this IApplicationBuilder applicationBuilder)
    {
        applicationBuilder
            .UseAuthentication()
            .UseAuthorization()
            .UseForwardedHeaders(
                new ForwardedHeadersOptions
                {
                    RequireHeaderSymmetry = false,
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                }
            );

        return applicationBuilder;
    }
}
