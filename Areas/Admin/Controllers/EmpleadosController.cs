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
using ControlMDBI.Areas.Admin.ViewModels;

namespace ControlMDBI.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]

    public class EmpleadosController : Controller
    {
        private readonly ControlMDBIDbContext _context;

        public EmpleadosController(ControlMDBIDbContext context)
        {
            _context = context;
        }
        //PaginadoEmpleados
        public async Task<EmpleadoPaginadoViewModel> GetEmpleadoPaginado(string? busquedaNombre, string? busquedaDNI, int paginaActual, int usuariosPorPagina)
        {
            IQueryable<Empleado> query = _context.Empleado.Include(e => e.Sede);
            if (!string.IsNullOrEmpty(busquedaNombre))
            {
                query = query.Where(e => (e.Nombres + " " + e.Apellidos).Contains(busquedaNombre));
            }
            if (!string.IsNullOrEmpty(busquedaDNI))
            {
                query = query.Where(e => e.DNI.Contains(busquedaDNI));
            }
            int totalEmpleados = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalEmpleados / usuariosPorPagina);
            if (paginaActual < 1)
            {
                paginaActual = 1;
            }
            else if (paginaActual > totalPaginas)
            {
                paginaActual = totalPaginas;
            }
            List<Empleado> empleados = new();
            if (totalEmpleados > 0)
            {
                empleados = await query.OrderBy(e => e.Nombres)
                    .Skip((paginaActual - 1) * usuariosPorPagina)
                    .Take(usuariosPorPagina)
                    .ToListAsync();
            }
            var model = new EmpleadoPaginadoViewModel
            {
                Empleados = empleados,
                PaginaActual = paginaActual,
                TotalPaginas = totalPaginas,
                BusquedaNombre = busquedaNombre,
                BusquedaDNI = busquedaDNI
            };
            return model;
        }

        // GET: Admin/Empleados
        public async Task<IActionResult> Index(string? busquedaNombre, string? busquedaDNI, int paginaActual = 1)
        {
            int usuariosPorPagina = 12;
            if (string.IsNullOrEmpty(busquedaNombre))
            {
                busquedaNombre = "";
            }
            if (string.IsNullOrEmpty(busquedaDNI))
            {
                busquedaDNI = "";
            }
            var model = await GetEmpleadoPaginado(busquedaNombre, busquedaDNI, paginaActual, usuariosPorPagina);

            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int id, bool estado)
        {
            var empleado = await _context.Empleado.FindAsync(id);
            if (empleado != null)
            {
                empleado.Activo =estado;
                _context.Update(empleado);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();

        }

        // GET: Admin/Empleados/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleado
                .Include(e => e.Sede)
                .FirstOrDefaultAsync(m => m.IdEmpleado == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // GET: Admin/Empleados/Create
        public IActionResult Create()
        {
            ViewData["IdSede"] = new SelectList(_context.Sede, "IdSede", "Nombre");
            return View();
        }

        // POST: Admin/Empleados/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                //Verificar si ya existe una aprobación para esa solicitud 
                var existeEmpleado = await _context.Empleado
                    .AnyAsync(a => a.DNI == empleado.DNI || a.Nombres.Equals(empleado.Nombres) || a.Apellidos.Equals(empleado.Apellidos));
                if (existeEmpleado)
                {
                    // muestrar un mensaje de error más claro
                    TempData["ErrorMessage"] = "Error al crear algunos datos coiciden con otro empleado, intenlo de nuevo";

                    //return RedirectToAction(nameof(Index));
                    //ModelState.AddModelError(string.Empty, "Ya existe una empleado con ese DNI.");
                }
                else
                {

                    _context.Add(empleado);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "El empleado fue creado con exito.";
                    return RedirectToAction(nameof(Index));
                }

            }
            ViewData["IdSede"] = new SelectList(_context.Sede, "IdSede", "Nombre", empleado.IdSede);
            return View(empleado);
        }

        // GET: Admin/Empleados/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleado.FindAsync(id);
            if (empleado == null)
            {
                return NotFound();
            }
            ViewData["IdSede"] = new SelectList(_context.Sede, "IdSede", "Nombre", empleado.IdSede);
            return View(empleado);
        }

        // POST: Admin/Empleados/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Empleado empleado)
        {
            if (id != empleado.IdEmpleado)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                //Verificar si ya existe una empleado con ese DNI 
                var existeEmpleado = await _context.Empleado
                    .AnyAsync(a => a.DNI == empleado.DNI && a.IdEmpleado != empleado.IdEmpleado);
                if (existeEmpleado)
                {
                    // Puedes redirigir o mostrar un mensaje de error más claro
                    TempData["ErrorMessage"] = "Error al guardar.";
                    return RedirectToAction(nameof(Index));
                }
                else

                {
                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Los cambios que se hicieron se guardaron con exito.";
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["IdSede"] = new SelectList(_context.Sede, "IdSede", "Nombre", empleado.IdSede);
            return View(empleado);
        }

        // GET: Admin/Empleados/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleado
                .Include(e => e.Sede)
                .FirstOrDefaultAsync(m => m.IdEmpleado == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }

        // POST: Admin/Empleados/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleado.FindAsync(id);
            if (empleado != null)
            {
                _context.Empleado.Remove(empleado);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleado.Any(e => e.IdEmpleado == id);
        }
    }
}
