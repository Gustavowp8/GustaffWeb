using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GustaffWeb.Controllers
{
    [Authorize]
    public class InicioController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
