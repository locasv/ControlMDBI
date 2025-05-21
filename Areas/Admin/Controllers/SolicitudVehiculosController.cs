using ControlMDBI.Areas.Admin.ViewModels;
using ControlMDBI.Data;
using ControlMDBI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace ControlMDBI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class SolicitudVehiculosController : Controller
    {
        private readonly ControlMDBIDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public SolicitudVehiculosController(ControlMDBIDbContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        //Paginado de solicitudes de vehiculo
        public async Task<SolicitudesPaginadoViewModel> GetSolicitudPaginado(string? busquedaNombres, string? busquedaPlacaVehiculo, int paginaActual, int usuariosPorPagina)
        {
            
            IQueryable<SolicitudVehiculo> query = _context.SolicitudVehiculo
                .Include(u => u.Usuario)
                .ThenInclude(e => e.Empleado)
                .Include(v => v.Vehiculo);
            if (!string.IsNullOrEmpty(busquedaNombres))
            {
                query = query.Where(s => (s.Usuario.Empleado.Nombres + " " + s.Usuario.Empleado.Apellidos).Contains(busquedaNombres));
            }
            if (!string.IsNullOrEmpty(busquedaPlacaVehiculo))
            {
                query = query.Where(s => s.Vehiculo.Placa.Contains(busquedaPlacaVehiculo));
            }
            int totalSolicitudes = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalSolicitudes / usuariosPorPagina);
            if (paginaActual < 1)
            {
                paginaActual = 1;
            }
            else if (paginaActual > totalPaginas)
            {
                paginaActual = totalPaginas;
            }
            List<SolicitudVehiculo> solicitudes = new();
            if (totalSolicitudes > 0)
            {
                solicitudes = await query.OrderBy(e => e.Usuario.Empleado.Nombres)
                    .Skip((paginaActual - 1) * usuariosPorPagina)
                    .Take(usuariosPorPagina)
                    .ToListAsync();
            }
            var model = new SolicitudesPaginadoViewModel
            {
                Solicitudes = solicitudes,
                PaginaActual = paginaActual,
                TotalPaginas = totalPaginas,
                BusquedaNombres = busquedaNombres,
                BusquedaPlacaVehiculo = busquedaPlacaVehiculo
            };
            return model;
        }

        // GET: Admin/SolicitudVehiculos
        public async Task<IActionResult> Index(string? busquedaNombres, string? busquedaPlacaVehiculo, int paginaActual = 1)
        {
            int usuariosPorPagina = 10;
            if (string.IsNullOrEmpty(busquedaNombres))
            {
                busquedaNombres = "";
            }
            if (string.IsNullOrEmpty(busquedaPlacaVehiculo))
            {
                busquedaPlacaVehiculo = "";
            }
            var usuarioActual = _context.Usuario.FirstOrDefault(u => u.NombreUsuario == User.Identity.Name);

            if (usuarioActual != null)
            {
                ViewBag.IdUsuarioActual = usuarioActual.IdUsuario;
            }

            var model = await GetSolicitudPaginado(busquedaNombres, busquedaPlacaVehiculo, paginaActual, usuariosPorPagina);

            //    var solicitudes = _context.SolicitudVehiculo
            //.Include(s => s.Usuario)
            //    .ThenInclude(u => u.Empleado)
            //.Include(s => s.Vehiculo);
            //return View(await solicitudes.ToListAsync());
            return View(model);
        }

        // GET: Admin/SolicitudVehiculos/Details/5
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
        // GET: Admin/SolicitudVehiculos/GenerarPDF/5 QuesPDF
        public async Task<IActionResult> GenerarPDF(int? id)
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

            // Registrar QuestPDF con licencia Community (si es para uso no comercial)
            QuestPDF.Settings.License = LicenseType.Community;

            QuestPDF.Settings.EnableDebugging = true;

            // Crear el documento PDF utilizando QuestPDF
            byte[] pdfBytes = GenerarDocumentoPDF(solicitudVehiculo);

            // Devolver el PDF generado
            //return File(pdfBytes, "application/pdf", $"Solicitud_Vehiculo_{id}.pdf");
            //vista previa del pdf
            return File(pdfBytes, "application/pdf");

        }
        private byte[] GenerarDocumentoPDF(SolicitudVehiculo solicitudVehiculo)
        {
            return Document.Create(document =>
            {
                document.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Times New Roman"));

                    // Contenedor principal con borde redondeado
                    page.Content().Column(mainColumn =>
                    {
                        mainColumn.Item().Background(Colors.White)
                                    .Border(1)
                                    .BorderColor(Colors.Black)
                                    .Padding(15)
                                    .Column(innerColumn =>
                                {
                                    // Logo, título y número de solicitud
                                    innerColumn.Item().Row(row =>
                                    {
                                        string logoPath = Path.Combine(_hostingEnvironment.WebRootPath, "img", "logos", "contrlmdbi-removebg-preview.png");
                                        row.ConstantItem(50).Image(logoPath);
                                        row.RelativeItem().Text("MUNICIPALIDAD DISTRITAL BAÑOS DEL INCA").Bold().FontSize(14).AlignCenter();
                                        row.ConstantItem(100).Text($"N° :  {solicitudVehiculo.IdSolicitudVehiculo}").AlignRight();
                                    });

                                    innerColumn.Item().Text("SOLICITUD OFICIAL DE MOVILIDAD").Bold().FontSize(12).AlignCenter().Underline();
                                    innerColumn.Item().PaddingVertical(5);

                                    innerColumn.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text($"CONDUCTOR :  {solicitudVehiculo.Usuario.Empleado.Nombres} {solicitudVehiculo.Usuario.Empleado.Apellidos}");
                                        row.RelativeItem().Text($"PLACA : {solicitudVehiculo.Vehiculo.Placa}");
                                    });

                                    innerColumn.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text($"\nMARCA : {solicitudVehiculo.Vehiculo.Marca}");
                                    });

                                    innerColumn.Item().Text($"\nSUB GERENCIA / JEFE DE UNIDAD SOLICITANTE : {solicitudVehiculo.Usuario.Empleado.Unidad}");
                                    innerColumn.Item().Text($"\nLUGAR DE RECORRIDO : {solicitudVehiculo.Recorrido}");

                                    innerColumn.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text($"\nFECHA DE SALIDA : {solicitudVehiculo.FechaSalida:dd/MM/yy}");
                                        row.RelativeItem().Text($"\nHORA : {solicitudVehiculo.FechaSalida:HH:mm tt}");
                                    });

                                    innerColumn.Item().Row(row =>
                                    {
                                        row.RelativeItem().Text($"\nFECHA DE REGRESO : {solicitudVehiculo.FechaRegreso:dd/MM/yy}");
                                        row.RelativeItem().Text($"\nHORA : {solicitudVehiculo.FechaRegreso:HH:mm tt}");
                                    });

                                    innerColumn.Item().PaddingVertical(10);

                                    innerColumn.Item().Row(row =>
                                    {
                                        row.RelativeItem().Column(col =>
                                        {
                                            col.Item().Text("\n\n\n___________________________").AlignCenter();
                                            col.Item().Text("SUB GERENCIA / JEFE DE UNIDAD").AlignCenter();
                                            col.Item().Text("FIRMA Y SELLO").AlignCenter();
                                        });

                                        row.RelativeItem().Column(col =>
                                        {
                                            col.Item().Text("\n\n\n___________________________").AlignCenter();
                                            col.Item().Text("CONDUCTOR / OPERADOR").AlignCenter();
                                            col.Item().Text($"DNI : {solicitudVehiculo.Usuario.Empleado.DNI}").AlignCenter();
                                        });

                                        row.RelativeItem().Column(col =>
                                        {
                                            col.Item().Text("\n\n\n___________________________").AlignCenter();
                                            col.Item().Text("UNIDAD DE CONTROL PATRIMONIAL V° B°").AlignCenter();
                                        });
                                    });

                                    innerColumn.Item().PaddingVertical(10);
                                    innerColumn.Item().Text("________________________________________________________________________________").AlignCenter();
                                    innerColumn.Item().Text("LLENADO SOLO POR EL GUARDIAN RESPONSABLE DEL TURNO").Bold().AlignCenter();

                                    innerColumn.Item().Row(row =>
                                    {
                                        row.RelativeItem().Column(col =>
                                        {
                                            col.Item().Text("\nNOMBRES Y APELLIDOS : __________________________________________________________________").AlignLeft();
                                            col.Item().Text("\nHORA DE SALIDA : ________________   FECHA: ________________  FIRMA : ______________________").AlignLeft();
                                        });
                                    });

                                    innerColumn.Item().PaddingVertical(3);

                                    innerColumn.Item().Row(row =>
                                    {
                                        row.RelativeItem().Column(col =>
                                        {
                                            col.Item().Text("\nNOMBRES Y APELLIDOS : __________________________________________________________________").AlignLeft();
                                            col.Item().Text("\nHORA DE REGRESO : ________________   FECHA: ________________  FIRMA : _____________________").AlignLeft();
                                        });
                                    });
                                });
                        // Texto "PATRIMONIO" fuera del contenedor con borde
                        mainColumn.Item().AlignRight().Text("PATRIMONIO").FontSize(10);
                    });
                });
            }).GeneratePdf();
        }

        // GET: Admin/SolicitudVehiculos/Create
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


            //ViewBag.IdUsuario = new SelectList(_context.Usuario
            //            .Include(u => u.Empleado)
            //            .Where(u => u.Empleado != null) // para evitar nulls
            //            .Select(u => new
            //            {
            //                u.IdUsuario,
            //                NombreCompletoYUnidad = u.Empleado.Nombres + " " + u.Empleado.Apellidos + "- Cargo : " + u.Empleado.Cargo + " - Subgerencia/Unidad : " + u.Empleado.Unidad
            //            })
            //            .ToList(), "IdUsuario", "NombreCompletoYUnidad");

                        ViewBag.IdUsuario = _context.Usuario
                .Include(u => u.Empleado)
                .Where(u => u.Empleado != null)
                .Select(u => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = u.IdUsuario.ToString(),
                    Text = $"{u.Empleado.Nombres} {u.Empleado.Apellidos}  <small>Cargo: {u.Empleado.Cargo} - Unidad: {u.Empleado.Unidad}</small>"
                }).ToList();

            //Para el empleado, mas no para el administrador o patrimonio
            //ViewBag.NombreEmpleado = $"{usuarioActual.Empleado.Nombres} {usuarioActual.Empleado.Apellidos} - Cargo : {usuarioActual.Empleado.Cargo} - Subgerencia/Unidad : {usuarioActual.Empleado.Unidad}";
            //ViewBag.IdUsuario = usuarioActual.IdUsuario;

            ViewBag.ListaVehiculos = _context.Vehiculo.ToList(); // pasa todos los vehículos

            return View(modelo);
        }

        //BUSCAR Vehiculos por placa
        [HttpGet]
        public async Task<JsonResult> GetPlacas(string? placaBuscada)
        {
            var placaVehiculos = await _context.Vehiculo
                .Where(b => b.Estado == "Activo" && (b.Placa.Contains(placaBuscada)))
                .Select(e => new { id = e.IdVehiculo, text = "Placa : " + e.Placa + " - " + e.Estado })
                .ToListAsync();

            return Json(placaVehiculos);
        }

        // POST: Admin/SolicitudVehiculos/Create
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

        // GET: Admin/SolicitudVehiculos/Edit/5
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


            ViewData["IdUsuario"] = new SelectList(_context.Usuario.Include(u => u.Empleado)
    .Select(u => new
    {
        IdUsuario = u.IdUsuario,
        Nombre = u.Empleado.Nombres + " " + u.Empleado.Apellidos + " - Cargo: " + u.Empleado.Cargo + " - Subgerencia/Unidad: " + u.Empleado.Unidad
    }), "IdUsuario", "Nombre", solicitudVehiculo.IdUsuario);


            ViewData["IdVehiculo"] = new SelectList(_context.Vehiculo, "IdVehiculo", "Placa", solicitudVehiculo.IdVehiculo);

            ViewBag.ListaVehiculos = _context.Vehiculo.ToList(); // pasa todos los vehículos

            // solicitudVehiculo.FechaSolicitud = DateTime.Now; // se setea FechaSolicitud automáticamente

            return View(solicitudVehiculo);
        }

        // POST: Admin/SolicitudVehiculos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SolicitudVehiculo solicitudVehiculo)
        {
            if (id != solicitudVehiculo.IdSolicitudVehiculo)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {            // Indicar que la entidad ha sido modificada en lugar de intentar adjuntarla

                    _context.Update(solicitudVehiculo);
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


            ViewData["IdUsuario"] = new SelectList(_context.Usuario, "IdUsuario", "NombreUsuario", solicitudVehiculo.IdUsuario);
            ViewData["IdVehiculo"] = new SelectList(_context.Vehiculo, "IdVehiculo", "Placa", solicitudVehiculo.IdVehiculo);
            ViewBag.ListaVehiculos = _context.Vehiculo.ToList();

            return View(solicitudVehiculo);
        }

        // GET: Admin/SolicitudVehiculos/Delete/5
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

            if (solicitudVehiculo == null)
            {
                return NotFound();
            }
            return View(solicitudVehiculo);
        }

        // POST: Admin/SolicitudVehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var solicitudVehiculo = await _context.SolicitudVehiculo.AsNoTracking().FirstOrDefaultAsync(s => s.IdSolicitudVehiculo == id);
            if (solicitudVehiculo != null)
            {
                _context.SolicitudVehiculo.Remove(solicitudVehiculo);

            }

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
