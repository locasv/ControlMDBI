using ControlMDBI.Models;

namespace ControlMDBI.Areas.Admin.ViewModels
{
    public class EmpleadoPaginadoViewModel
    {
        public List<Empleado> Empleados { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public string? BusquedaNombre { get; set; }
        public string? BusquedaDNI { get; set; }
    }
  
}
