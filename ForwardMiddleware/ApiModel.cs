namespace ForwardMiddleware
{    
    using Newtonsoft.Json;
    
    public class ApiModel
    {
        /// <summary>
        /// Gets or sets the name of the APIExSetting.
        /// </summary>
        /// <value>The name of the API ex setting.</value>
        public string ApiExSettingName { get; set; }

        /// <summary>
        /// Gets or sets the path.
        /// </summary>
        /// <value>The path.</value>
        public string FullPath { get; set; }

        /// <summary>
        /// Gets or sets the relate path.
        /// </summary>
        /// <value>The relate path.</value>
        public string RelatePath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:ForwardMiddleware.ApiModel"/> is free.
        /// </summary>
        /// <value><c>true</c> if is free; otherwise, <c>false</c>.</value>
        public bool IsFree { get; set; }

        /// <summary>
        /// Gets or sets the charge code.
        /// </summary>
        /// <value>The charge code.</value>
        public string ChargeCode { get; set; }

        /// <summary>
        /// Gets or sets the name of the charge code.
        /// </summary>
        /// <value>The name of the charge code.</value>
        public string ChargeCodeName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:ForwardMiddleware.ApiModel"/> is result encrypted.
        /// </summary>
        /// <value><c>true</c> if is result encrypted; otherwise, <c>false</c>.</value>
        public bool IsResultEncrypted { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:ForwardMiddleware.ApiModel"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:ForwardMiddleware.ApiModel"/>.</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
