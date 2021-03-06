﻿namespace ForwardMiddleware
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Charge;
    using ApiConfig;

    public class ForwardMiddleware
    {        
        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;
        private readonly IChargeHandler _chargeHandler;
        private readonly IApiConfigHandler _apiConfigHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ForwardMiddleware.ForwardMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="clientFactory">Client factory.</param>
        /// <param name="chargeHandler">Charge handler.</param>
        /// <param name="apiConfigHandler">API config handler.</param>
        public ForwardMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IHttpClientFactory clientFactory,IChargeHandler chargeHandler,IApiConfigHandler apiConfigHandler)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ForwardMiddleware>();
            _clientFactory = clientFactory;
            _chargeHandler = chargeHandler;
            _apiConfigHandler = apiConfigHandler;
        }

        /// <summary>
        /// Invoke the specified httpContext.
        /// </summary>
        /// <returns>The invoke.</returns>
        /// <param name="httpContext">Http context.</param>
        public async Task Invoke(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value;

            var apiModels = await _apiConfigHandler.GetApisAsync();

            var apiModel = apiModels.FirstOrDefault(x => x.RelatePath.Equals(path, StringComparison.InvariantCultureIgnoreCase));

            if (apiModel==null)
            {
                await httpContext.Response.WriteAsync("Not support url");
                return;
            }

            var queryString = httpContext.Request.QueryString.Value;
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                apiModel.FullPath += $"?{queryString}";
            }

            _logger.LogDebug($"forward request info => {apiModel.ToString()}");

            var requestMessage = new HttpRequestMessage
            {
                Method = new HttpMethod(httpContext.Request.Method),
                Content = await BuildHttpContentAsync(httpContext.Request),
                RequestUri = new Uri(apiModel.FullPath)
            };

            foreach (var header in httpContext.Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            try
            {
                var client = _clientFactory.CreateClient(apiModel.ApiExSettingName);
                var res = await client.SendAsync(requestMessage);
                var str = await res.Content.ReadAsStringAsync();
                _logger.LogDebug($"response res => {str}");

                if (_chargeHandler.IsNeedToCharged) await _chargeHandler.DoChargeAsync(apiModel, str);
                    
                await httpContext.Response.WriteAsync(str);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"request error.");
                await httpContext.Response.WriteAsync(Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    code = 500,
                    msg = ex.Message
                }));
                return;
            }
        }

        /// <summary>
        /// Builds the http content async.
        /// </summary>
        /// <returns>The http content async.</returns>
        /// <param name="request">Request.</param>
        private async Task<HttpContent> BuildHttpContentAsync(HttpRequest request)
        {
            if (request.Body == null || (request.Body.CanSeek && request.Body.Length <= 0))
            {
                return null;
            }

            var content = new ByteArrayContent(await ToByteArrayAsync(request.Body));

            if (!string.IsNullOrEmpty(request.ContentType))
            {
                content.Headers
                    .TryAddWithoutValidation("Content-Type", new[] { request.ContentType });
            }

            return content;
        }


        /// <summary>
        /// Tos the byte array async.
        /// </summary>
        /// <returns>The byte array async.</returns>
        /// <param name="stream">Stream.</param>
        private async Task<byte[]> ToByteArrayAsync(Stream stream)
        {
            using (stream)
            {
                using (var memStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memStream);
                    return memStream.ToArray();
                }
            }
        }             
    }
}
