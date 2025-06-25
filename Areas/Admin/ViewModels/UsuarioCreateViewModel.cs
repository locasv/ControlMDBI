using System.ComponentModel.DataAnnotations;

namespace ControlMDBI.Areas.Admin.ViewModels
{
    public class UsuarioCreateViewModel
    {

        [Required(ErrorMessage = "Debe seleccionar un empleado")]
        [Display(Name = "Empleado")]
        public int IdEmpleado { get; set; }

        [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 50 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "El nombre de usuario solo puede contener letras, números, puntos, guiones y guiones bajos")]
        [Display(Name = "Nombre de Usuario")]
        public string NombreUsuario { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [Display(Name = "Contraseña")]
        public string Contrasenia { get; set; }

        [Required(ErrorMessage = "Debe confirmar la contraseña")]
        [Compare("Contrasenia", ErrorMessage = "Las contraseñas no coinciden")]
        [Display(Name = "Confirmar Contraseña")]
        public string ConfirmarContrasenia { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        [Display(Name = "Rol")]
        public string Rol { get; set; }

        [Display(Name = "Fecha de Registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Propiedades para mostrar información del empleado seleccionado
        public string? EmpleadoSeleccionado { get; set; }
        public string? CargoEmpleado { get; set; }
        public string? UnidadEmpleado { get; set; }
    }
}
