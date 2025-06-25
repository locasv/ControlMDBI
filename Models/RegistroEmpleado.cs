using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlMDBI.Models
{
    public class RegistroEmpleado
    {
        [Key]
        public int IdRegistroEmpleado { get; set; }
        //Empleado registrado lo rescata de la base de datos conn su DNI
        public int IdEmpleado { get; set; }
        [ForeignKey("IdEmpleado")]
        public Empleado Empleado { get; set; }

        // Es necesario el usuario (vigilante) que registra el empleado
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; }

       

        [Required(ErrorMessage = "El tipo de registro es obligatorio.")]
        [StringLength(50, ErrorMessage = "El tipo de registro no puede exceder los {1} caracteres.")]
        public string TipoRegistro { get; set; }

        [Required(ErrorMessage = "La razon de ingreso es obligatoria.")]
        [StringLength(50, ErrorMessage = "La razon de ingreso no puede exceder los {1} caracteres.")]
        public string Razon { get; set; }

        [Required(ErrorMessage = "La fecha y hora de registro es obligatoria.")]
        [DataType(DataType.DateTime, ErrorMessage = "La fecha y hora de registro no es válida.")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime FechaRegistroEmpleado { get; set; } = DateTime.Now;

        public bool Objeto { get; set; }
        [StringLength(50, ErrorMessage = "El codigo del objeto no puede exceder los {1} caracteres.")]
        public string? CodigoObjeto { get; set; }
        [StringLength(200, ErrorMessage = "Las observaciones no puede exceder los {1} caracteres.")]
        public string? Observaciones { get; set; }

    }

    public enum ListaTipoRegistro
    {
        Ingreso,
        Salida,
    }
}
