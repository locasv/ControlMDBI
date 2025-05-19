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
    public class UsuariosController : Controller
    {
        private readonly ControlMDBIDbContext _context;

        public UsuariosController(ControlMDBIDbContext context)
        {
            _context = context;
        }
        //PáginadoUsuarios
        public async Task<UsuarioPaginadoViewModel> GetUsuariosPaginado(string? busquedaNombre, int paginaActual, int usuariosPorPagina)
        {
            IQueryable<Usuario> query = _context.Usuario.Include(u => u.Empleado);
            if(!string.IsNullOrEmpty(busquedaNombre))
            {
                query = query.Where(u => u.NombreUsuario.Contains(busquedaNombre));
            }
            int totalUsuarios = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling((double)totalUsuarios / usuariosPorPagina);
            if (paginaActual < 1)
            {
                paginaActual = 1;
            }
            else if (paginaActual > totalPaginas)
            {
                paginaActual = totalPaginas;
            }
            List<Usuario> usuarios = new();
            if (totalUsuarios > 0)
            {
                usuarios = await query.OrderBy(p => p.NombreUsuario)
                    .Skip((paginaActual - 1) * usuariosPorPagina)
                    .Take(usuariosPorPagina)
                    .ToListAsync();
            }
            var model = new UsuarioPaginadoViewModel
            {
                Usuarios = usuarios,
                PaginaActual = paginaActual,
                TotalPaginas = totalPaginas,
                BusquedaNombre = busquedaNombre
            };

            return model;
        }

        // GET: Admin/Usuarios
        public async Task<IActionResult> Index(string? busquedaNombre,int paginaActual =1)
        {
            int usuariosPorPagina = 15;
        if(string.IsNullOrEmpty(busquedaNombre))
            {
                busquedaNombre = "";
            }
        var model = await GetUsuariosPaginado(busquedaNombre, paginaActual, usuariosPorPagina);
            return View(model);
        

        //var controlMDBIDbContext = _context.Usuario.Include(u => u.Empleado);
          //  return View(await controlMDBIDbContext.ToListAsync());
        }

        // GET: Admin/Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .Include(u => u.Empleado)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }
        //buscar empleado por nombres y dni
        [HttpGet]
        public async Task<JsonResult> GetEmpleados(string? empleadoBuscado)
        {
          
            var empleados = await _context.Empleado
                .Where(b => b.Activo && ((b.Nombres + " " + b.Apellidos).Contains(empleadoBuscado) || (b.Apellidos + " " + b.Nombres).Contains(empleadoBuscado) ) || b.DNI.Contains(empleadoBuscado))
                .Select(e => new { id = e.IdEmpleado, text = e.Nombres + " " + e.Apellidos +" - Cargo : "+e.Cargo+" Unidad/Subgerencia : "+ e.Unidad })
                .ToListAsync();
            return Json(empleados);
        }


        // GET: Admin/Usuarios/Create
        public IActionResult Create()
        {

            /*ViewData["IdEmpleado"] = new SelectList(
        _context.Empleado.Select(e => new {
            e.IdEmpleado,
            DisplayText = $"{e.Apellidos}, {e.Nombres} - Cargo : {e.Cargo} - Unidad/Subgerencia : {e.Unidad}"
        }),
        "IdEmpleado",
        "DisplayText");
            */
            // Crear una instancia de Usuario con la fecha actual
            var usuario = new Usuario
            {
                FechaRegistro = DateTime.Now
            };
            return View(usuario);
        }

        // POST: Admin/Usuarios/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Usuario usuario)
        {

            if (ModelState.IsValid)
            {
                //Hashear la contraseña antes de guardar
                usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(usuario.Contrasenia);
                

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
           /*
            ViewData["IdEmpleado"] = new SelectList(
            _context.Empleado.Select(e => new {
                 e.IdEmpleado,
                 DisplayText = $"{e.Apellidos}, {e.Nombres} - Cargo : {e.Cargo} - Unidad/Subgerencia : {e.Unidad}"
             }), "IdEmpleado","DisplayText",usuario.IdEmpleado);*/
           
            return View(usuario);
        }

        // GET: Admin/Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //var usuario = await _context.Usuario.FindAsync(id);
            var usuario = await _context.Usuario
       .Include(u => u.Empleado) // Incluye el empleado relacionado
       .FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario == null)
                {
                    return NotFound();
                }
            var viewModel = new UsuarioEditViewModel
            {
                IdUsuario = usuario.IdUsuario,
                IdEmpleado = usuario.IdEmpleado,
                NombreUsuario = usuario.NombreUsuario,
                Contrasenia = usuario.Contrasenia,
                Rol = usuario.Rol,
                FechaRegistro = usuario.FechaRegistro,
                NombresEmpleado = usuario.Empleado.Nombres,
                ApellidosEmpleado = usuario.Empleado.Apellidos,
                Cargo = usuario.Empleado.Cargo,
                Unidad = usuario.Empleado.Unidad,
                ActivoEmpleado = usuario.Empleado.Activo
            };

            //ViewData["IdEmpleado"] = new SelectList(_context.Empleado, "IdEmpleado", "Apellidos", usuario.IdEmpleado);
            ViewData["IdEmpleado"] = new SelectList(
      _context.Empleado.Select(e => new {
          e.IdEmpleado,
          DisplayText = $"{e.Apellidos}, {e.Nombres} - Cargo : {e.Cargo} - Unidad/Subgerencia : {e.Unidad}"
      }),"IdEmpleado","DisplayText",usuario.IdEmpleado);

          

            return View(viewModel);

        }

        // POST: Admin/Usuarios/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UsuarioEditViewModel model)
        {
            if (id != model.IdUsuario)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var usuario = await _context.Usuario.FindAsync(id);
                var empleado = await _context.Empleado.FindAsync(model.IdEmpleado);
                if (usuario == null || empleado == null)
                {
                    return NotFound();
                }
                // Obtener la contraseña actual del usuario en la base de datos
                var usuarioActual = await _context.Usuario.AsNoTracking().FirstOrDefaultAsync(u => u.IdUsuario == id);
                
                
                if (usuarioActual == null)
                {
                    return NotFound();
                }

                // Hashear solo si cambió la contraseña
                if (model.Contrasenia != usuarioActual.Contrasenia)
                {
                    usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(model.Contrasenia);
                }
                else
                {
                    usuario.Contrasenia = usuarioActual.Contrasenia; // No cambió, mantener la misma
                }

                usuario.NombreUsuario = model.NombreUsuario;
                usuario.Rol = model.Rol;
                usuario.FechaRegistro = DateTime.Now;
                //actualizar estado del empleado

                empleado.Activo = model.ActivoEmpleado;

                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Usuario.Any(u => u.IdUsuario == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
                return View(model);
        }
       
        // GET: Admin/Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .Include(u => u.Empleado)
                .FirstOrDefaultAsync(m => m.IdUsuario == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Admin/Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuario.Remove(usuario);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuario.Any(e => e.IdUsuario == id);
        }
    }
}
