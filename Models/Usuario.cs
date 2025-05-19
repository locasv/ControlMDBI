using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace ControlMDBI.Models
{

    public class Usuario
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required(ErrorMessage = "Es necesario identifica al empleado")]
        public int IdEmpleado { get; set; }
        [ForeignKey("IdEmpleado")]
        public Empleado? Empleado { get; set; }

        [Required(ErrorMessage = "Es necesario ingresar un nombre de usuario.")]
        [StringLength(50, ErrorMessage = "El nombre de usuario no puede exceder los 50 caracteres.")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = "El nombre de usuario no debe contener espacios.")]
        public string NombreUsuario { get; set; }
        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [StringLength(255)]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = "La contraseña no debe contener espacios.")]
        public string Contrasenia { get; set; }
        [Required(ErrorMessage = "El rol es obligatorio.")]
        [StringLength(50, ErrorMessage = "El rol no puede exceder los 50 caracteres.")]
        public string Rol { get; set; }

        [Required(ErrorMessage = "La fecha de registro es obligatorio.")]
        [DataType(DataType.DateTime, ErrorMessage = "La fecha de registro no es válida.")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Relación: Un usuario (vigilante) puede hacer muchos registros de empleados
        public ICollection<RegistroEmpleado>? RegistroEmpleados { get; set; }

        // Relación: Un usuario (vigilante) puede tener muchos registros de visitantes
        public ICollection<RegistroVisitante>? RegistrarVisitantes { get; set; }

        // Relación: Un usuario (empleado) puede hacer muchas solicitudes para vehiculos
        public ICollection<SolicitudVehiculo>? SolicitudVehiculos { get; set; }

        // Relación: Un usuario (Patrimonio) puede aprobar muchas solicitudes
        public ICollection<AprobacionVehiculo>? AprobacionVehiculos { get; set; }

        // Relacion: Un usuario (vigilante) puede registrar muchos vehiculos
        public ICollection<RegistroVehiculo>? RegistroVehiculos { get; set; }

    }
    public enum ListaRolesUsuario
    {
        Administrador,
        Empleado,
        Patrimonio,
        Vigilante,
        RRHH,
    }
}
