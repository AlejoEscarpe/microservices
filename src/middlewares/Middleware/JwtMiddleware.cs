using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using System.Threading.Tasks;

namespace Middleware;

public class JwtMiddleware(IJwtBuilder jwtBuilder) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Get the token from the Authorization header
        var bearer = context.Request.Headers["Authorization"].ToString();
        var token = string.Empty;
        if (!string.IsNullOrWhiteSpace(bearer) && bearer.StartsWith("Bearer "))
        {
            token = bearer.Substring("Bearer ".Length).Trim();
        }

        if (string.IsNullOrEmpty(token))
        {
            // No token provided - short-circuit with 401
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.CompleteAsync();
            return;
        }

        // Verify the token using the IJwtBuilder
        var userId = jwtBuilder.ValidateToken(token);

        if (ObjectId.TryParse(userId, out _))
        {
            // Store the userId in the HttpContext items for later use
            context.Items["userId"] = userId;
            // Continue processing the request
            await next(context);
            return;
        }

        // If token or userId are invalid, send 401 Unauthorized and do not continue pipeline
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        await context.Response.CompleteAsync();
    }
}
