using System.ComponentModel.DataAnnotations;

namespace PortalInmobiliario.Models
{
    public class Inmueble
    {
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Codigo { get; set; }

        [Required]
        public string Titulo { get; set; }

        public string Imagen { get; set; }

        [Required]
        public TipoInmueble Tipo { get; set; }

        public string Ciudad { get; set; }
        public string Direccion { get; set; }

        [Range(0, int.MaxValue)]
        public int Dormitorios { get; set; }

        [Range(0, int.MaxValue)]
        public int Banos { get; set; }

        [Range(0.01, double.MaxValue)]
        public double MetrosCuadrados { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Precio { get; set; }

        public bool Activo { get; set; } = true;

        public List<Visita> Visitas { get; set; }
        public Reserva Reserva { get; set; }
    }
}
