namespace PortalInmobiliario.Models
{
    public class CatalogoCacheDto
    {
        public List<CatalogoItemDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
    }
}
