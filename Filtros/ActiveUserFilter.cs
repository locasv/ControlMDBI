using ControlMDBI.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ControlMDBI.Filtros
{
    public class ActiveUserFilter : IAsyncAuthorizationFilter
    {
        private readonly ControlMDBIDbContext _context;

        public ActiveUserFilter(ControlMDBIDbContext context)
        {
            _context = context;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated) return;

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", new { message = "No se pudo validar el usuario actual" });
                return;
            }

            var usuarioActivo = await _context.Usuario
                .Include(u => u.Empleado)
                .AnyAsync(u => u.IdUsuario == userId && u.Empleado.Activo);

            if (!usuarioActivo)
            {
                await context.HttpContext.SignOutAsync();
                context.Result = new RedirectToActionResult("Login", "Auth", new { message = "Su cuenta ha sido desactivada" });
            }
        }
    }
}
