using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Xunit;

namespace Extensions.Tests
{
    [Trait("", "WebApiHost")]
    public class WebApiHostExtensionsTest
    {
        [Fact(DisplayName = "Log")]
        public void Test1()
        {
            var host = WebHost.CreateDefaultBuilder()
                .UseExtensions(null)
                .ConfigureLogging(logBuilder =>
                {
                    logBuilder.AddTestLogger();
                })
                .UseStartup<Startup>()
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<WebApiHostExtensionsTest>>();
            int count = 10;
            using (LogContext.PushProperty("aaa", "BBB"))
            {
                string log = new string('a', 1024 * 1024);

                using (var scope = logger.BeginScope("this is a {scope}", "scope"))
                {
                    for (int i = 0; i < count; i++)
                    {
                        logger.LogInformation(log);
                    }
                }
            }

            var logContent = host.Services.GetTestLogContent();
            Assert.Equal(count, logContent.LogContents.Count);
        }
    }

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
        }
    }
}
