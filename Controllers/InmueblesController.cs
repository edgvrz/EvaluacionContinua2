using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Controllers
{
    public class InmueblesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache;

        public InmueblesController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // GET: /Inmuebles
        public async Task<IActionResult> Index()
        {
            return View(await _context.Inmuebles.ToListAsync());
        }

        // GET: /Inmuebles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Inmuebles/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Inmueble inmueble)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inmueble);
                await _context.SaveChangesAsync();

                // 🔹 Invalidar la cache
                await _cache.SetStringAsync("Catalogo_Version", DateTime.UtcNow.Ticks.ToString());

                return RedirectToAction(nameof(Index));
            }
            return View(inmueble);
        }

        // GET: /Inmuebles/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null) return NotFound();

            return View(inmueble);
        }

        // POST: /Inmuebles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Inmueble inmueble)
        {
            if (id != inmueble.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inmueble);
                    await _context.SaveChangesAsync();

                    // 🔹 Invalidar la cache
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
            return View(inmueble);
        }

        // GET: /Inmuebles/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var inmueble = await _context.Inmuebles.FirstOrDefaultAsync(m => m.Id == id);
            if (inmueble == null) return NotFound();

            return View(inmueble);
        }

        // POST: /Inmuebles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble != null)
            {
                _context.Inmuebles.Remove(inmueble);
                await _context.SaveChangesAsync();

                // 🔹 Invalidar la cache
                await _cache.SetStringAsync("Catalogo_Version", DateTime.UtcNow.Ticks.ToString());
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Inmuebles/ToggleActivo/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(int id)
        {
            var inmueble = await _context.Inmuebles.FindAsync(id);
            if (inmueble == null) return NotFound();

            inmueble.Activo = !inmueble.Activo;
            _context.Update(inmueble);
            await _context.SaveChangesAsync();

            // 🔹 Invalidar la cache
            await _cache.SetStringAsync("Catalogo_Version", DateTime.UtcNow.Ticks.ToString());

            return RedirectToAction(nameof(Index));
        }
    }
}
