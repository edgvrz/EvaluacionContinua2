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
    public class InmueblesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public InmueblesController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IActionResult> Index()
        {
            var inmuebles = await _context.Inmuebles.ToListAsync();
            return View(inmuebles);
        }

        public IActionResult Create() => View(new Inmueble());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inmueble inmueble)
        {
            if (!ModelState.IsValid) return View(inmueble);
            _context.Add(inmueble);
            await _context.SaveChangesAsync();
            await _cache.SetStringAsync("Catalogo_Version", DateTime.UtcNow.Ticks.ToString());
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null) return NotFound();
            return View(inmueble);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inmueble inmueble)
        {
            if (id != inmueble.Id) return NotFound();
            if (!ModelState.IsValid) return View(inmueble);

            try
            {
                _context.Update(inmueble);
                await _context.SaveChangesAsync();
                await _cache.SetStringAsync("Catalogo_Version", DateTime.UtcNow.Ticks.ToString());
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Inmuebles.Any(e => e.Id == inmueble.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null) return NotFound();
            return View(inmueble);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble != null)
            {
                _context.Inmuebles.Remove(inmueble);
                await _context.SaveChangesAsync();
                await _cache.SetStringAsync("Catalogo_Version", DateTime.UtcNow.Ticks.ToString());
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null) return NotFound();

            inmueble.Activo = !inmueble.Activo;
            _context.Update(inmueble);
            await _context.SaveChangesAsync();
            await _cache.SetStringAsync("Catalogo_Version", DateTime.UtcNow.Ticks.ToString());

            return RedirectToAction(nameof(Index));
        }
    }
}
