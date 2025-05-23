using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlMDBI.Models
{
    public class AprobacionVehiculo
    {
        [Key]
        public int IdAprobacionVehiculo { get; set; }


        [Required(ErrorMessage = "La solicitud es obligatoria.")]
        public int IdSolicitudVehiculo { get; set; }
        [ForeignKey("IdSolicitudVehiculo")]
        public SolicitudVehiculo? SolicitudVehiculo { get; set; }

        // El empleado que es usuario con el cargo de jefe de la unidad/subgerencia de patrimonio que aprueba la solicitud como es usuario del sistema se añade automaticamente
        [Required(ErrorMessage = "El Usuario que aprueba la solicitud es obligatorio.")]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "La fecha de aprobación es obligatoria.")]
        [DataType(DataType.DateTime, ErrorMessage = "La fecha de aprobación no es válida.")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm tt}", ApplyFormatInEditMode = false)]
        [Display(Name = "Fecha de Salida")]
        public DateTime FechaAprobacion { get; set; }

        [StringLength(200, ErrorMessage = "Las observaciones no pueden exceder los {1} caracteres.")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "El estado de la solicitud es obligatorio.")]
        [StringLength(15, ErrorMessage = "El estado no puede exceder los 50 caracteres.")]
        public string Estado { get; set; }

        // Relacion: Una aprobacion puede tener un registro de vehiculo
        public ICollection<RegistroVehiculo>? RegistroVehiculos { get; set; }
    }
    public enum ListaEstadoAprobacion
    {
        Aprobado,
        Rechazado,
        Pendiente,
    }
}
