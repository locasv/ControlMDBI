using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlMDBI.Data;
using ControlMDBI.Models;
using Microsoft.AspNetCore.Authorization;


namespace ControlMDBI.Areas.Emple.Controllers
{
    [Area("Emple")]
    [Authorize(Roles = "Empleado")]
    public class SolicitudVehiculosController : Controller
    {
        private readonly ControlMDBIDbContext _context;

        public SolicitudVehiculosController(ControlMDBIDbContext context)
        {
            _context = context;
        }

        // GET: Emple/SolicitudVehiculos
        public async Task<IActionResult> Index()
        {
            var usuarioActual = _context.Usuario
        .FirstOrDefault(u => u.NombreUsuario == User.Identity.Name);

            if (usuarioActual != null)
            {
                ViewBag.IdUsuarioActual = usuarioActual.IdUsuario;
            }

            var solicitudes = _context.SolicitudVehiculo
        .Include(s => s.Usuario)
            .ThenInclude(u => u.Empleado)
        .Include(s => s.Vehiculo);

     

            return View(await solicitudes.ToListAsync());
        }

        // GET: Emple/SolicitudVehiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudVehiculo = await _context.SolicitudVehiculo
                .Include(s => s.Usuario)
                .ThenInclude(u => u.Empleado)
                .Include(s => s.Vehiculo)
                .FirstOrDefaultAsync(m => m.IdSolicitudVehiculo == id);

         

            if (solicitudVehiculo == null)
            {
                return NotFound();
            }

            return View(solicitudVehiculo);
        }

        // GET: Emple/SolicitudVehiculos/Create
        public IActionResult Create()
        {

            var usuarioActual = _context.Usuario
                                .Include(u => u.Empleado)
                                .FirstOrDefault(u => u.NombreUsuario == User.Identity.Name);

            if (usuarioActual == null)
            {
                return Unauthorized(); // o redirigir a login
            }
            if (usuarioActual.Empleado == null)
            {
                // Puedes redirigir o mostrar un mensaje de error más claro
                TempData["ErrorMessage"] = "Este usuario no tiene un empleado asociado. Contacte al administrador.";
                return RedirectToAction("Index");
            }
            var modelo = new SolicitudVehiculo()
            {
                FechaSalida = DateTime.Now,
                FechaRegreso = DateTime.Now.AddHours(1),
                FechaSolicitud = DateTime.Now
            };

            ViewData["IdVehiculo"] = new SelectList(_context.Vehiculo, "IdVehiculo", "Placa");
            
            ViewBag.NombreEmpleado = $"{usuarioActual.Empleado.Nombres} {usuarioActual.Empleado.Apellidos} - Cargo : {usuarioActual.Empleado.Cargo} - Subgerencia/Unidad : {usuarioActual.Empleado.Unidad}";
            ViewBag.IdUsuario = usuarioActual.IdUsuario;

            ViewBag.ListaVehiculos = _context.Vehiculo.ToList(); // pasa todos los vehículos
            
            

            return View(modelo);
        }

