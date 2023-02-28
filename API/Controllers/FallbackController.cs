using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    //Fallback Controller is used to redirect unknown requests to angular app (wwwroot) folder
    public class FallbackController : Controller
    {
        public ActionResult Index()
        {
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), "text/HTML");
        }
    }
}