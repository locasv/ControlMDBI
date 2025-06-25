using ControlMDBI.Areas.Admin.ViewModels;
using ControlMDBI.Data;
using ControlMDBI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ControlMDBI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class RegistrosPersonasController : Controller
    {
        private readonly ControlMDBIDbContext _context;

        public RegistrosPersonasController(ControlMDBIDbContext context)
        {
            _context = context;
        }

        // GET: Create
        public IActionResult Create()
        {

            return View();
        }

        // POST: Create Empleado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmpleado(RegistroPersonaViewModel model)
        {
            // Quita validaciones del modelo de visitante
            ModelState.Remove(nameof(model.DniVisitante));
            ModelState.Remove(nameof(model.RazonVisita));
            if (!string.IsNullOrEmpty(model.DniEmpleado))
            {
                var empleado = await _context.Empleado
                    .FirstOrDefaultAsync(e => e.DNI == model.DniEmpleado);

                if (empleado == null)
                {
                    TempData["Error"] = "No se encontró un empleado con el DNI proporcionado.";
                    return RedirectToAction("Index", "Registros");
                }


                var registroEmpleado = new RegistroEmpleado
                {
                    IdEmpleado = empleado.IdEmpleado,
                    FechaRegistroEmpleado = DateTime.Now,
                    TipoRegistro = model.TipoRegistroEmpleado,
                    Razon = model.RazonEmpleado,
                    Objeto = model.PortaObjeto,
                    CodigoObjeto = model.CodigoObjeto,
                    Observaciones = model.ObservacionesEmpleado,

                    IdUsuario = GetCurrentUserId(),
                };

                if (ModelState.IsValid)
                {
                    _context.RegistroEmpleado.Add(registroEmpleado);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Registro de empleado creado exitosamente.";
                    return RedirectToAction("Index", "Registros");
                }

            }


            return View("Create", model);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarEmpleado(string dni)
        {
            if (string.IsNullOrEmpty(dni) || dni.Length != 8)
            {
                return Json(new { success = false, message = "DNI debe tener 8 dígitos" });
            }

            // Buscar solo empleados activos
            var empleado = await _context.Empleado.Include(empleado => empleado.Sede)
                .FirstOrDefaultAsync(e => e.DNI == dni && e.Activo == true); // Ajusta según tu campo de estado

            if (empleado == null)
            {
                return Json(new
                {
                    success = false,
                    message = "No se encontró empleado activo con este DNI",
                    dniNoEncontrado = dni
                });
            }
            //return Json(new
            //{
            //    success = false,
            //    message = "No se encontró empleado activo con este DNI. Se ha cambiado a la pestaña de visitante automáticamente.",
            //    dniNoEncontrado = dni // Enviamos el DNI para usarlo en el visitante
            //});



            return Json(new
            {
                success = true,
                empleado = new
                {
                    nombre = empleado.Nombres,
                    apellido = empleado.Apellidos,
                    cargo = empleado.Cargo,
                    area = empleado.Unidad,
                    estado = empleado.Activo ? "Activo" : "Inactivo",
                    sede = empleado.Sede?.Nombre
                }
            });
        }

        // POST: Create Visitante
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVisitante(RegistroPersonaViewModel model)
        {
            // Quita validaciones del modelo de empleado
            ModelState.Remove(nameof(model.DniEmpleado));
            ModelState.Remove(nameof(model.TipoRegistroEmpleado));
            ModelState.Remove(nameof(model.RazonEmpleado));
            ModelState.Remove(nameof(model.PortaObjeto));
            ModelState.Remove(nameof(model.CodigoObjeto));
            ModelState.Remove(nameof(model.ObservacionesEmpleado));

            var registroVisitante = new RegistroVisitante
            {
                IdUsuario = GetCurrentUserId(),
                FechaRegistroVisitante = DateTime.Now,
                DNI = model.DniVisitante,
                RazonVisita = model.RazonVisita
            };

            if (ModelState.IsValid)
            {
                _context.RegistroVisitante.Add(registroVisitante);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Registro de visitante creado exitosamente.";
                return RedirectToAction("Index", "Registros");
            }

            return View("Create");
        }



        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("IdUsuario");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }

            throw new Exception("No se pudo obtener el ID del usuario actual.");


        }
    }
    }
