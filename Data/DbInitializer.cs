using ControlMDBI.Models;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace ControlMDBI.Data
{
    public static class DbInitializer
    {
        public static void Inialize(ControlMDBIDbContext _context)
        {
            // Asegurar que la sede principal exista
            var sede = _context.Sede.FirstOrDefault(s => s.Nombre == "Principal");
            if (sede == null)
            {
                sede = new Sede
                {
                    Nombre = "Principal",
                    Direccion = "Av.Atahualpa s/n"
                };
                _context.Sede.Add(sede);
                _context.SaveChanges();
            }

            // Verificar y crear empleado administrador
            var empleadoAdmin = _context.Empleado
                .FirstOrDefault(e => e.DNI == "00000000");

            if (empleadoAdmin == null)
            {
                empleadoAdmin = new Empleado
                {
                    DNI = "00000000",
                    Nombres = "Administrador",
                    Apellidos = "Administrador",
                    Direccion = "Dirección del administrador",
                    Correo = "admin@admin.com",
                    Cargo = "Jefe",
                    Unidad = "TI",
                    IdSede = sede.IdSede,
                    Activo = true
                };
                _context.Empleado.Add(empleadoAdmin);
                _context.SaveChanges();
            }

            // Verificar y crear usuario administrador
            var usuarioAdmin = _context.Usuario.Include(u => u.Empleado)
    .FirstOrDefault(u => u.NombreUsuario.ToLower() == "admin".ToLower());

            if (usuarioAdmin == null)
            {
                usuarioAdmin = new Usuario
                {
                    IdEmpleado = empleadoAdmin.IdEmpleado,
                    NombreUsuario = "admin", // En minúsculas para consistencia
                    Contrasenia = BCrypt.Net.BCrypt.HashPassword("admin"),
                    Rol = ListaRolesUsuario.Administrador.ToString(),
                    FechaRegistro = DateTime.Now
                };
                _context.Usuario.Add(usuarioAdmin);
                _context.SaveChanges();

            }
        }
    }
}
