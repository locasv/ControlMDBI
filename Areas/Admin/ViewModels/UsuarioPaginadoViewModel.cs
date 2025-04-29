using ControlMDBI.Models;

namespace ControlMDBI.Areas.Admin.ViewModels
{
    public class UsuarioPaginadoViewModel
    {
        public List<Usuario> Usuarios { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public string? BusquedaNombre { get; set; }
    }
}
