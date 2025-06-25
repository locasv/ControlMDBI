using System.ComponentModel.DataAnnotations;

namespace ControlMDBI.Models
{
    public class Sede
    {
        [Key]
        public int IdSede { get; set; }

        [Required(ErrorMessage = "El nombre de la sede es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre de la sede no puede exceder los 100 caracteres.")]
        [RegularExpression(@"^[a-zA-ZñÑ\s]{4,}$", ErrorMessage = "Los/El nombres/nombre no es válido")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "La dirección de la sede es obligatoria.")]
        [StringLength(200, ErrorMessage = "La dirección de la sede no puede exceder los 200 caracteres.")]
        [RegularExpression(@"^[a-zA-ZñÑ0-9\s,./#-]+$", ErrorMessage = "La dirección no es válída")]
        public string Direccion { get; set; }

        // Relación: Una sede puede tener muchos empleados
        public ICollection<Empleado>? Empleados { get; set; }

        // Relación: Una sede puede tener muchos vehículos
        public ICollection<Vehiculo>? Vehiculos { get; set; }
    }
}