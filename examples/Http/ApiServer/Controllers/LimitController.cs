using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiServer.Controllers
{
    [ApiController]
    [Route("{controller}")]
    [Authorize(AuthenticationSchemes = "BasicAuthentication")]
    public class LimitController : ControllerBase
    {

        public string Test()
        {
            return User.Identity.Name;
        }
    }
}
