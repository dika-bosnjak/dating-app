
using API.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //base template controller (use ApiController with api/[controllerName])
    [ServiceFilter(typeof(LogUserActivity))]
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {

    }
}