using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlMDBI.Models
{
    public class RegistroVehiculo
    {
        [Key]
        public int IdRegistroVehiculo { get; set; }
        //Es necesario el codigo de aprobacion para el registro de vehiculo ya que ahi se puede obtener el vehiculo para el registro
        [Required(ErrorMessage = "La aprobacion es obligatoria.")]
        public int IdAprobacionVehiculo { get; set; }
        [ForeignKey("IdAprobacionVehiculo")]
        public AprobacionVehiculo? AprobacionVehiculo { get; set; }

        //Es necesario saber quien (vigilante) registro el vehiculo
        [Required(ErrorMessage = "El usuario que registra el vehiculo es obligatorio.")]
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }
        //Es necesario saber el vehiculo que se registro, pero eso viene desde solicitud que pasa por la aprobacion.

        [Required(ErrorMessage = "La fecha del registro del vehiculo es obligatorio.")]
        [DataType(DataType.DateTime, ErrorMessage = "La fecha del registro no es válida.")]
        public DateTime FechaRegistroVehiculo { get; set; }


        [Required(ErrorMessage = "El tipo de registro del vehiculo es obligatorio.")]
        [StringLength(15, ErrorMessage = "El tipo de registro no puede exceder los {1} caracteres.")]
        public string TipoRegistro { get; set; }

   
        [StringLength(200, ErrorMessage = "Las observaciones no puede exceder los {1} caracteres.")]
        public string? Observaciones { get; set; }


    }
    public enum ListaTipoRegistroVehiculo
    {
        Ingreso,
        Salida,
    }
}
