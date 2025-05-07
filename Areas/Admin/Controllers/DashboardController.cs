using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlMDBI.Models;
using ControlMDBI.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ControlMDBI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class DashboardController : Controller
    {
        private readonly ControlMDBIDbContext _context;

        public DashboardController(ControlMDBIDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Total de usuarios
            int totalUsuarios = await _context.Usuario.CountAsync();

            // Usuarios activos (que tienen un empleado activo)
            int usuariosActivos = await _context.Usuario
                .Include(u => u.Empleado)
                .CountAsync(u => u.Empleado != null && u.Empleado.Activo);

            // Total de empleados
            int totalEmpleados = await _context.Empleado.CountAsync();

            // Total de empleados activos
            int empleadosActivos = await _context.Empleado.CountAsync(e => e.Activo);

            // Sedes con conteo de empleados
            var sedesConEmpleados = await _context.Sede
                .Select(s => new {
                    s.IdSede,
                    s.Nombre,
                    TotalEmpleados = s.Empleados.Count,
                    EmpleadosActivos = s.Empleados.Count(e => e.Activo)
                })
                .ToListAsync();

            // Crear el modelo para la vista
            var dashboardVM = new
            {
                TotalUsuarios = totalUsuarios,
                UsuariosActivos = usuariosActivos,
                TotalEmpleados = totalEmpleados,
                EmpleadosActivos = empleadosActivos,
                SedesEmpleados = sedesConEmpleados
            };

            return View(dashboardVM);
        }
    }
}