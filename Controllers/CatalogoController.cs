using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CatalogoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Catalogo
        public async Task<IActionResult> Index(string ciudad, TipoInmueble? tipo, decimal? precioMin, decimal? precioMax, int? dormitorios, int page = 1, int pageSize = 5)
        {
            var query = _context.Inmuebles
                .Where(i => i.Activo)
                .AsQueryable();

            // filtros
            if (!string.IsNullOrWhiteSpace(ciudad))
                query = query.Where(i => i.Ciudad.Contains(ciudad));

            if (tipo.HasValue)
                query = query.Where(i => i.Tipo == tipo);

            if (precioMin.HasValue)
                query = query.Where(i => i.Precio >= precioMin);

            if (precioMax.HasValue)
                query = query.Where(i => i.Precio <= precioMax);

            if (dormitorios.HasValue)
                query = query.Where(i => i.Dormitorios >= dormitorios);

            // validaciones server-side
            if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax)
                ModelState.AddModelError("", "El precio mínimo no puede ser mayor al precio máximo.");

            if (dormitorios.HasValue && dormitorios < 0)
                ModelState.AddModelError("", "El número de dormitorios no puede ser negativo.");

            // paginación simple
            int totalItems = await query.CountAsync();
            var inmuebles = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(inmuebles);
        }

        // GET: /Catalogo/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inmueble == null)
                return NotFound();

            return View(inmueble);
        }
    }
}
    