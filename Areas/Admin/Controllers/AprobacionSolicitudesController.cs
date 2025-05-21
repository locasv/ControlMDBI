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

namespace ControlMDBI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class AprobacionSolicitudesController : Controller
    {
        private readonly ControlMDBIDbContext _context;

        public AprobacionSolicitudesController(ControlMDBIDbContext context)
        {
            _context = context;
        }

        // GET: Admin/AprobacionSolicitudes
        public async Task<IActionResult> Index(string estado = null, string buscarPalabra = null, int? pagina = 1)
        {
            // Consulta base con todos los includes necesarios
            var query = _context.AprobacionVehiculo
                .Include(a => a.SolicitudVehiculo)
                    .ThenInclude(s => s.Usuario)
                .Include(a => a.Usuario)
                .Include(a => a.SolicitudVehiculo)
                    .ThenInclude(s => s.Vehiculo)
                .AsQueryable();

            // Filtrar por estado si se especifica
            if (!string.IsNullOrEmpty(estado) && estado != "Todos")
            {
                query = query.Where(a => a.Estado == estado);
            }

            // Filtrar por texto de búsqueda
            if (!string.IsNullOrEmpty(buscarPalabra))
            {
                buscarPalabra = buscarPalabra.ToLower();
                query = query.Where(a =>
                   //Por recorrido
                    a.SolicitudVehiculo.Recorrido.ToLower().Contains(buscarPalabra) ||
                    //Por usuario que aprueba
                    a.Usuario.NombreUsuario.ToLower().Contains(buscarPalabra) ||
                    //Por solicitante
                    a.SolicitudVehiculo.Usuario.NombreUsuario.ToLower().Contains(buscarPalabra) ||
                    //Por observaciones
                    a.Observaciones.ToLower().Contains(buscarPalabra) ||
                    //Por numero de solicitud
                    a.IdSolicitudVehiculo.ToString().Contains(buscarPalabra)||
                    // Por vehiculo
                    a.SolicitudVehiculo.Vehiculo.Placa.ToLower().Contains(buscarPalabra)
                );
            }

            // Ordenar por fecha de aprobación (las más recientes primero)
            query = query.OrderByDescending(a => a.FechaAprobacion);

            // Implementar paginación si es necesario
            int elementosPagina = 10; // Número de elementos por página

            // Obtener el conteo total y aplicar la paginación
            var totalItems = await query.CountAsync();
            var items = await query.Skip((pagina.Value - 1) * elementosPagina).Take(elementosPagina).ToListAsync();

            // Pasar información de paginación a la vista si se necesita
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(totalItems / (double)elementosPagina);
            ViewBag.TotalItems = totalItems;
            ViewBag.FiltroActual = buscarPalabra;
            ViewBag.EstadoActual = estado;

            // Lista de estados para el filtro
            ViewBag.Estados = new List<string> { "Todos", "Aprobado", "Rechazado", "Pendiente" };

            return View(items);
        }
        // Método para procesar los filtros del formulario
        [HttpGet]
        public IActionResult Filtrar(string buscarPalabra, string estado)
        {
            return RedirectToAction("Index", new { buscarPalabra, estado, pagina = 1 });
        }

        // GET: Admin/AprobacionSolicitudes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aprobacionVehiculo = await _context.AprobacionVehiculo
                .Include(a => a.SolicitudVehiculo)
                    .ThenInclude(s => s.Usuario)
                .Include(a => a.SolicitudVehiculo)
                    .ThenInclude(s => s.Vehiculo)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.IdAprobacionVehiculo == id);

            if (aprobacionVehiculo == null)
            {
                return NotFound();
            }

            return View(aprobacionVehiculo);
        }

        // GET: Admin/AprobacionSolicitudes/Create
        public IActionResult Create(int? idSolicitud)
        {
            // Obtener lista de solicitudes de vehículos para el dropdown
            var solicitudes = _context.SolicitudVehiculo
                .Include(s => s.Usuario)
                .Include(s => s.Vehiculo)
                .ToList();

            // Preparar el ViewData con información detallada para el dropdown
            ViewData["IdSolicitudVehiculo"] = new SelectList(
                solicitudes.Select(s => new
                {
                    IdSolicitudVehiculo = s.IdSolicitudVehiculo,
                    Descripcion = $"N° Solicitud: {s.IdSolicitudVehiculo} - Usuario: {s.Usuario?.NombreUsuario} - Fecha: {s.FechaSolicitud.ToString("dd/MM/yyyy HH:mm")}"
                }),
                "IdSolicitudVehiculo",
                "Descripcion"
            );

            // Obtener el usuario actual que aprueba
            ViewData["IdUsuario"] = new SelectList(_context.Usuario, "IdUsuario", "NombreUsuario");

            // Si se proporciona un ID de solicitud, precargarlo
            if (idSolicitud.HasValue)
            {
                var solicitud = solicitudes.FirstOrDefault(s => s.IdSolicitudVehiculo == idSolicitud.Value);
                if (solicitud != null)
                {
                    ViewData["SolicitudSeleccionada"] = solicitud;
                    return View(new AprobacionVehiculo
                    {
                        IdSolicitudVehiculo = idSolicitud.Value,
                        FechaAprobacion = DateTime.Now,
                        Estado = ListaEstadoAprobacion.Pendiente.ToString()
                    });
                }
            }

            return View();
        }

        // POST: Admin/AprobacionSolicitudes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AprobacionVehiculo aprobacionVehiculo)
        {
            if (ModelState.IsValid)
            {
                //Verificar si ya existe una aprobación para esa solicitud 
                var existeAprobacion = await _context.AprobacionVehiculo
                    .AnyAsync(a => a.IdSolicitudVehiculo == aprobacionVehiculo.IdSolicitudVehiculo);
                if (existeAprobacion)
                {
                    ModelState.AddModelError(string.Empty, "Ya existe una aprobación para esta solicitud.");
                }
                else
                {

                    // Establecer la fecha de aprobación al momento actual si no está establecida
                    if (aprobacionVehiculo.FechaAprobacion == default)
                        aprobacionVehiculo.FechaAprobacion = DateTime.Now;

                    _context.Add(aprobacionVehiculo);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                
            }

            // Si hay errores, volver a cargar las listas desplegables
            var solicitudes = _context.SolicitudVehiculo
                .Include(s => s.Usuario)
                .Include(s => s.Vehiculo)
                .ToList();

            ViewData["IdSolicitudVehiculo"] = new SelectList(
                solicitudes.Select(s => new
                {
                    IdSolicitudVehiculo = s.IdSolicitudVehiculo,
                    Descripcion = $"ID: {s.IdSolicitudVehiculo} - Usuario: {s.Usuario?.NombreUsuario} - Fecha: {s.FechaSolicitud.ToString("dd/MM/yyyy HH:mm")}"
                }),
                "IdSolicitudVehiculo",
                "Descripcion",
                aprobacionVehiculo.IdSolicitudVehiculo
            );

            ViewData["IdUsuario"] = new SelectList(_context.Usuario, "IdUsuario", "NombreUsuario", aprobacionVehiculo.IdUsuario);

            // Cargar detalles de la solicitud seleccionada
            if (aprobacionVehiculo.IdSolicitudVehiculo > 0)
            {
                var solicitudSeleccionada = solicitudes.FirstOrDefault(s => s.IdSolicitudVehiculo == aprobacionVehiculo.IdSolicitudVehiculo);
                if (solicitudSeleccionada != null)
                {
                    ViewData["SolicitudSeleccionada"] = solicitudSeleccionada;
                }
            }

            return View(aprobacionVehiculo);
        }

        // GET: Admin/AprobacionSolicitudes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aprobacionVehiculo = await _context.AprobacionVehiculo
                .Include(a => a.SolicitudVehiculo)
                .ThenInclude(s => s.Usuario)
                .Include(a => a.SolicitudVehiculo)
                .ThenInclude(s => s.Vehiculo)
                .FirstOrDefaultAsync(a => a.IdAprobacionVehiculo == id);

            if (aprobacionVehiculo == null)
            {
                return NotFound();
            }

            // Obtener lista de solicitudes de vehículos para el dropdown
            var solicitudes = _context.SolicitudVehiculo
                .Include(s => s.Usuario)
                .Include(s => s.Vehiculo)
                .ToList();

            // Preparar el ViewData con información detallada para el dropdown
            ViewData["IdSolicitudVehiculo"] = new SelectList(
                solicitudes.Select(s => new
                {
                    IdSolicitudVehiculo = s.IdSolicitudVehiculo,
                    Descripcion = $"N° Solicitud: {s.IdSolicitudVehiculo} - Usuario: {s.Usuario?.NombreUsuario} - Fecha: {s.FechaSolicitud.ToString("dd/MM/yyyy HH:mm")}"
                }),
                "IdSolicitudVehiculo",
                "Descripcion",
                aprobacionVehiculo.IdSolicitudVehiculo
            );

            // Obtener el usuario que aprueba
            ViewData["IdUsuario"] = new SelectList(_context.Usuario, "IdUsuario", "NombreUsuario", aprobacionVehiculo.IdUsuario);

            // Cargar detalles de la solicitud seleccionada
            ViewData["SolicitudSeleccionada"] = aprobacionVehiculo.SolicitudVehiculo;

            return View(aprobacionVehiculo);
        }

        // POST: Admin/AprobacionSolicitudes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AprobacionVehiculo aprobacionVehiculo)
        {
            if (id != aprobacionVehiculo.IdAprobacionVehiculo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Conservar la fecha de la aprobación original si no se cambia
                    if (aprobacionVehiculo.FechaAprobacion == default)
                    {
                        var aprobacionOriginal = await _context.AprobacionVehiculo
                            .AsNoTracking()
                            .FirstOrDefaultAsync(a => a.IdAprobacionVehiculo == id);

                        if (aprobacionOriginal != null)
                        {
                            aprobacionVehiculo.FechaAprobacion = aprobacionOriginal.FechaAprobacion;
                        }
                        else
                        {
                            aprobacionVehiculo.FechaAprobacion = DateTime.Now;
                        }
                    }

                    _context.Update(aprobacionVehiculo);
                    await _context.SaveChangesAsync();

                    // Puedes agregar aquí lógica adicional según el estado seleccionado
                    // Por ejemplo, si el estado cambia a "Aprobado", podrías actualizar la solicitud

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AprobacionVehiculoExists(aprobacionVehiculo.IdAprobacionVehiculo))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Si hay errores, volver a cargar las listas desplegables
            var solicitudes = _context.SolicitudVehiculo
                .Include(s => s.Usuario)
                .Include(s => s.Vehiculo)
                .ToList();

            ViewData["IdSolicitudVehiculo"] = new SelectList(
                solicitudes.Select(s => new
                {
                    IdSolicitudVehiculo = s.IdSolicitudVehiculo,
                    Descripcion = $"N° Solicitud: {s.IdSolicitudVehiculo} - Usuario: {s.Usuario?.NombreUsuario} - Fecha: {s.FechaSolicitud.ToString("dd/MM/yyyy HH:mm")}"
                }),
                "IdSolicitudVehiculo",
                "Descripcion",
                aprobacionVehiculo.IdSolicitudVehiculo
            );

            ViewData["IdUsuario"] = new SelectList(_context.Usuario, "IdUsuario", "NombreUsuario", aprobacionVehiculo.IdUsuario);

            // Cargar detalles de la solicitud seleccionada
            if (aprobacionVehiculo.IdSolicitudVehiculo > 0)
            {
                var solicitudSeleccionada = solicitudes.FirstOrDefault(s => s.IdSolicitudVehiculo == aprobacionVehiculo.IdSolicitudVehiculo);
                if (solicitudSeleccionada != null)
                {
                    ViewData["SolicitudSeleccionada"] = solicitudSeleccionada;
                }
            }

            return View(aprobacionVehiculo);
        }

        //// GET: Admin/AprobacionSolicitudes/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var aprobacionVehiculo = await _context.AprobacionVehiculo
        //        .Include(a => a.SolicitudVehiculo)
        //        .Include(a => a.Usuario)
        //        .FirstOrDefaultAsync(m => m.IdAprobacionVehiculo == id);
        //    if (aprobacionVehiculo == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(aprobacionVehiculo);
        //}

        //// POST: Admin/AprobacionSolicitudes/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var aprobacionVehiculo = await _context.AprobacionVehiculo.FindAsync(id);
        //    if (aprobacionVehiculo != null)
        //    {
        //        _context.AprobacionVehiculo.Remove(aprobacionVehiculo);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}
        // GET: Admin/AprobacionSolicitudes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aprobacionVehiculo = await _context.AprobacionVehiculo
                .Include(a => a.SolicitudVehiculo)
                    .ThenInclude(s => s.Usuario)
                .Include(a => a.SolicitudVehiculo)
                    .ThenInclude(s => s.Vehiculo)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.IdAprobacionVehiculo == id);


            if (aprobacionVehiculo == null)
            {
                return NotFound();
            }

            return View(aprobacionVehiculo);
        }

        // POST: Admin/AprobacionSolicitudes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var aprobacionVehiculo = await _context.AprobacionVehiculo.FindAsync(id);
                if (aprobacionVehiculo == null)
                {
                    return NotFound();
                }

                _context.AprobacionVehiculo.Remove(aprobacionVehiculo);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "La aprobación de vehículo ha sido eliminada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the error
                TempData["ErrorMessage"] = "Ha ocurrido un error al eliminar la aprobación. Por favor, inténtelo nuevamente.";
                return RedirectToAction(nameof(Index));
            }
        }
        private bool AprobacionVehiculoExists(int id)
        {
            return _context.AprobacionVehiculo.Any(e => e.IdAprobacionVehiculo == id);
        }
    }
}
