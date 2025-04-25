using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ControlMDBI.Areas.Vigi.Controllers
{
    [Area("Vigi")]
    [Authorize(Roles = "Vigilante")]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
