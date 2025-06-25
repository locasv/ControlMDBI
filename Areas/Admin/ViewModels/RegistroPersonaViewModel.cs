// ViewModels/RegistroPersonaViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace ControlMDBI.Areas.Admin.ViewModels
{
    public class RegistroPersonaViewModel
    {
        public int IdRegistroEmpleado { get; set; }
        // Para Empleados
        [Display(Name = "DNI del Empleado")]
        public string DniEmpleado { get; set; }

        [Required(ErrorMessage = "El tipo de registro es obligatorio.")]
        [Display(Name = "Tipo de Registro")]
        public string TipoRegistroEmpleado { get; set; }

        [Required(ErrorMessage = "La razón es obligatoria.")]
        [Display(Name = "Razón")]
        [StringLength(50, ErrorMessage = "La razón no puede exceder los {1} caracteres.")]
        public string RazonEmpleado { get; set; }

        [Display(Name = "Porta Objeto")]
        public bool PortaObjeto { get; set; }

        [Display(Name = "Código del Objeto")]
        [StringLength(50, ErrorMessage = "El código del objeto no puede exceder los {1} caracteres.")]
        public string? CodigoObjeto { get; set; }

        [Display(Name = "Observaciones")]
        [StringLength(200, ErrorMessage = "Las observaciones no pueden exceder los {1} caracteres.")]
        public string? ObservacionesEmpleado { get; set; }

        // Para Visitantes
        public int IdRegistroVisitante { get; set; }

        [Required(ErrorMessage = "El campo DNI es obligatorio.")]
        [StringLength(8, ErrorMessage = "El DNI debe tener {1} caracteres.")]
        [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El DNI debe contener 8 dígitos")]
        [Display(Name = "DNI del Visitante")]
        public string DniVisitante { get; set; }

        [Required(ErrorMessage = "La razón de visita es obligatoria.")]
        [StringLength(200, ErrorMessage = "La razón de visita no puede exceder los {1} caracteres.")]
        [Display(Name = "Razón de Visita")]
        public string RazonVisita { get; set; }
    }
}