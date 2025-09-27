using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Inmueble> Inmuebles { get; set; }
        public DbSet<Visita> Visitas { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Código único
            builder.Entity<Inmueble>().HasIndex(i => i.Codigo).IsUnique();

            // Reserva: un inmueble solo puede tener una
            builder.Entity<Reserva>()
                .HasOne(r => r.Inmueble)
                .WithOne(i => i.Reserva)
                .HasForeignKey<Reserva>(r => r.InmuebleId);

            // Relación visitas
            builder.Entity<Visita>()
                .HasOne(v => v.Inmueble)
                .WithMany(i => i.Visitas)
                .HasForeignKey(v => v.InmuebleId);

            // Restricciones de datos
            builder.Entity<Inmueble>().HasCheckConstraint("CK_Inmueble_Precio", "Precio > 0");
            builder.Entity<Inmueble>().HasCheckConstraint("CK_Inmueble_Metros", "MetrosCuadrados > 0");
            builder.Entity<Visita>().HasCheckConstraint("CK_Visita_Fechas", "FechaInicio < FechaFin");

            // Guardar enums como strings
            builder.Entity<Inmueble>().Property(i => i.Tipo).HasConversion<string>();
            builder.Entity<Visita>().Property(v => v.Estado).HasConversion<string>();

            // Semilla de inmuebles
            builder.Entity<Inmueble>().HasData(
                new Inmueble { Id = 1, Codigo = "A100", Titulo = "Depto céntrico 2D", Tipo = TipoInmueble.Departamento, Ciudad = "Lima", Direccion="Av. Ejemplo 123", Dormitorios=2, Banos=1, MetrosCuadrados=60, Precio=85000m, Activo=true },
                new Inmueble { Id = 2, Codigo = "B200", Titulo = "Casa familiar", Tipo = TipoInmueble.Casa, Ciudad = "Arequipa", Direccion="Calle Falsa 456", Dormitorios=3, Banos=2, MetrosCuadrados=120, Precio=150000m, Activo=true },
                new Inmueble { Id = 3, Codigo = "C300", Titulo = "Oficina moderna", Tipo = TipoInmueble.Oficina, Ciudad = "Lima", Direccion="Zona Financiera", Dormitorios=0, Banos=1, MetrosCuadrados=45, Precio=60000m, Activo=true }
            );
        }
    }
}
