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
            int usuariosPorPagina = 2;
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
                _context.Add(empleado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> Edit(int id, [Bind("IdEmpleado,DNI,Nombres,Apellidos,Direccion,Correo,Cargo,Unidad,Activo,IdSede")] Empleado empleado)
        {
            if (id != empleado.IdEmpleado)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empleado);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.IdEmpleado))
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
