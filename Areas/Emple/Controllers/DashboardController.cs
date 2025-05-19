using ControlMDBI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ControlMDBI.Areas.Emple.Controllers
{
    [Area("Emple")]
    [Authorize(Roles = "Empleado")]
    public class DashboardController : Controller
    {
        

  
        public async Task<IActionResult> Index()
        {
          

            return View();
        }


    }
}
