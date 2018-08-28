namespace ForwardMiddleware.Web
{
    using System.Collections.Generic;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .ConfigureServices(s =>
                    {                        
                        s.AddForward(ApiModel.GetApiModels());
                    })
                   .Configure(app =>
                    {
                        app.UseForward();
                    });
    }
}
