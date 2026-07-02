using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using MongoDB.Driver;
using System.Text;

namespace Middleware;

public static class Extentions
{
    public static void AddMongoDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoOptions>(configuration.GetSection("mongo"));
        services.AddSingleton(c =>
        {
            var options = c.GetService<IOptions<MongoOptions>>();

            return new MongoClient(options.Value.ConnectionString);
        });
        services.AddSingleton(c =>
        {
            var options = c.GetService<IOptions<MongoOptions>>();
            var client = c.GetService<MongoClient>();

            return client.GetDatabase(options.Value.Database);
        });
    }

    public static void AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new JwtOptions();
        var section = configuration.GetSection("jwt");
        section.Bind(options);

        // Prefer environment variable for secret, fallback to configured secret
        var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        var secret = !string.IsNullOrWhiteSpace(envSecret) ? envSecret : options?.Secret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("JWT secret is not configured. Set JWT_SECRET environment variable or configure jwt:secret in configuration.");
        }

        // Ensure configured options reflect resolved secret
        services.Configure<JwtOptions>(opts =>
        {
            opts.Secret = secret;
            opts.ExpiryMinutes = options?.ExpiryMinutes ?? 60;
        });

        services.AddSingleton<IJwtBuilder, JwtBuilder>();
        services.AddAuthentication()
            .AddJwtBearer(x =>
            {
                // Require HTTPS metadata to avoid accepting tokens over insecure channels
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            });
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection("jwt");
        var options = section.Get<JwtOptions>();
        section.Bind(options);

        // Prefer environment variable for secret, fallback to configured secret
        var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        var secret = !string.IsNullOrWhiteSpace(envSecret) ? envSecret : options?.Secret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("JWT secret is not configured. Set JWT_SECRET environment variable or configure jwt:secret in configuration.");
        }

        services.Configure<JwtOptions>(opts =>
        {
            opts.Secret = secret;
            opts.ExpiryMinutes = options?.ExpiryMinutes ?? 60;
        });

        var key = Encoding.UTF8.GetBytes(secret);

        services.AddSingleton<IJwtBuilder, JwtBuilder>();
        services.AddTransient<JwtMiddleware>();

        services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                // Require HTTPS to avoid token exposure over insecure channels
                x.RequireHttpsMetadata = true;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    // Encourage validation of issuer/audience in production by configuration
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true
                };
            });

        services.AddAuthorization(x =>
        {
            x.DefaultPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                .RequireAuthenticatedUser()
                .Build();
        });
    }
}