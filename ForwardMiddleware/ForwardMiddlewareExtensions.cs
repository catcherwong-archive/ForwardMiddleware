namespace ForwardMiddleware
{    
    using Microsoft.AspNetCore.Builder;

    public static class ForwardMiddlewareExtensions
    {
        public static IApplicationBuilder UseForward(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ForwardMiddleware>();
        }
    }
}
