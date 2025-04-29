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
            ViewData["IdSede"] = new SelectList(_context.Sede, "IdSede", "Direccion");
            return View();
        }

        // POST: Admin/Vehiculos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdVehiculo,Placa,Marca,Estado,IdSede")] Vehiculo vehiculo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vehiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdSede"] = new SelectList(_context.Sede, "IdSede", "Direccion", vehiculo.IdSede);
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
        public async Task<IActionResult> Edit(int id, [Bind("IdVehiculo,Placa,Marca,Estado,IdSede")] Vehiculo vehiculo)
        {
            if (id != vehiculo.IdVehiculo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehiculo);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
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
