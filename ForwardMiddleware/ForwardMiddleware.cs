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
    using Microsoft.Extensions.Caching.Memory;

    public class ForwardMiddleware
    {
        /// <summary>
        /// The URL mapping(should read from cache).
        /// </summary>
        private static Dictionary<string, DownSettings> UrlMapping = new Dictionary<string, DownSettings>
        {
            {"/api/values", new DownSettings
                {
                    FullUrl = "http://localhost:9999/api/persons/data",
                    IsFree = false,
                    ChargeCode = "0",
                    ChargeCodeName = "code",
                    IsResultEncrypted = false,
                        
                }}
        };

        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ForwardMiddleware.ForwardMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next.</param>
        /// <param name="loggerFactory">Logger factory.</param>
        public ForwardMiddleware(RequestDelegate next,ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<ForwardMiddleware>();
        }

        /// <summary>
        /// Invoke the specified httpContext.
        /// </summary>
        /// <returns>The invoke.</returns>
        /// <param name="httpContext">Http context.</param>
        public async Task Invoke(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value;

            DownSettings downSettings = null;
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
            if(!string.IsNullOrWhiteSpace(queryString))
            {
                downSettings.FullUrl += $"?{queryString}";    
            }

            _logger.LogDebug($"forward request info => {downSettings.ToString()}");

            var requestMessage = new HttpRequestMessage();
            requestMessage.Method = new HttpMethod(httpContext.Request.Method);
            requestMessage.Content = await BuildHttpContentAsync(httpContext.Request);
            requestMessage.RequestUri = new Uri(downSettings.FullUrl);

            foreach (var header in httpContext.Request.Headers)
            {                
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
         
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var res = await client.SendAsync(requestMessage);
                    var str = await res.Content.ReadAsStringAsync();
                    _logger.LogDebug($"response res => {str}");

                    DoCharge(downSettings, str);

                    await httpContext.Response.WriteAsync(str);
                    return;
                }
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
        private void  DoCharge(DownSettings settings, string json)
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
                    _logger.LogError(ex,$"Do charge error");
                }
            }
        }
    }  
}
