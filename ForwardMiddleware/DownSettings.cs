namespace ForwardMiddleware
{
    using Newtonsoft.Json;

    public class DownSettings
    {
        public string FullUrl { get; set; }

        public bool IsFree { get; set; }

        public string ChargeCode { get; set; }

        public string ChargeCodeName { get; set; }

        public bool IsResultEncrypted { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
