using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ForwardMiddleware
{
    public class ApiModel
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public int TimeOut { get; set; }

        public int ExceptionsAllowedBeforeBreaking { get; set; }

        public int DurationOfBreak { get; set; }

        public bool IsFree { get; set; }

        public string ChargeCode { get; set; }

        public string ChargeCodeName { get; set; }

        public bool IsResultEncrypted { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static List<ApiModel> GetApiModels()
        {
            return new List<ApiModel>
            {
                new ApiModel
                {
                    Name = "Test_A",
                    Path = "http://localhost:9999/api/persons/data",
                    IsFree = false,
                    ChargeCode = "0",
                    ChargeCodeName = "code",
                    IsResultEncrypted = false,
                    TimeOut = 3,
                    DurationOfBreak = 3,
                    ExceptionsAllowedBeforeBreaking = 2,
                },
                new ApiModel
                {
                    Name = "Test_B",
                    Path = "http://localhost:9999/api/persons/data",
                    IsFree = false,
                    ChargeCode = "0",
                    ChargeCodeName = "code",
                    IsResultEncrypted = false,
                    TimeOut = 3,
                    DurationOfBreak = 3,
                    ExceptionsAllowedBeforeBreaking = 2,
                }
            };
        }
    }
}