        // POST: Emple/SolicitudVehiculos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SolicitudVehiculo solicitudVehiculo)
        {
            if (!ModelState.IsValid)
            {
                // Repoblar los datos que necesita la vista
                ViewData["IdVehiculo"] = new SelectList(_context.Vehiculo, "IdVehiculo", "Placa");
                ViewBag.ListaVehiculos = _context.Vehiculo.ToList();

                // Para mantener el nombre del usuario
                var usuarioActual = _context.Usuario
                    .Include(u => u.Empleado)
                    .FirstOrDefault(u => u.NombreUsuario == User.Identity.Name);


                if (usuarioActual?.Empleado != null)
                {
                    ViewBag.NombreEmpleado = $"{usuarioActual.Empleado.Nombres} {usuarioActual.Empleado.Apellidos} - Cargo : {usuarioActual.Empleado.Cargo} - Subgerencia/Unidad : {usuarioActual.Empleado.Unidad}";
                    ViewBag.IdUsuario = usuarioActual.IdUsuario;
                }

                return View(solicitudVehiculo);

            }
            _context.Add(solicitudVehiculo);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "La solicitud fue creada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Emple/SolicitudVehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudVehiculo = await _context.SolicitudVehiculo.FindAsync(id);
          
            if (solicitudVehiculo == null)
            {
                return NotFound();
            }
            var usuarioActual = _context.Usuario
     .Include(u => u.Empleado)
     .FirstOrDefault(u => u.NombreUsuario == User.Identity.Name);

            if (usuarioActual == null)
            {
                return Unauthorized(); // o redirigir a login
            }
            // Verificar si el usuario actual es el creador de la solicitud
            if (solicitudVehiculo.IdUsuario != usuarioActual.IdUsuario)
            {
                // Si no es el creador, redirigir con mensaje de error
                TempData["ErrorMessage"] = "No tiene permiso para editar esta solicitud.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.NombreEmpleado = $"{usuarioActual.Empleado.Nombres} {usuarioActual.Empleado.Apellidos} - Cargo : {usuarioActual.Empleado.Cargo} - Subgerencia/Unidad : {usuarioActual.Empleado.Unidad}";
            ViewBag.IdUsuario = usuarioActual.IdUsuario;

            ViewData["IdVehiculo"] = new SelectList(_context.Vehiculo, "IdVehiculo", "Placa",solicitudVehiculo.IdVehiculo);
            ViewBag.ListaVehiculos = _context.Vehiculo.ToList(); // pasa todos los vehículos

            solicitudVehiculo.FechaSolicitud = DateTime.Now; // se setea FechaSolicitud automáticamente

            return View(solicitudVehiculo);
        }

        // POST: Emple/SolicitudVehiculos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSolicitudVehiculo,IdUsuario,IdVehiculo,FechaSalida,FechaRegreso,Recorrido,FechaSolicitud")] SolicitudVehiculo solicitudVehiculo)
        {
            if (id != solicitudVehiculo.IdSolicitudVehiculo)
            {
                return NotFound();
            }
            // Verificar que el usuario actual sea el creador de la solicitud
            var usuarioActual = _context.Usuario
                .FirstOrDefault(u => u.NombreUsuario == User.Identity.Name);

            if (usuarioActual == null)
            {
                return Unauthorized();
            }

            // Obtener la solicitud original desde la base de datos
            var solicitudOriginal = await _context.SolicitudVehiculo.AsNoTracking().FirstOrDefaultAsync(s => s.IdSolicitudVehiculo == id);
            if (solicitudOriginal == null)
            {
                return NotFound();
            }
         

            // Verificar si el usuario actual es el creador de la solicitud original
            if (solicitudOriginal.IdUsuario != usuarioActual.IdUsuario)
            {
                TempData["ErrorMessage"] = "No tiene permiso para editar esta solicitud.";
                return RedirectToAction(nameof(Index));
            }

            // Asegurarse de que no se modifique el IdUsuario original
            solicitudVehiculo.IdUsuario = solicitudOriginal.IdUsuario;

            if (ModelState.IsValid)
            {
                try
                {            // Indicar que la entidad ha sido modificada en lugar de intentar adjuntarla

                    _context.Update(solicitudVehiculo).State=EntityState.Modified;
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Solicitud actualizada correctamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SolicitudVehiculoExists(solicitudVehiculo.IdSolicitudVehiculo))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Si hay errores en el modelo, preparar nuevamente los datos para la vista
            var usuario = await _context.Usuario
                .Include(u => u.Empleado)
                .FirstOrDefaultAsync(u => u.IdUsuario == usuarioActual.IdUsuario);

            ViewBag.NombreEmpleado = $"{usuarioActual.Empleado.Nombres} {usuarioActual.Empleado.Apellidos} - Cargo : {usuarioActual.Empleado.Cargo} - Subgerencia/Unidad : {usuarioActual.Empleado.Unidad}";
            ViewBag.IdUsuario = usuarioActual.IdUsuario;

            ViewData["IdVehiculo"] = new SelectList(_context.Vehiculo, "IdVehiculo", "Placa", solicitudVehiculo.IdVehiculo);
            ViewBag.ListaVehiculos = _context.Vehiculo.ToList();

            return View(solicitudVehiculo);
        }

        // GET: Emple/SolicitudVehiculos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var solicitudVehiculo = await _context.SolicitudVehiculo
              .Include(s => s.Usuario)
                  .ThenInclude(u => u.Empleado)
              .Include(s => s.Vehiculo)
              .FirstOrDefaultAsync(m => m.IdSolicitudVehiculo == id);
            // Verificar que el usuario actual sea el creador de la solicitud para permitir la eliminación
            var usuarioActual = _context.Usuario
                .FirstOrDefault(u => u.NombreUsuario == User.Identity.Name);
            if (usuarioActual == null)
            {
                return Unauthorized(); // o redirigir a login
            }
            if (solicitudVehiculo == null)
            {
                return NotFound();
            }
            // Verificar si el usuario actual es el creador de la solicitud

            if (solicitudVehiculo.IdUsuario != usuarioActual.IdUsuario)
            {
                // Si no es el creador, redirigir con mensaje de error
                TempData["ErrorMessage"] = "No tiene permiso para eliminar esta solicitud.";
                return RedirectToAction(nameof(Index));
            }

            return View(solicitudVehiculo);
        }

        // POST: Emple/SolicitudVehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var solicitudVehiculo = await _context.SolicitudVehiculo.AsNoTracking().FirstOrDefaultAsync(s => s.IdSolicitudVehiculo == id);
            if (solicitudVehiculo != null)
            {
                return NotFound();

            }
            // Obtener el usuario actual
            var usuarioActual = _context.Usuario
                .FirstOrDefault(u => u.NombreUsuario == User.Identity.Name);

            if (usuarioActual == null)
            {
                return Unauthorized(); // o redirigir a login
            }

            // Verificar si el usuario actual es el creador de la solicitud
            if (solicitudVehiculo.IdUsuario != usuarioActual.IdUsuario)
            {
                // Si no es el creador, redirigir con mensaje de error
                TempData["ErrorMessage"] = "No tiene permiso para eliminar esta solicitud.";
                return RedirectToAction(nameof(Index));
            }
            // Crear una nueva instancia de la entidad para eliminar y establecer solo la clave primaria
            var solicitudToDelete = new SolicitudVehiculo { IdSolicitudVehiculo = id };
            _context.Entry(solicitudToDelete).State = EntityState.Deleted;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Solicitud eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool SolicitudVehiculoExists(int id)
        {
            return _context.SolicitudVehiculo.Any(e => e.IdSolicitudVehiculo == id);
        }
    }
}
