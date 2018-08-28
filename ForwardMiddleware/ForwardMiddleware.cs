namespace ForwardMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json.Linq;
    using Microsoft.Extensions.Logging;

    public class ForwardMiddleware
    {
        /// <summary>
        /// The URL mapping(should read from cache).
        /// </summary>
        private static Dictionary<string, ApiModel> UrlMapping = new Dictionary<string, ApiModel>
        {
            {"/api/values", ApiModel.GetApiModels()[0]},
            {"/api/val", ApiModel.GetApiModels()[1]},
        };

        private readonly RequestDelegate _next;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ForwardMiddleware.ForwardMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        /// <param name="clientFactory">Client factory.</param>
        public ForwardMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, IHttpClientFactory clientFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ForwardMiddleware>();
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// Invoke the specified httpContext.
        /// </summary>
        /// <returns>The invoke.</returns>
        /// <param name="httpContext">Http context.</param>
        public async Task Invoke(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value;

            ApiModel downSettings = null;
            if (UrlMapping.ContainsKey(path))
            {
                downSettings = UrlMapping[path];
            }
            else
            {
                await httpContext.Response.WriteAsync("Not support url");
                return;
            }

            var queryString = httpContext.Request.QueryString.Value;
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                downSettings.Path += $"?{queryString}";
            }

            _logger.LogDebug($"forward request info => {downSettings.ToString()}");

            var requestMessage = new HttpRequestMessage
            {
                Method = new HttpMethod(httpContext.Request.Method),
                Content = await BuildHttpContentAsync(httpContext.Request),
                RequestUri = new Uri(downSettings.Path)
            };

            foreach (var header in httpContext.Request.Headers)
            {
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }

            try
            {
                var client = _clientFactory.CreateClient(downSettings.Name);
                var res = await client.SendAsync(requestMessage);
                var str = await res.Content.ReadAsStringAsync();
                _logger.LogDebug($"response res => {str}");

                DoCharge(downSettings, str);

                await httpContext.Response.WriteAsync(str);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"request error.");
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

        /// <summary>
        /// Dos the charge.
        /// </summary>
        /// <param name="settings">Settings.</param>
        /// <param name="json">Json.</param>
        private void DoCharge(ApiModel settings, string json)
        {
            if (!settings.IsFree
                && !string.IsNullOrWhiteSpace(settings.ChargeCode)
                && !string.IsNullOrWhiteSpace(settings.ChargeCodeName))
            {
                try
                {
                    var jObj = JObject.Parse(json);

                    if (jObj.ContainsKey(settings.ChargeCodeName))
                    {
                        var code = jObj[settings.ChargeCodeName].ToString();

                        //charge code may split by ,
                        var codes = settings.ChargeCode.Split(',');

                        if (codes.Contains(code))
                        {
                            //need to charge
                            //send msg
                            _logger.LogDebug($"this request need to be charged => code = {code}");
                        }

                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, $"Do charge error");
                }
            }
        }
    }
}
