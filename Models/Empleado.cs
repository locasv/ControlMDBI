using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlMDBI.Models
{

    public class Empleado
    {
        [Key]
        public int IdEmpleado { get; set; }

        
        [Required(ErrorMessage = "El campo DNI es obligatorio.")]
        [StringLength(8, ErrorMessage = "El DNI debe tener {1} caracteres.")]
        [RegularExpression(@"^[^\s][0-9]+$",ErrorMessage ="DNI debe ser solo numeros, sin espacios")]
        public string DNI { get; set; }

        [Required(ErrorMessage = "Este campo es obligatorio.")]
        [StringLength(30, ErrorMessage = "Este campo no puede exceder los {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZñÑ\s]{3,}$",ErrorMessage = "Los/El nombres/nombre no es válido")]
        public string Nombres { get; set; }

        [Required(ErrorMessage = "Este campo es obligatorio.")]
        [StringLength(30, ErrorMessage = "Este campo no puede exceder los {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZñÑ\s]{3,}$", ErrorMessage = "Los/El apellidos/apellido no es válido")]
        public string Apellidos { get; set; }

        [StringLength(100, ErrorMessage = "La dirección no puede exceder los {1} caracteres")]
        [RegularExpression(@"^[a-zA-ZñÑ0-9\s,.#-]+$",ErrorMessage = "La dirección no es válída")]
        public string? Direccion { get; set; }

        [Required(ErrorMessage = "Este corrreo es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
        [StringLength(50, ErrorMessage = "El correo no puede exceder los {1} caracteres.")]
        public string Correo { get; set; }

        [Required(ErrorMessage = "El cargo es obligatorio.")]
        [StringLength(15, ErrorMessage = "El cargo no puede exceder los {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZñÑ\s]{4,}$", ErrorMessage = "El cargo no es válido")]
        public string Cargo { get; set; }

        [Required(ErrorMessage = "El área a la que pertence es obligatoria.")]
        [StringLength(50, ErrorMessage = "El área no puede exceder los {1} caracteres.")]
        [RegularExpression(@"^[a-zA-ZñÑ\s]{4,}$", ErrorMessage = "El área no es válido")]
        public string Unidad { get; set; }

        [Required(ErrorMessage = "El estado es obligatorio.")]
        public bool Activo { get; set; } = true;


        [Required(ErrorMessage = "La sede es obligatoria.")]
        public int IdSede { get; set; }
        [ForeignKey("IdSede")]
        public Sede? Sede { get; set; }


        // Relación: Un empleado puede tener un usuario (vigilante)
        public ICollection<Usuario>? Usuarios { get; set; }
        // Relación: Un empleado puede tener muchos registros (entradas/salidas)
        public ICollection<RegistroEmpleado>? RegistroEmpleados { get; set; }
    }
}