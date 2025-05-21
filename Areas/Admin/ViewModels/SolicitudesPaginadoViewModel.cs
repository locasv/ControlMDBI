using ControlMDBI.Models;
namespace ControlMDBI.Areas.Admin.ViewModels
{
    public class SolicitudesPaginadoViewModel
    {
        public List<SolicitudVehiculo> Solicitudes { get; set; }
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }
        public string? BusquedaNombres { get; set; }
        public string? BusquedaPlacaVehiculo { get; set; }
    }
}
