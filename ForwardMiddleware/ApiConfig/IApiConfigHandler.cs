namespace ForwardMiddleware.ApiConfig
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// API config handler.
    /// </summary>
    public interface IApiConfigHandler
    {
        /// <summary>
        /// Gets the apis async.
        /// </summary>
        /// <returns>The apis async.</returns>
        Task<List<ApiModel>> GetApisAsync();
    }
}
