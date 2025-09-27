using System.ComponentModel.DataAnnotations;

namespace PortalInmobiliario.Models
{
    public class Inmueble
    {
        public int Id { get; set; }

        [Required]
        public string Codigo { get; set; } = "";

        [Required]
        public string Titulo { get; set; } = "";

        [Required]
        public TipoInmueble Tipo { get; set; }

        [Required]
        public string Ciudad { get; set; } = "";

        [Required]
        public string Direccion { get; set; } = "";

        [Range(0, 50)]
        public int Dormitorios { get; set; }

        [Range(0, 20)]
        public int Banos { get; set; }

        [Range(1, int.MaxValue)]
        public int MetrosCuadrados { get; set; }

        [Range(1, double.MaxValue)]
        public decimal Precio { get; set; }

        public bool Activo { get; set; } = true;

        [Required]
        public string Imagen { get; set; } = "";

        // Relaciones
        public Reserva? Reserva { get; set; }
        public ICollection<Visita> Visitas { get; set; } = new List<Visita>();
    }
}
