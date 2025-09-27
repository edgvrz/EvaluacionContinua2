namespace PortalInmobiliario.Models
{
    public class CatalogoItemDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Titulo { get; set; }
        public string Imagen { get; set; }
        public TipoInmueble Tipo { get; set; }
        public string Ciudad { get; set; }
        public string Direccion { get; set; }
        public int Dormitorios { get; set; }
        public int Banos { get; set; }
        public double MetrosCuadrados { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        public DateTime? ReservaExpiracionUtc { get; set; }
    }
}
