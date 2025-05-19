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

namespace ControlMDBI.Areas.Patrimonio.Controllers
{
    [Area("Patrimonio")]
    [Authorize(Roles = "Administrador, Patrimonio")]
    public class AprobacionVehiculosController : Controller
    {
        private readonly ControlMDBIDbContext _context;

        public AprobacionVehiculosController(ControlMDBIDbContext context)
        {
            _context = context;
        }

        // GET: Patrimonio/AprobacionVehiculos
        public async Task<IActionResult> Index()
        {
            var controlMDBIDbContext = _context.AprobacionVehiculo.Include(a => a.SolicitudVehiculo).Include(a => a.Usuario);
            return View(await controlMDBIDbContext.ToListAsync());
        }

        // GET: Patrimonio/AprobacionVehiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aprobacionVehiculo = await _context.AprobacionVehiculo
                .Include(a => a.SolicitudVehiculo)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.IdAprobacionVehiculo == id);
            if (aprobacionVehiculo == null)
            {
                return NotFound();
            }

            return View(aprobacionVehiculo);
        }

        // GET: Patrimonio/AprobacionVehiculos/Create
        public IActionResult Create()
        {
            ViewData["IdSolicitudVehiculo"] = new SelectList(_context.SolicitudVehiculo, "IdSolicitudVehiculo", "Recorrido");
            ViewData["IdUsuario"] = new SelectList(_context.Usuario, "IdUsuario", "Contrasenia");
            return View();
        }

        // POST: Patrimonio/AprobacionVehiculos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAprobacionVehiculo,IdSolicitudVehiculo,IdUsuario,FechaAprobacion,Observaciones,Estado")] AprobacionVehiculo aprobacionVehiculo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(aprobacionVehiculo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdSolicitudVehiculo"] = new SelectList(_context.SolicitudVehiculo, "IdSolicitudVehiculo", "Recorrido", aprobacionVehiculo.IdSolicitudVehiculo);
            ViewData["IdUsuario"] = new SelectList(_context.Usuario, "IdUsuario", "Contrasenia", aprobacionVehiculo.IdUsuario);
            return View(aprobacionVehiculo);
        }

        // GET: Patrimonio/AprobacionVehiculos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aprobacionVehiculo = await _context.AprobacionVehiculo.FindAsync(id);
            if (aprobacionVehiculo == null)
            {
                return NotFound();
            }
            ViewData["IdSolicitudVehiculo"] = new SelectList(_context.SolicitudVehiculo, "IdSolicitudVehiculo", "Recorrido", aprobacionVehiculo.IdSolicitudVehiculo);
            ViewData["IdUsuario"] = new SelectList(_context.Usuario, "IdUsuario", "Contrasenia", aprobacionVehiculo.IdUsuario);
            return View(aprobacionVehiculo);
        }

        // POST: Patrimonio/AprobacionVehiculos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdAprobacionVehiculo,IdSolicitudVehiculo,IdUsuario,FechaAprobacion,Observaciones,Estado")] AprobacionVehiculo aprobacionVehiculo)
        {
            if (id != aprobacionVehiculo.IdAprobacionVehiculo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(aprobacionVehiculo);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdSolicitudVehiculo"] = new SelectList(_context.SolicitudVehiculo, "IdSolicitudVehiculo", "Recorrido", aprobacionVehiculo.IdSolicitudVehiculo);
            ViewData["IdUsuario"] = new SelectList(_context.Usuario, "IdUsuario", "Contrasenia", aprobacionVehiculo.IdUsuario);
            return View(aprobacionVehiculo);
        }

        // GET: Patrimonio/AprobacionVehiculos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var aprobacionVehiculo = await _context.AprobacionVehiculo
                .Include(a => a.SolicitudVehiculo)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.IdAprobacionVehiculo == id);
            if (aprobacionVehiculo == null)
            {
                return NotFound();
            }

            return View(aprobacionVehiculo);
        }

        // POST: Patrimonio/AprobacionVehiculos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var aprobacionVehiculo = await _context.AprobacionVehiculo.FindAsync(id);
            if (aprobacionVehiculo != null)
            {
                _context.AprobacionVehiculo.Remove(aprobacionVehiculo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AprobacionVehiculoExists(int id)
        {
            return _context.AprobacionVehiculo.Any(e => e.IdAprobacionVehiculo == id);
        }
    }
}
