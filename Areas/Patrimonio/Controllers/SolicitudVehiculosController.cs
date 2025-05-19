using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ControlMDBI.Data;
using ControlMDBI.Models;

namespace ControlMDBI.Areas.Patrimonio.Controllers
{
    [Area("Patrimonio")]
    public class SolicitudVehiculosController : Controller
    {
        private readonly ControlMDBIDbContext _context;

        public SolicitudVehiculosController(ControlMDBIDbContext context)
        {
            _context = context;
        }

        // GET: Patrimonio/SolicitudVehiculos
        public async Task<IActionResult> Index()
        {
            var controlMDBIDbContext = _context.SolicitudVehiculo.Include(s => s.Usuario).Include(s => s.Vehiculo);
            return View(await controlMDBIDbContext.ToListAsync());
        }

        // GET: Patrimonio/SolicitudVehiculos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitudVehiculo = await _context.SolicitudVehiculo
                .Include(s => s.Usuario)
                .Include(s => s.Vehiculo)
                .FirstOrDefaultAsync(m => m.IdSolicitudVehiculo == id);
            if (solicitudVehiculo == null)
            {
                return NotFound();
            }

            return View(solicitudVehiculo);
        }

        // GET: Patrimonio/SolicitudVehiculos/Edit/5
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
            ViewData["IdUsuario"] = new SelectList(_context.Usuario, "IdUsuario", "Contrasenia", solicitudVehiculo.IdUsuario);
            ViewData["IdVehiculo"] = new SelectList(_context.Vehiculo, "IdVehiculo", "Estado", solicitudVehiculo.IdVehiculo);
            return View(solicitudVehiculo);
        }

        // POST: Patrimonio/SolicitudVehiculos/Edit/5
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(solicitudVehiculo);
                    await _context.SaveChangesAsync();
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
            ViewData["IdUsuario"] = new SelectList(_context.Usuario, "IdUsuario", "Contrasenia", solicitudVehiculo.IdUsuario);
            ViewData["IdVehiculo"] = new SelectList(_context.Vehiculo, "IdVehiculo", "Estado", solicitudVehiculo.IdVehiculo);
            return View(solicitudVehiculo);
        }

       
        private bool SolicitudVehiculoExists(int id)
        {
            return _context.SolicitudVehiculo.Any(e => e.IdSolicitudVehiculo == id);
        }
    }
}
