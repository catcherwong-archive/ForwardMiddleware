namespace ForwardMiddleware
{    
    using Microsoft.AspNetCore.Builder;

    public static class ForwardMiddlewareExtensions
    {
        /// <summary>
        /// Uses the forward.
        /// </summary>
        /// <returns>The forward.</returns>
        /// <param name="builder">Builder.</param>
        public static IApplicationBuilder UseForward(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ForwardMiddleware>();
        }
    }
}
