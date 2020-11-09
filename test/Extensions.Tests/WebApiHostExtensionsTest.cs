using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Serilog.Context;
using Serilog.Events;

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
                .UseStartup<Startup>()
                .ConfigureLogging(logBuilder =>
                {
                    
                })
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<WebApiHostExtensionsTest>>();
            using (LogContext.PushProperty("aaa", "BBB"))
            {
                string log = new string('a', 1024 * 1024);
                using (var scope = logger.BeginScope("this is a {scope}", "scope"))
                {
                    for (int i = 0; i < 110; i++)
                    {
                        logger.LogInformation(log);
                    }
                }
            }
        
            
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
