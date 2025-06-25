using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ControlMDBI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ControlMDBI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class RegistrosController : Controller
    {
        private readonly ControlMDBIDbContext _context;

        public RegistrosController(ControlMDBIDbContext context)
        {
            _context = context;
        }

        // GET: Registros
        public async Task<ActionResult> Index()
        {
            var registrosPersonas = await _context.RegistroEmpleado
                .Include(r => r.Empleado)
                .Include(r => r.Usuario).ThenInclude(u => u.Empleado)
                .Where(r => r.FechaRegistroEmpleado.Date == DateTime.Today)
                .OrderByDescending(r => r.FechaRegistroEmpleado)
                .Take(10)
                .ToListAsync();

            var registrosVisitantes = await _context.RegistroVisitante
                .Include(r => r.Usuario).ThenInclude(u => u.Empleado)
                .Where(r => r.FechaRegistroVisitante.Date == DateTime.Today)
                .OrderByDescending(r => r.FechaRegistroVisitante)
                .Take(10)
                .ToListAsync();

            var registrosVehiculos = await _context.RegistroVehiculo
                .Include(r => r.AprobacionVehiculo)
                .ThenInclude(a => a.SolicitudVehiculo)
                .ThenInclude(s => s.Vehiculo)
                .Include(r => r.Usuario).ThenInclude(u => u.Empleado)
                .Where(r => r.FechaRegistroVehiculo.Date == DateTime.Today)
                .OrderByDescending(r => r.FechaRegistroVehiculo)
                .Take(10)
                .ToListAsync();

            ViewBag.RegistrosPersonas = registrosPersonas;
            ViewBag.RegistrosVisitantes = registrosVisitantes;
            ViewBag.RegistrosVehiculos = registrosVehiculos;

            return View();
        }
    }
}
