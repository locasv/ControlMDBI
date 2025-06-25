namespace ControlMDBI.Areas.Admin.ViewModels
{
    public class UsuarioEditViewModel
    {
        public int IdUsuario { get; set; }
        public int IdEmpleado { get; set; }

        public string NombreUsuario { get; set; }
        // Contraseña actual (solo para mostrar que existe)
        public string ContraseniaActual { get; set; }

        // Nueva contraseña (para cambios)
        public string? ContraseniaNueva { get; set; }

        // Confirmación de nueva contraseña
        public string? ConfirmarContrasenia { get; set; }
        public string Rol { get; set; }
        public DateTime FechaRegistro { get; set; }

        // Propiedades del empleado relacionadas
        public string? NombresEmpleado { get; set; }
        public string? ApellidosEmpleado { get; set; }
        public string? Cargo { get; set; }
        public string? Unidad { get; set; }
        public bool ActivoEmpleado { get; set; }
        // Indica si el usuario tiene contraseña establecida
        public bool TieneContrasenia { get; set; }
    }
}
