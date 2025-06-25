// RegistrosVehiculosController.cs - Controlador para vehículos
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlMDBI.Data;
using ControlMDBI.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ControlMDBI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class RegistrosVehiculosController : Controller
    {
        private readonly ControlMDBIDbContext _context;

        public RegistrosVehiculosController(ControlMDBIDbContext context)
        {
            _context = context;
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            var aprobaciones = await _context.AprobacionVehiculo
                .Include(a => a.SolicitudVehiculo)
                 .ThenInclude(s => s.Vehiculo)
                .Include(u => u.Usuario)
                 .ThenInclude(u => u.Empleado)
                .Where(a => a.Estado == "Aprobado")
                .Select(a => new SelectListItem
                {
                    Value = a.IdAprobacionVehiculo.ToString(),
                    Text = $"{a.SolicitudVehiculo.Vehiculo.Placa} "//- {a.SolicitudVehiculo.Vehiculo.Sede.Nombre}
                })
                .ToListAsync();

            ViewBag.AprobacionesVehiculos = aprobaciones;
            ViewBag.TiposRegistro = new SelectList(Enum.GetValues(typeof(ListaTipoRegistroVehiculo)));

            return View();
        }
        // API para buscar vehículo por placa
        [HttpGet]
        public async Task<IActionResult> BuscarVehiculoPorPlaca(string placa)
        {
            if (string.IsNullOrEmpty(placa))
            {
                return Json(new { success = false, message = "Placa no proporcionada" });
            }

            try
            {
                placa = placa.ToUpper();
                var vehiculoAprobado = await _context.AprobacionVehiculo
                    .Include(a => a.SolicitudVehiculo)
                     .ThenInclude(s => s.Vehiculo)
                     .ThenInclude(v => v.Sede)
                    .Include(i => i.SolicitudVehiculo)
                     .ThenInclude(o => o.Usuario)
                     .ThenInclude(u => u.Empleado)
                    .Include(a => a.Usuario)
                     .ThenInclude(u => u.Empleado)
                     .ThenInclude(s => s.Sede)
                    .FirstOrDefaultAsync(a => a.Estado == "Aprobado" &&
                                           a.SolicitudVehiculo.Vehiculo.Placa.ToUpper() == placa);
                //Recatamos al usuario que esta usando el sistema
                var usuarioActual = _context.Usuario
                                .Include(u => u.Empleado)
                                .FirstOrDefault(u => u.NombreUsuario == User.Identity.Name);

                if (vehiculoAprobado != null)
                {
                    return Json(new
                    {
                        success = true,
                        vehiculo = new
                        {
                            id = vehiculoAprobado.IdAprobacionVehiculo,
                            aprobador = vehiculoAprobado.Usuario.Empleado.Nombres + " " + vehiculoAprobado.Usuario.Empleado.Apellidos,
                            solicitante = vehiculoAprobado.SolicitudVehiculo.Usuario.Empleado.Nombres + " " + vehiculoAprobado.SolicitudVehiculo.Usuario.Empleado.Apellidos,
                            descripcion = $"{vehiculoAprobado.SolicitudVehiculo.Vehiculo.Placa} - {vehiculoAprobado.SolicitudVehiculo.Vehiculo.Marca} - Sede: {vehiculoAprobado.SolicitudVehiculo.Vehiculo.Sede.Nombre}",
                            placa = vehiculoAprobado.SolicitudVehiculo?.Vehiculo?.Placa,
                            marca = vehiculoAprobado.SolicitudVehiculo?.Vehiculo?.Marca,
                            estado = vehiculoAprobado.SolicitudVehiculo?.Vehiculo?.Estado,
                            sede = vehiculoAprobado.SolicitudVehiculo?.Vehiculo?.Sede?.Nombre,
                            vigilante = usuarioActual.Empleado.Nombres + " " + usuarioActual.Empleado.Apellidos,


                        }
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Vehículo no encontrado o no aprobado" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al buscar el vehículo: " + ex.Message });
            }
        }


        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegistroVehiculo registroVehiculo)
        {
            registroVehiculo.IdUsuario = GetCurrentUserId();
            registroVehiculo.FechaRegistroVehiculo = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.RegistroVehiculo.Add(registroVehiculo);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Registro de vehículo creado exitosamente.";
                return RedirectToAction("Index", "Registros");
            }

            // Recargar datos para la vista en caso de error
            var aprobaciones = await _context.AprobacionVehiculo
                .Include(a => a.SolicitudVehiculo)
                .ThenInclude(s => s.Vehiculo)
                .Where(a => a.Estado == "Aprobado")
                .Select(a => new SelectListItem
                {
                    Value = a.IdAprobacionVehiculo.ToString(),
                    Text = $"{a.SolicitudVehiculo.Vehiculo.Placa} "//- {a.SolicitudVehiculo.Vehiculo.Sede.Nombre}
                })
                .ToListAsync();

            ViewBag.AprobacionesVehiculos = aprobaciones;
            ViewBag.TiposRegistro = new SelectList(Enum.GetValues(typeof(ListaTipoRegistroVehiculo)));

            return View(registroVehiculo);
        }

        private int GetCurrentUserId()
        {

            var userDB = _context.Usuario
                 .Include(u => u.Empleado) // Cargar la relación con Empleado
                 .FirstOrDefault(u => u.Empleado.Activo == true);
            var claim = new Claim("IdUsuario", userDB.IdUsuario.ToString());

            return userDB.IdUsuario;

        }
    }
}