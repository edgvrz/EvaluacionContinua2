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
    public class PanelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public PanelController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // Listar inmuebles
        public async Task<IActionResult> Inmuebles()
        {
            var inmuebles = await _context.Inmuebles.ToListAsync();
            return View(inmuebles);
        }

        // Crear - GET
        public IActionResult CreateInmueble()
        {
            return View(new Inmueble());
        }

        // Crear - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateInmueble(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                _context.Inmuebles.Add(inmueble);
                await _context.SaveChangesAsync();
                await _cache.SetStringAsync("Catalogo_Version", DateTime.UtcNow.Ticks.ToString());
                return RedirectToAction(nameof(Inmuebles));
            }
            return View(inmueble);
        }

        // Editar - GET
        public async Task<IActionResult> EditInmueble(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null) return NotFound();
            return View(inmueble);
        }

        // Editar - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditInmueble(int id, Inmueble inmueble)
        {
            if (id != inmueble.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inmueble);
                    await _context.SaveChangesAsync();
                    await _cache.SetStringAsync("Catalogo_Version", DateTime.UtcNow.Ticks.ToString());
                    return RedirectToAction(nameof(Inmuebles));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InmuebleExists(inmueble.Id))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(inmueble);
        }

        // Eliminar - GET
        public async Task<IActionResult> DeleteInmueble(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null) return NotFound();
            return View(inmueble);
        }

        // Eliminar - POST
        [HttpPost, ActionName("DeleteInmueble")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteInmuebleConfirmed(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble != null)
            {
                _context.Inmuebles.Remove(inmueble);
                await _context.SaveChangesAsync();
                await _cache.SetStringAsync("Catalogo_Version", DateTime.UtcNow.Ticks.ToString());
            }
            return RedirectToAction(nameof(Inmuebles));
        }

        private bool InmuebleExists(int id)
        {
            return _context.Inmuebles.Any(e => e.Id == id);
        }
    }
}
