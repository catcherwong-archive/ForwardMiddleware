namespace ForwardMiddleware
{
    using System;
    using System.Net;
    using System.Net.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Polly;

    
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddForward(this IServiceCollection services, IConfiguration configuration, ILoggerFactory loggerFactory = null)
        {
            services.AddSingleton<ApiConfig.IApiConfigHandler, ApiConfig.DefaultApiConfigHandler>();
            services.AddSingleton<Charge.IChargeHandler, Charge.DefaultChargeHandler>();

            var apiExSettingsOptions = new ApiExSettingsOptions();
            configuration.GetSection("ApiExSettings").Bind(apiExSettingsOptions);
            var apiModels = apiExSettingsOptions.Settings;

            foreach (var item in apiModels)
            {
                services.AddHttpClient(item.Name)
                       //fallback
                       .AddPolicyHandler(Policy<HttpResponseMessage>.Handle<Exception>().FallbackAsync(new HttpResponseMessage
                       {
                           Content = new StringContent("fallback"),
                           StatusCode = HttpStatusCode.OK
                       }, async b =>
                       {
                           loggerFactory?.CreateLogger("ForwareMiddleware")?.LogWarning($"fallback here {b.Exception.Message}");
                           await System.Threading.Tasks.Task.CompletedTask;
                       }))
                       //circuit breaker
                       .AddPolicyHandler(Policy<HttpResponseMessage>.Handle<Exception>().CircuitBreakerAsync(item.ExceptionsAllowedBeforeBreaking, TimeSpan.FromSeconds(item.DurationOfBreak), (ex, ts) =>
                       {
                           loggerFactory?.CreateLogger("ForwareMiddleware")?.LogWarning($"break here {ts.TotalMilliseconds}");
                       }, () =>
                       {
                           loggerFactory?.CreateLogger("ForwareMiddleware")?.LogWarning($"reset here ");
                       }))
                       //timeout
                       .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(item.TimeOut));
            }

            return services;
        } 
    }
}
