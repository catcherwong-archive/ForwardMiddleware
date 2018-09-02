namespace ForwardMiddleware.Charge
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    
    public class DefaultChargeHandler : IChargeHandler
    {
        private readonly ILogger _logger;

        public DefaultChargeHandler(ILoggerFactory loggerFactory)
        {
            this._logger = loggerFactory.CreateLogger<DefaultChargeHandler>();
        }

        public bool IsNeedToCharged => true;

        public Task DoChargeAsync(ApiModel apiModel, string json)
        {
            if (!apiModel.IsFree
                && !string.IsNullOrWhiteSpace(apiModel.ChargeCode)
                && !string.IsNullOrWhiteSpace(apiModel.ChargeCodeName))
            {
                try
                {
                    var jObj = JObject.Parse(json);

                    if (jObj.ContainsKey(apiModel.ChargeCodeName))
                    {
                        var code = jObj[apiModel.ChargeCodeName].ToString();

                        //charge code may split by ,
                        var codes = apiModel.ChargeCode.Split(',');

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

            return Task.CompletedTask;
        }
    }
}
