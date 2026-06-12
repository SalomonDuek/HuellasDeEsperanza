using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HuellasDeEsperanza.Data;
using HuellasDeEsperanza.Models;

namespace HuellasDeEsperanza.Controllers
{
    public class SolicitudController : Controller
    {
        private readonly HDEDbContext _context;

        public SolicitudController(HDEDbContext context)
        {
            _context = context;
        }

        // GET: Solicitud
        public async Task<IActionResult> Index()
        {
            var hDEDbContext = _context.Solicitudes.Include(s => s.AuditadoPor).Include(s => s.Mascota).Include(s => s.Usuario);
            return View(await hDEDbContext.ToListAsync());
        }

        // GET: Solicitud/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitud = await _context.Solicitudes
                .Include(s => s.AuditadoPor)
                .Include(s => s.Mascota)
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (solicitud == null)
            {
                return NotFound();
            }

            return View(solicitud);
        }

        // GET: Solicitud/Create
        public IActionResult Create()
        {
            ViewData["AuditadoPorId"] = new SelectList(_context.Usuarios, "Id", "Contrasenia");
            ViewData["MascotaId"] = new SelectList(_context.Mascotas, "Id", "Descripcion");
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Contrasenia");
            return View();
        }

        // POST: Solicitud/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Tipo,Estado,FechaCreacion,FechaAuditoria,DetalleRechazo,UsuarioId,MascotaId,AuditadoPorId")] Solicitud solicitud)
        {
            if (ModelState.IsValid)
            {
                _context.Add(solicitud);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuditadoPorId"] = new SelectList(_context.Usuarios, "Id", "Contrasenia", solicitud.AuditadoPorId);
            ViewData["MascotaId"] = new SelectList(_context.Mascotas, "Id", "Descripcion", solicitud.MascotaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Contrasenia", solicitud.UsuarioId);
            return View(solicitud);
        }

        // GET: Solicitud/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitud = await _context.Solicitudes.FindAsync(id);
            if (solicitud == null)
            {
                return NotFound();
            }
            ViewData["AuditadoPorId"] = new SelectList(_context.Usuarios, "Id", "Contrasenia", solicitud.AuditadoPorId);
            ViewData["MascotaId"] = new SelectList(_context.Mascotas, "Id", "Descripcion", solicitud.MascotaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Contrasenia", solicitud.UsuarioId);
            return View(solicitud);
        }

        // POST: Solicitud/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Tipo,Estado,FechaCreacion,FechaAuditoria,DetalleRechazo,UsuarioId,MascotaId,AuditadoPorId")] Solicitud solicitud)
        {
            if (id != solicitud.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(solicitud);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SolicitudExists(solicitud.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AuditadoPorId"] = new SelectList(_context.Usuarios, "Id", "Contrasenia", solicitud.AuditadoPorId);
            ViewData["MascotaId"] = new SelectList(_context.Mascotas, "Id", "Descripcion", solicitud.MascotaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Contrasenia", solicitud.UsuarioId);
            return View(solicitud);
        }

        // GET: Solicitud/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitud = await _context.Solicitudes
                .Include(s => s.AuditadoPor)
                .Include(s => s.Mascota)
                .Include(s => s.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (solicitud == null)
            {
                return NotFound();
            }

            return View(solicitud);
        }

        // POST: Solicitud/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var solicitud = await _context.Solicitudes.FindAsync(id);
            if (solicitud != null)
            {
                _context.Solicitudes.Remove(solicitud);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SolicitudExists(int id)
        {
            return _context.Solicitudes.Any(e => e.Id == id);
        }
    }
}
