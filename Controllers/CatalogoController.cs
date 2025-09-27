using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Controllers
{
    public class CatalogoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CatalogoController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
                .Include(i => i.Reserva)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inmueble == null)
                return NotFound();

            return View(inmueble);
        }

        // GET: /Catalogo/AgendarVisita/5
        [Authorize]
        public IActionResult AgendarVisita(int id)
        {
            var visita = new Visita { InmuebleId = id, FechaInicio = DateTime.Now, FechaFin = DateTime.Now.AddHours(1) };
            return View(visita);
        }

        // POST: /Catalogo/AgendarVisita
        [Authorize, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AgendarVisita(Visita visita)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            visita.UsuarioId = user.Id;

            // Validaciones
            if (visita.FechaInicio >= visita.FechaFin)
            {
                ModelState.AddModelError("", "La fecha de inicio debe ser menor que la fecha de fin.");
            }

            if (visita.FechaInicio.Hour < 8 || visita.FechaFin.Hour > 19)
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
                .Include(i => i.Reserva)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (inmueble == null) return NotFound();

            if (inmueble.Reserva != null && inmueble.Reserva.FechaExpiracion > DateTime.UtcNow)
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

