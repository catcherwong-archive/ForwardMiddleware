using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace ForwardMiddleware
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddForward(this IServiceCollection services, List<ApiModel> apiModels)
        {
            var fallbackResponse = new HttpResponseMessage();
            fallbackResponse.Content = new StringContent("fallback");
            fallbackResponse.StatusCode = HttpStatusCode.OK;

            foreach (var item in apiModels)
            {
                services.AddHttpClient(item.Name)
                        //fallback
                        .AddPolicyHandler(Policy<HttpResponseMessage>.Handle<Exception>().FallbackAsync(fallbackResponse, async b =>
                        {
                           //Logger.LogWarning($"fallback here {b.Exception.Message}");
                        }))
                        //circuit breaker
                        .AddPolicyHandler(Policy<HttpResponseMessage>.Handle<Exception>().CircuitBreakerAsync(item.ExceptionsAllowedBeforeBreaking, TimeSpan.FromSeconds(item.DurationOfBreak), (ex, ts) =>
                        {
                            //Logger.LogWarning($"break here {ts.TotalMilliseconds}");
                        }, () =>
                        {
                            //Logger.LogWarning($"reset here ");
                        }))
                        //timeout
                        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(item.TimeOut));
            }

            return services;
        } 
    }
}
