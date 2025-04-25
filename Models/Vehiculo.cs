using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlMDBI.Models
{
    public class Vehiculo
    {
        [Key]
        public int IdVehiculo { get; set; }
        [Required(ErrorMessage = "La placa es obligatoria.")]
        [StringLength(10, ErrorMessage = "La placa no puede exceder los {1} caracteres.")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = "La placa no debe contener espacios.")]
        public string Placa { get; set; }

        [Required(ErrorMessage = "La marca del vehiculo es obligatorio.")]
        [StringLength(20, ErrorMessage = "La marca no puede exceder los {1} caracteres.")]
        public string Marca { get; set; }

        [Required(ErrorMessage = "El estado del vehiculo es obligatorio.")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder los {1} caracteres.")]
        public string Estado { get; set; }

        [Required(ErrorMessage = "La sede donde se encuentra el vehiculo es obligatorio.")]
        public int IdSede { get; set; }
        [ForeignKey("IdSede")]
        public Sede? Sede { get; set; }

        // Relación: Un vehiculo puede tener muchas solicitudes eso depende de su estado
        public ICollection<SolicitudVehiculo>? SolicitudVehiculos { get; set; }

    }

    public enum ListaEstadoVehiculo
    {
        Activo,
        Inactivo,
        Mantenimiento
    }
}
