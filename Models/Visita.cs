using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace PortalInmobiliario.Models
{
    public class Visita
    {
        public int Id { get; set; }

        public int InmuebleId { get; set; }
        public Inmueble Inmueble { get; set; }

        public string UsuarioId { get; set; }
        public IdentityUser Usuario { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        public EstadoVisita Estado { get; set; } = EstadoVisita.Solicitada;

        public string Notas { get; set; }
    }
}
