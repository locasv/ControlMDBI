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
                
                query = query.Where(u => (u.Empleado.Nombres + " " + u.Empleado.Apellidos).Contains(busquedaNombre));
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
          .Where(e => e.Activo &&
                     !_context.Usuario.Any(u => u.IdEmpleado == e.IdEmpleado) &&
                     (string.IsNullOrEmpty(empleadoBuscado) ||
                      (e.Nombres + " " + e.Apellidos).Contains(empleadoBuscado) || e.DNI.Contains(empleadoBuscado)))
          .Select(e => new {
              id = e.IdEmpleado,
              text = $"{e.Apellidos}, {e.Nombres}"
          })
          .Take(10)
          .ToListAsync();

            return Json(empleados);
            //var empleados = await _context.Empleado
            //    .Where(b => b.Activo && ((b.Nombres + " " + b.Apellidos).Contains(empleadoBuscado) || (b.Apellidos + " " + b.Nombres).Contains(empleadoBuscado) || b.DNI.Contains(empleadoBuscado)))
            //    .Select(e => new { 
            //        id = e.IdEmpleado, 
            //        text =$"{e.Nombres} {e.Apellidos}" 
            //    })
            //    .Take(5)
            //    .ToListAsync();
            //return Json(empleados);
        }
        //Actualizar estado
        [HttpPost]
        public async Task<IActionResult> ActualizarEstado(int id, bool estado)
        {
            var empleado = await _context.Empleado.FindAsync(id);
            if (empleado != null)
            {
                empleado.Activo = estado;
                _context.Update(empleado);
                await _context.SaveChangesAsync();
                return Ok();
            }
            return NotFound();

        }

        // GET: Admin/Usuarios/Create

        public IActionResult Create()
        {
            var viewModel = new UsuarioCreateViewModel
            {
                FechaRegistro = DateTime.Now
            };
            return View(viewModel);
        }

        // POST: Admin/Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UsuarioCreateViewModel model)
        {
            // Validaciones adicionales
            await ValidarCreacionUsuario(model);

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el empleado existe y está activo
                    var empleado = await _context.Empleado
                        .FirstOrDefaultAsync(e => e.IdEmpleado == model.IdEmpleado && e.Activo);

                    if (empleado == null)
                    {
                        ModelState.AddModelError("IdEmpleado", "El empleado seleccionado no existe o no está activo");
                        return View(model);
                    }

                    // Crear el nuevo usuario
                    var usuario = new Usuario
                    {
                        IdEmpleado = model.IdEmpleado,
                        NombreUsuario = model.NombreUsuario.Trim().ToLower(),
                        Contrasenia = BCrypt.Net.BCrypt.HashPassword(model.Contrasenia),
                        Rol = model.Rol,
                        FechaRegistro = DateTime.Now
                    };

                    _context.Add(usuario);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Usuario '{model.NombreUsuario}' creado exitosamente para {empleado.Nombres} {empleado.Apellidos}";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Log del error (si tienes un sistema de logging)
                    TempData["ErrorMessage"] = "Ocurrió un error al crear el usuario. Intente nuevamente.";
                }
            }

            return View(model);
        }

        private async Task ValidarCreacionUsuario(UsuarioCreateViewModel model)
        {
            // Verificar si ya existe un usuario con ese nombre
            if (!string.IsNullOrEmpty(model.NombreUsuario))
            {
                var existeNombreUsuario = await _context.Usuario
                    .AnyAsync(u => u.NombreUsuario.ToLower() == model.NombreUsuario.Trim().ToLower());

                if (existeNombreUsuario)
                {
                    ModelState.AddModelError("NombreUsuario", "Ya existe un usuario con este nombre");
                }
            }

            // Verificar si el empleado ya tiene un usuario asignado
            if (model.IdEmpleado > 0)
            {
                var empleadoTieneUsuario = await _context.Usuario
                    .AnyAsync(u => u.IdEmpleado == model.IdEmpleado);

                if (empleadoTieneUsuario)
                {
                    ModelState.AddModelError("IdEmpleado", "Este empleado ya tiene un usuario asignado");
                }
            }
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
                // Mostrar indicador de contraseña existente
                ContraseniaActual = !string.IsNullOrEmpty(usuario.Contrasenia) ? "••••••••" : "",
                TieneContrasenia = !string.IsNullOrEmpty(usuario.Contrasenia),
                Rol = usuario.Rol,
                FechaRegistro = usuario.FechaRegistro,
                //Datos empleados
                NombresEmpleado = usuario.Empleado.Nombres,
                ApellidosEmpleado = usuario.Empleado.Apellidos,
                Cargo = usuario.Empleado.Cargo,
                Unidad = usuario.Empleado.Unidad,
                ActivoEmpleado = usuario.Empleado.Activo
            };
            //Obtener contraseña hasheada


            //ViewData["IdEmpleado"] = new SelectList(_context.Empleado, "IdEmpleado", "Apellidos", usuario.IdEmpleado);
            ViewData["IdEmpleado"] = new SelectList(
              _context.Empleado.Select(e => new {
                  e.IdEmpleado,
                  DisplayText = $"{e.Apellidos}, {e.Nombres}" 
               }
              ),"IdEmpleado","DisplayText",usuario.IdEmpleado
             );

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

            // Validar confirmación de contraseña si se proporciona nueva contraseña
            if (!string.IsNullOrEmpty(model.ContraseniaNueva))
            {
                if (model.ContraseniaNueva != model.ConfirmarContrasenia)
                {
                    ModelState.AddModelError("ConfirmarContrasenia", "Las contraseñas no coinciden.");
                }

                if (model.ContraseniaNueva.Length < 6)
                {
                    ModelState.AddModelError("ContraseniaNueva", "La contraseña debe tener al menos 6 caracteres.");
                }
            }
            // Comprobar la validez del modelo, pero excluir los campos que no son relevantes para la edición
            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener el usuario actual
                    var usuario = await _context.Usuario.FindAsync(id);
                    var empleado = await _context.Empleado.FindAsync(model.IdEmpleado);

                    if (usuario == null || empleado == null)
                    {
                        return NotFound();
                    }

                    // Actualizar los datos del usuario
                    usuario.NombreUsuario = model.NombreUsuario;
                    usuario.Rol = model.Rol;



                    // Actualizar contraseña solo si se proporcionó una nueva
                    if (!string.IsNullOrEmpty(model.ContraseniaNueva))
                    {
                        usuario.Contrasenia = BCrypt.Net.BCrypt.HashPassword(model.ContraseniaNueva);
                    }
                    // Actualizar estado del empleado
                    empleado.Activo = model.ActivoEmpleado;




                    //Verificar si ya existe una usuario con ese nombre
                    var existeUsuario = await _context.Usuario
                        .AnyAsync(a => a.NombreUsuario == model.NombreUsuario && a.IdUsuario != model.IdUsuario);
                    if (existeUsuario)
                    {
                        TempData["ErrorMessage"] = "Error nombre de usuario ya existe en la base de datos";

                    }
                    else
                    {
                        // Guardar los cambios
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Cambios realizados con exito";
                        return RedirectToAction(nameof(Index));
                    }
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
            // Si hay errores, recargar datos necesarios para la vista
            var usuarioReload = await _context.Usuario
               .Include(u => u.Empleado)
               .FirstOrDefaultAsync(u => u.IdUsuario == id);

            if (usuarioReload != null)
            {
                model.NombresEmpleado = usuarioReload.Empleado.Nombres;
                model.ApellidosEmpleado = usuarioReload.Empleado.Apellidos;
                model.ContraseniaActual = !string.IsNullOrEmpty(usuarioReload.Contrasenia) ? "••••••••" : "";
                model.TieneContrasenia = !string.IsNullOrEmpty(usuarioReload.Contrasenia);
            }

            ViewData["IdEmpleado"] = new SelectList(
                   _context.Empleado.Select(e => new
                   {
                       e.IdEmpleado,
                       DisplayText = $"{e.Apellidos}, {e.Nombres}"
                   }
                   ), "IdEmpleado", "DisplayText", model.IdEmpleado
                  );

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
