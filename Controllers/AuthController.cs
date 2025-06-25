using ControlMDBI.Data;
using ControlMDBI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics.Contracts;

namespace ControlMDBI.Controllers
{
    public class AuthController : Controller
    {
        private readonly ControlMDBIDbContext _context;


        public AuthController(ControlMDBIDbContext context)
        {
            _context = context;
        }

        public IActionResult Login(string? seccioExpirada)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
                if (userRole == "Administrador")
                {
                    return RedirectToAction("Index", "Empleados", new { area = "Admin" });
                }
                else if (userRole == "Empleado")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Emple" });
                }
                else if (userRole == "Patrimonio")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "Patri" });
                }
                else if (userRole == "Vigilante")
                {
                    return RedirectToAction("Index", "Registros", new { area = "Vigi" });
                }
                else if (userRole == "RRHH")
                {
                    return RedirectToAction("Index", "Dashboard", new { area = "RRHH" });
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }

            if (!string.IsNullOrEmpty(seccioExpirada) && seccioExpirada == "true")
            {
                ViewBag.Mensaje = "Tu sesión ha expirado por inactividad.";
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Usuario usuario)
        {


            if (string.IsNullOrEmpty(usuario.NombreUsuario) || string.IsNullOrEmpty(usuario.Contrasenia))
            {
                ModelState.AddModelError("Contrasenia", "El nombre de usuario y la contraseña son requeridos");
                return View(usuario);
            }

            // Incluir el empleado relacionado en la consulta
            var userDB = await _context.Usuario
                .Include(u => u.Empleado) // Cargar la relación con Empleado
                .FirstOrDefaultAsync(u => u.NombreUsuario == usuario.NombreUsuario && u.Empleado.Activo == true);
            //verifica 2: que el empleado este activo

            // Verificar 1: Que el usuario exista,Que la contraseña sea correcta
            if (userDB != null && BCrypt.Net.BCrypt.Verify(usuario.Contrasenia, userDB.Contrasenia))
            {

                // Crear claims (añadir más datos si es necesario)
                var claims = new List<Claim>
                    {
                       new Claim(ClaimTypes.Name, userDB.NombreUsuario),
                        new Claim(ClaimTypes.Role, userDB.Rol),
                        new Claim("NombreEmpleado",$"{userDB.Empleado.Nombres}"),
                        new Claim("IdUsuario", userDB.IdUsuario.ToString()),
                        new Claim(ClaimTypes.NameIdentifier, userDB.IdUsuario.ToString()),
                    };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);


                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = true, // 🔥 Esto hace que la cookie persista
                        ExpiresUtc = DateTime.UtcNow.AddMinutes(30) // ⏱️ Tiempo de expiración
                    });
                // Redirección por rol
                return userDB.Rol switch
                {
                    "Administrador" => RedirectToAction("Index", "Empleados", new { area = "Admin" }),
                    "Empleado" => RedirectToAction("Index", "Dashboard", new { area = "Emple" }),
                    "Patrimonio" => RedirectToAction("Index", "Dashboard", new { area = "Patri" }),
                    "Vigilante" => RedirectToAction("Index", "Registros", new { area = "Vigi" }),
                    "RRHH" => RedirectToAction("Index", "Dashboard", new { area = "RRHH" }),
                    _ => RedirectToAction("Index", "Home"),
                };
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Credednciales incorrectas o su cuenta esta desactivada, contacte con el administrador");
                return View(usuario);

            }
        }
        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
        public JsonResult MiPerfil()
        {
            var userName = User.Identity.Name;
            var usuario = _context.Usuario.Include(u => u.Empleado).Where(u => u.NombreUsuario == userName).Select(u => new
            {
                u.IdUsuario,
                u.IdEmpleado,
                u.NombreUsuario,
                u.Rol,
                Empleado = new
                {
                    u.Empleado.Nombres,
                    u.Empleado.Apellidos,
                }
            }).FirstOrDefault();

            if (usuario == null)
            {
                return Json(new { success = false, message = "Usuario no encontrado" });
            }
            // Serializar el objeto usuario a JSON
            //string data = JsonConvert.SerializeObject(usuario);

            return Json(new { success = true, data = usuario });
        }
    }
}
