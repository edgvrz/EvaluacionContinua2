using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Areas.Broker.Controllers
{
    [Area("Broker")]
    [Authorize(Roles = "Broker")]
    public class ReservasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public ReservasController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Inmueble)
                .Where(r => r.FechaExpiracion > DateTime.UtcNow)
                .OrderBy(r => r.FechaExpiracion)
                .ToListAsync();

            return View(reservas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Liberar(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);
            if (reserva == null) return NotFound();

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            // invalidar cache del catálogo
            await _cache.SetStringAsync("Catalogo_Version", DateTime.UtcNow.Ticks.ToString());

            return RedirectToAction(nameof(Index));
        }
    }
}
