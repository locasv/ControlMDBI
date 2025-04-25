using ControlMDBI.Models;
using Microsoft.EntityFrameworkCore;

namespace ControlMDBI.Data
{
    public class ControlMDBIDbContext : DbContext

    {
        public ControlMDBIDbContext(DbContextOptions<ControlMDBIDbContext> options) : base(options)
        {
        }
        public DbSet<AprobacionVehiculo> AprobacionVehiculo { get; set; }
        public DbSet<Empleado> Empleado { get; set; }
        public DbSet<Sede> Sede { get; set; }
        public DbSet<SolicitudVehiculo> SolicitudVehiculo { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<RegistroVehiculo> RegistroVehiculo { get; set; }
        public DbSet<RegistroEmpleado> RegistroEmpleado { get; set; }
        public DbSet<RegistroVisitante> RegistroVisitante { get; set; }
        public DbSet<Vehiculo> Vehiculo { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>().ToTable("Usuario");
            modelBuilder.Entity<Empleado>().ToTable("Empleado");
            modelBuilder.Entity<Sede>().ToTable("Sede");
            modelBuilder.Entity<SolicitudVehiculo>().ToTable("SolicitudVehiculo");
            modelBuilder.Entity<AprobacionVehiculo>().ToTable("AprobacionVehiculo");
            modelBuilder.Entity<RegistroVehiculo>().ToTable("RegistroVehiculo");
            modelBuilder.Entity<RegistroEmpleado>().ToTable("RegistroEmpleado");
            modelBuilder.Entity<RegistroVisitante>().ToTable("RegistroVisitante");
            modelBuilder.Entity<Vehiculo>().ToTable("Vehiculo");
            // Evitar error de múltiples rutas de eliminación en cascada
            modelBuilder.Entity<Empleado>()
                .HasOne(e => e.Sede)
                .WithMany(s => s.Empleados)
                .HasForeignKey(e => e.IdSede)
                .OnDelete(DeleteBehavior.Restrict);


            base.OnModelCreating(modelBuilder);
        }

    }
}
