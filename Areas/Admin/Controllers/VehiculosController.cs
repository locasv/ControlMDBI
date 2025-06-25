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
    public class VehiculosController : Controller
    {
        private readonly ControlMDBIDbContext _context;

        public VehiculosController(ControlMDBIDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Vehiculos
        public async Task<IActionResult> Index()
        {
            var controlMDBIDbContext = _context.Vehiculo.Include(v => v.Sede);
            return View(await controlMDBIDbContext.ToListAsync());
        }
        // POST: Método para actualizar el estado del vehículo via AJAX
        [HttpPost]
        public async Task<IActionResult> UpdateEstado(int id, string nuevoEstado)
        {
            try
            {
                var vehiculo = await _context.Vehiculo.FindAsync(id);
                if (vehiculo == null)
                {
                    return Json(new { success = false, message = "Vehículo no encontrado" });
                }

                // Validar que el estado sea válido
                var estadosValidos = new[] { "Activo", "Inactivo", "Mantenimiento", "Circulando" };
                if (!estadosValidos.Contains(nuevoEstado))
                {
                    return Json(new { success = false, message = "Estado no válido" });
                }

                vehiculo.Estado = nuevoEstado;
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"Estado del vehículo {vehiculo.Placa} actualizado a {nuevoEstado}",
                    nuevoEstado = nuevoEstado
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error al actualizar el estado: " + ex.Message });
            }
        }
        // GET: Admin/Vehiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculo
                .Include(v => v.Sede)
                .FirstOrDefaultAsync(m => m.IdVehiculo == id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // GET: Admin/Vehiculos/Create
        public IActionResult Create()
        {
            ViewData["IdSede"] = new SelectList(_context.Sede, "IdSede", "Nombre");
            return View();
        }

        // POST: Admin/Vehiculos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehiculo vehiculo)
        {
           
            if (ModelState.IsValid)
            {
                //Verificar si ya existe un vehiculo con esa placa
                var existeVehiculo = await _context.Vehiculo
                    .AnyAsync(a => a.Placa == vehiculo.Placa);
                if (existeVehiculo)
                {
                    // Puedes redirigir o mostrar un mensaje de error más claro
                    TempData["ErrorMessage"] = "Ya existe un Vehiculo con esa placa.";
                    //return RedirectToAction(nameof(Index));
                    //ModelState.AddModelError(string.Empty, "Ya existe una empleado con ese DNI.");
                }
                else
                {
                    _context.Add(vehiculo);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "El vehiculo fue creado con exito.";
                    return RedirectToAction(nameof(Index));

                }
            }
            ViewData["IdSede"] = new SelectList(_context.Sede, "IdSede", "Nombre", vehiculo.IdSede);
            return View(vehiculo);
        }

        // GET: Admin/Vehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculo.FindAsync(id);
            if (vehiculo == null)
            {
                return NotFound();
            }
            ViewData["IdSede"] = new SelectList(_context.Sede, "IdSede", "Direccion", vehiculo.IdSede);
            return View(vehiculo);
        }

        // POST: Admin/Vehiculos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehiculo vehiculo)
        {
            if (id != vehiculo.IdVehiculo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                //Verificar si ya existe un vehiculo con esa placa
                var existeVehiculo = await _context.Vehiculo
                    .AnyAsync(a => a.Placa == vehiculo.Placa && a.IdVehiculo != vehiculo.IdVehiculo);
                try
                {
                    if (existeVehiculo)
                    {
                        // Mostrar un mensaje de error más claro
                        TempData["ErrorMessage"] = "Ya existe un Vehiculo con esa placa.";
                        //return RedirectToAction(nameof(Index));

                    }
                    else
                    {
                        _context.Update(vehiculo);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "El los cambios realizados se guardaron con exito.";
                        return RedirectToAction(nameof(Index));

                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehiculoExists(vehiculo.IdVehiculo))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                
            }
            ViewData["IdSede"] = new SelectList(_context.Sede, "IdSede", "Direccion", vehiculo.IdSede);
            return View(vehiculo);
        }

        // GET: Admin/Vehiculos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehiculo = await _context.Vehiculo
                .Include(v => v.Sede)
                .FirstOrDefaultAsync(m => m.IdVehiculo == id);
            if (vehiculo == null)
            {
                return NotFound();
            }

            return View(vehiculo);
        }

        // POST: Admin/Vehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehiculo = await _context.Vehiculo.FindAsync(id);
            if (vehiculo != null)
            {
                _context.Vehiculo.Remove(vehiculo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehiculoExists(int id)
        {
            return _context.Vehiculo.Any(e => e.IdVehiculo == id);
        }
    }
}
