    using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;
using System.Text.Json;

namespace PortalInmobiliario.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IDistributedCache _cache;

        public CatalogoController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            IDistributedCache cache)
        {
            _context = context;
            _userManager = userManager;
            _cache = cache;
        }

        // GET: /Catalogo
        public async Task<IActionResult> Index(
            string ciudad, TipoInmueble? tipo, decimal? precioMin,
            decimal? precioMax, int? dormitorios, int page = 1, int pageSize = 5)
        {
            // Guardar filtros en sesión
            HttpContext.Session.SetString("Filtro_Ciudad", ciudad ?? "");
            HttpContext.Session.SetString("Filtro_Tipo", tipo?.ToString() ?? "");
            HttpContext.Session.SetString("Filtro_PrecioMin", precioMin?.ToString() ?? "");
            HttpContext.Session.SetString("Filtro_PrecioMax", precioMax?.ToString() ?? "");
            HttpContext.Session.SetString("Filtro_Dormitorios", dormitorios?.ToString() ?? "");

            // Validaciones server-side
            if (precioMin.HasValue && precioMax.HasValue && precioMin > precioMax)
                ModelState.AddModelError("", "El precio mínimo no puede ser mayor que el precio máximo.");

            if ((precioMin.HasValue && precioMin < 0) || (precioMax.HasValue && precioMax < 0) || (dormitorios.HasValue && dormitorios < 0))
                ModelState.AddModelError("", "Los valores numéricos no pueden ser negativos.");

            if (!ModelState.IsValid)
            {
                ViewBag.Page = 1;
                ViewBag.TotalPages = 1;
                return View(new List<CatalogoItemDto>());
            }

            // Cache key
            var version = await _cache.GetStringAsync("Catalogo_Version") ?? "1";
            string cacheKey = $"Catalogo:v{version}:{ciudad}:{tipo}:{precioMin}:{precioMax}:{dormitorios}:Page{page}";

            // Intentar leer del cache
            var cached = await _cache.GetStringAsync(cacheKey);
            CatalogoCacheDto cacheDto = null;

            if (!string.IsNullOrEmpty(cached))
            {
                cacheDto = JsonSerializer.Deserialize<CatalogoCacheDto>(cached);
            }

            if (cacheDto == null)
            {
                var query = _context.Inmuebles
                    .Where(i => i.Activo)
                    .AsQueryable();

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

                int totalItems = await query.CountAsync();

                var items = await query
                    .OrderBy(i => i.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(i => new CatalogoItemDto
                    {
                        Id = i.Id,
                        Codigo = i.Codigo,
                        Titulo = i.Titulo,
                        Imagen = i.Imagen,
                        Tipo = i.Tipo,
                        Ciudad = i.Ciudad,
                        Direccion = i.Direccion,
                        Dormitorios = i.Dormitorios,
                        Banos = i.Banos,
                        MetrosCuadrados = i.MetrosCuadrados,
                        Precio = i.Precio,
                        Activo = i.Activo,
                        ReservaExpiracionUtc = _context.Reservas
                            .Where(r => r.InmuebleId == i.Id && r.FechaExpiracion > DateTime.UtcNow)
                            .Select(r => (DateTime?)r.FechaExpiracion)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                cacheDto = new CatalogoCacheDto { Items = items, TotalItems = totalItems };

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(cacheDto), cacheOptions);
            }

            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(cacheDto.TotalItems / (double)pageSize);

            return View(cacheDto.Items);
        }

        // GET: /Catalogo/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inmueble == null)
                return NotFound();

            // obtener reserva activa
            var reservaActiva = await _context.Reservas
                .FirstOrDefaultAsync(r => r.InmuebleId == id && r.FechaExpiracion > DateTime.UtcNow);

            inmueble.Reserva = reservaActiva;

            HttpContext.Session.SetInt32("UltimoInmuebleId", id);
            HttpContext.Session.SetString("UltimoInmuebleTitulo", inmueble.Titulo);

            return View(inmueble);
        }

        // GET: /Catalogo/AgendarVisita/5
        [Authorize]
        public IActionResult AgendarVisita(int id)
        {
            var visita = new Visita
            {
                InmuebleId = id,
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now.AddHours(1)
            };
            return View(visita);
        }

        // POST: /Catalogo/AgendarVisita
        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AgendarVisita(Visita visita)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            visita.UsuarioId = user.Id;

            // Convertir a UTC
            visita.FechaInicio = DateTime.SpecifyKind(visita.FechaInicio, DateTimeKind.Local).ToUniversalTime();
            visita.FechaFin = DateTime.SpecifyKind(visita.FechaFin, DateTimeKind.Local).ToUniversalTime();

            // Validaciones
            if (visita.FechaInicio >= visita.FechaFin)
            {
                ModelState.AddModelError("", "La fecha de inicio debe ser menor que la fecha de fin.");
            }

            var inicioLocal = visita.FechaInicio.ToLocalTime().TimeOfDay;
            var finLocal = visita.FechaFin.ToLocalTime().TimeOfDay;

            if (inicioLocal < TimeSpan.FromHours(8) || finLocal > TimeSpan.FromHours(19))
            {
                ModelState.AddModelError("", "Las visitas deben estar dentro del horario laboral (08:00 - 19:00).");
            }

            // Verificar solapamiento
            bool existeSolapada = await _context.Visitas.AnyAsync(v =>
                v.InmuebleId == visita.InmuebleId &&
                v.Estado != EstadoVisita.Cancelada &&
                ((visita.FechaInicio < v.FechaFin) && (visita.FechaFin > v.FechaInicio))
            );

            if (existeSolapada)
            {
                ModelState.AddModelError("", "Ya existe una visita en ese intervalo para este inmueble.");
            }

            if (!ModelState.IsValid)
            {
                return View(visita);
            }

            _context.Visitas.Add(visita);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Visita agendada correctamente.";
            return RedirectToAction("Detalle", new { id = visita.InmuebleId });
        }

        // POST: /Catalogo/Reservar/5
        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reservar(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var inmueble = await _context.Inmuebles
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inmueble == null) return NotFound();

            var existeReservaActiva = await _context.Reservas
                .AnyAsync(r => r.InmuebleId == id && r.FechaExpiracion > DateTime.UtcNow);

            if (existeReservaActiva)
            {
                TempData["Error"] = "Este inmueble ya tiene una reserva activa.";
                return RedirectToAction("Detalle", new { id });
            }

            var reserva = new Reserva
            {
                InmuebleId = id,
                UsuarioId = user.Id,
                FechaCreacion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddHours(48)
            };

            _context.Reservas.Add(reserva);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Reserva creada por 48 horas.";
            return RedirectToAction("Detalle", new { id });
        }
    }
}
