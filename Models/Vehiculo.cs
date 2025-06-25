using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlMDBI.Models
{
    public class Vehiculo
    {
        [Key]
        public int IdVehiculo { get; set; }
        [Required(ErrorMessage = "La placa es obligatoria.")]
        [StringLength(7, ErrorMessage = "La placa no puede exceder los {1} caracteres.")]
        [RegularExpression(@"^[A-Z0-9]{3}-[A-Z0-9]{3}$", ErrorMessage = "La placa debe tener el formato 'A3I-36N'.")]
        public string Placa { get; set; }
        [StringLength(20, ErrorMessage = "La marca no puede exceder los {1} caracteres.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúñÑ\s]{2,20}$", ErrorMessage = "La marca debe contener solo letras y espacios.")]
        public string? Marca { get; set; }
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúñÑ\s]{2,20}$", ErrorMessage = "El modelo debe contener solo letras y espacios.")]
        [StringLength(20, ErrorMessage = "El modelo no puede exceder los {1} caracteres.")]
        public string? Modelo { get; set; }

        [StringLength(10, ErrorMessage = "El color no puede exceder los {1} caracteres.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúñÑ\s]{2,10}$", ErrorMessage = "El color debe contener solo letras y espacios.")]

        public string? Color {  get; set; }

        [Required(ErrorMessage = "El estado del vehiculo es obligatorio.")]
        [StringLength(20, ErrorMessage = "El estado no puede exceder los {1} caracteres.")]
        public string Estado { get; set; }

        [Required(ErrorMessage = "La sede es obligatoria.")]
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
        Mantenimiento,
        Circulando
    }
}
