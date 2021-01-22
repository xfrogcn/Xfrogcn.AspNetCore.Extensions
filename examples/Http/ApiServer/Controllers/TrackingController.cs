using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiServer.Controllers
{
    [ApiController]
    [Route("{controller}")]
    public class TrackingController : ControllerBase
    {
        private readonly ILogger<TrackingController> _logger;
        private readonly IHttpClientFactory _httpFactory;

        public TrackingController(
            ILogger<TrackingController> logger,
            IHttpClientFactory httpFactory)
        {
            _logger = logger;
            _httpFactory = httpFactory;
        }

        [HttpGet("serviceA")]
        public async Task<string> ServiceA()
        {
            _logger.LogWarning("ServiceA x-request-id: {requestId}", HttpContext.Request.Headers["x-request-id"]);
            var client = _httpFactory.CreateClient();
            var response = await client.GetAsync<string>("/tracking/serviceB");
            return $"ServiceA --> {response}";
        }

        [HttpGet("serviceB")]
        public string ServiceB()
        {
            _logger.LogWarning("ServiceB x-request-id: {requestId}", HttpContext.Request.Headers["x-request-id"]);
            return "ServiceB";
        }
    }
}
