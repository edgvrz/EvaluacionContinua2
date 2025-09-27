using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortalInmobiliario.Data;
using PortalInmobiliario.Models;

namespace PortalInmobiliario.Areas.Broker.Controllers
{
    [Area("Broker")]
    [Authorize(Roles = "Broker")]
    public class VisitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VisitasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // /Broker/Visitas/Agenda?day=2025-09-26 (opcional)
        public async Task<IActionResult> Agenda(DateTime? day)
        {
            var target = day?.Date ?? DateTime.Today;
            var visitas = await _context.Visitas
                .Include(v => v.Inmueble)
                .Where(v => v.FechaInicio.Date == target && v.Estado != EstadoVisita.Cancelada)
                .OrderBy(v => v.FechaInicio)
                .ToListAsync();

            return View(visitas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, EstadoVisita estado)
        {
            var visita = await _context.Visitas.FindAsync(id);
            if (visita == null) return NotFound();

            visita.Estado = estado;
            _context.Update(visita);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Agenda));
        }
    }
}
