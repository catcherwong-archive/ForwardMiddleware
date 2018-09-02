namespace ForwardMiddleware.ApiConfig
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Default API config handler.
    /// </summary>
    public class DefaultApiConfigHandler : IApiConfigHandler
    {
        private readonly IConfiguration _configuration;

        public DefaultApiConfigHandler(IConfiguration configuration)
        {
            this._configuration = configuration;
        }

        /// <summary>
        /// Gets the apis async.
        /// </summary>
        /// <returns>The apis async.</returns>
        public Task<List<ApiModel>> GetApisAsync()
        {
            var apiModels = new List<ApiModel>();
            _configuration.GetSection("ApiModels").Bind(apiModels);
            return Task.FromResult(apiModels);
        }
    }
}
