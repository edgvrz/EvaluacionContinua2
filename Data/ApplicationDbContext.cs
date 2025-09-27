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
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Visita> Visitas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Inmueble>().HasData(
                new Inmueble {
                    Id = 1,
                    Codigo = "A100",
                    Titulo = "Depto céntrico 2D",
                    Tipo = TipoInmueble.Departamento, // ✅ enum
                    Ciudad = "Lima",
                    Direccion = "Av. Ejemplo 123",
                    Dormitorios = 2,
                    Banos = 1,
                    MetrosCuadrados = 60,
                    Precio = 85000m,
                    Activo = true,
                    Imagen = "https://via.placeholder.com/300"
                },
                new Inmueble {
                    Id = 2,
                    Codigo = "B200",
                    Titulo = "Casa familiar",
                    Tipo = TipoInmueble.Casa, // ✅ enum
                    Ciudad = "Arequipa",
                    Direccion = "Calle Falsa 456",
                    Dormitorios = 3,
                    Banos = 2,
                    MetrosCuadrados = 120,
                    Precio = 150000m,
                    Activo = true,
                    Imagen = "https://via.placeholder.com/300"
                },
                new Inmueble {
                    Id = 3,
                    Codigo = "C300",
                    Titulo = "Oficina moderna",
                    Tipo = TipoInmueble.Oficina, // ✅ enum
                    Ciudad = "Lima",
                    Direccion = "Zona Financiera",
                    Dormitorios = 0,
                    Banos = 1,
                    MetrosCuadrados = 45,
                    Precio = 60000m,
                    Activo = true,
                    Imagen = "https://via.placeholder.com/300"
                }
            );
        }
    }
}
