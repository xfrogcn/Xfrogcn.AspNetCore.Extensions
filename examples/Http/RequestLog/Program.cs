using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

namespace RequestLog
{
    class Program
    {
        static async Task Main(string[] args)
        {
            TestWebApplicationFactory<ApiServer.Startup> factory = new TestWebApplicationFactory<ApiServer.Startup>();
            
            var client = factory.CreateClient();
            var response =await client.GetAsync<List<ApiServer.WeatherForecast>>("/WeatherForecast");

            Console.ReadLine();
        }
    }
}
