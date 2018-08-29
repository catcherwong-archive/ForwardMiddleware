using System.Collections.Generic;

namespace ForwardMiddleware
{
    public class ApiExSettings
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <remarks>
        /// Use to create HttpClient instance
        /// </remarks>
        /// <value>The name.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the time out.
        /// </summary>
        /// <value>The time out.</value>
        public int TimeOut { get; set; }

        /// <summary>
        /// Gets or sets the exceptions allowed before breaking.
        /// </summary>
        /// <value>The exceptions allowed before breaking.</value>
        public int ExceptionsAllowedBeforeBreaking { get; set; }

        /// <summary>
        /// Gets or sets the duration of break.
        /// </summary>
        /// <value>The duration of break.</value>
        public int DurationOfBreak { get; set; }
    }

    /// <summary>
    /// API ex settings options.
    /// </summary>
    public class ApiExSettingsOptions
    {
        public List<ApiExSettings> Settings { get; set; }
    }
}
