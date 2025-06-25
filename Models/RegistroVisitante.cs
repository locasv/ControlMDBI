using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlMDBI.Models
{
    public class RegistroVisitante
    {
        [Key]
        public int IdRegistroVisitante { get; set; }
        [Required(ErrorMessage = "El campo DNI es obligatorio.")]
        [StringLength(8, ErrorMessage = "El DNI debe tener {1} caracteres.")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = "El DNI no puede tener espacios")]
        public string DNI { get; set; }
        //El que registra el visitante es un usuario (vigilante)
        public int IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "La fecha y hora de registro es obligatoria.")]
        [DataType(DataType.DateTime, ErrorMessage = "La fecha y hora de registro no es válida.")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy hh:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime FechaRegistroVisitante { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "La razon de visita es obligatoria.")]
        [StringLength(200, ErrorMessage = "La razon de visita no puede exceder los {1} caracteres.")]
        public string RazonVisita { get; set; }

    }
}
