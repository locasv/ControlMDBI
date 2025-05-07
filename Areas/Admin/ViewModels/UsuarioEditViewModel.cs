namespace ControlMDBI.Areas.Admin.ViewModels
{
    public class UsuarioEditViewModel
    {
        public int IdUsuario { get; set; }
        public int IdEmpleado { get; set; }

        public string NombreUsuario { get; set; }
        public string Contrasenia { get; set; }
        public string Rol { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Propiedades del empleado relacionadas
        public string? NombresEmpleado { get; set; }
        public string? ApellidosEmpleado { get; set; }
        public string? Cargo { get; set; }
        public string? Unidad { get; set; }
        public bool ActivoEmpleado { get; set; }
    }
}
