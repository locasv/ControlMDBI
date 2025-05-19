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

            // Total de vehículos
            int totalVehiculos = await _context.Vehiculo.CountAsync();

            // Estado de Vehículos (activos, inactivos y mantenimiento)
            int vehiculosActivos = await _context.Vehiculo.CountAsync(v => v.Estado == "Activo");
            int vehiculosInactivos = await _context.Vehiculo.CountAsync(v => v.Estado == "Inactivo");
            int vehiculosMantenimiento = await _context.Vehiculo.CountAsync(v => v.Estado == "Mantenimiento");


            // Usuarios activos (que tienen un empleado activo)
            int usuariosActivos = await _context.Usuario
                .Include(u => u.Empleado)
                .CountAsync(u => u.Empleado != null && u.Empleado.Activo);

            // Estado de Vehiculos (activos, inactivos y mantenimiento)
            


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
                SedesEmpleados = sedesConEmpleados,
                TotalVehiculos = totalVehiculos,
                VehiculosActivos = vehiculosActivos,
                VehiculosInactivos = vehiculosInactivos,
                VehiculosMantenimiento = vehiculosMantenimiento
            };

            return View(dashboardVM);
        }
    }
}