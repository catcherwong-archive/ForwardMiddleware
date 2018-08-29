namespace ForwardMiddleware.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class Startup
    {
        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            LoggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; private set; }
        public ILoggerFactory LoggerFactory { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddForward(Configuration, LoggerFactory);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseForward();
        }

    }
}
