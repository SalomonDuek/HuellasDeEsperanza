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
            var solicitudes = await _context.Solicitudes
                .Include(s => s.AuditadoPor)
                .Include(s => s.Mascota)
                .Include(s => s.Usuario)
                .ToListAsync();

            return View(solicitudes);
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
            ViewData["AuditadoPorId"] = new SelectList(_context.Usuarios, "Id", "Nombre");
            ViewData["MascotaId"] = new SelectList(_context.Mascotas, "Id", "Descripcion");
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Nombre");
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
            ViewData["AuditadoPorId"] = new SelectList(_context.Usuarios, "Id", "Nombre", solicitud.AuditadoPorId);
            ViewData["MascotaId"] = new SelectList(_context.Mascotas, "Id", "Descripcion", solicitud.MascotaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Nombre", solicitud.UsuarioId);
            return View(solicitud);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSolicitudAdopcion(int mascotaId)
        {
            var solicitud = new Solicitud
            {
                Tipo = TipoSolicitud.Adopcion,
                Estado = EstadoSolicitud.Pendiente,
                FechaCreacion = DateTime.Now,

                // cambiar por el usuario logueado después
                UsuarioId = 3,

                MascotaId = mascotaId
            };

            _context.Solicitudes.Add(solicitud);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Mascota");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearSolicitudTransito(int mascotaId)
        {
            var solicitud = new Solicitud
            {
                Tipo = TipoSolicitud.Transito,
                Estado = EstadoSolicitud.Pendiente,
                FechaCreacion = DateTime.Now,

                // cambiar por el usuario logueado después
                UsuarioId = 3,

                MascotaId = mascotaId
            };

            _context.Solicitudes.Add(solicitud);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Mascota");
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
            ViewData["AuditadoPorId"] = new SelectList(_context.Usuarios, "Id", "Nombre", solicitud.AuditadoPorId);
            ViewData["MascotaId"] = new SelectList(_context.Mascotas, "Id", "Descripcion", solicitud.MascotaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Nombre", solicitud.UsuarioId);
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
            ViewData["AuditadoPorId"] = new SelectList(_context.Usuarios, "Id", "Nombre", solicitud.AuditadoPorId);
            ViewData["MascotaId"] = new SelectList(_context.Mascotas, "Id", "Descripcion", solicitud.MascotaId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Nombre", solicitud.UsuarioId);
            return View(solicitud);
        }
        public async Task<IActionResult> Adopcion()
        {
            var mascotas = await _context.Mascotas
                .Where(m => m.EstaDisponible)
                .ToListAsync();

            return View("Index", mascotas);
        }

        public async Task<IActionResult> Transito()
        {
            var mascotas = await _context.Mascotas
                .Where(m => m.EstaDisponible)
                .ToListAsync();

            return View("Index", mascotas);
        }

        [HttpPost]
        public async Task<IActionResult> Verificar(int id)
        {
            var solicitud = await _context.Solicitudes
                .Include(s => s.Usuario)
                .Include(s => s.Mascota)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            bool aceptada = false;

            // limpiar detalle anterior
            solicitud.DetalleRechazo = null;

            // ==========================================
            // VALIDAR SOLICITUD DE ADOPCIÓN
            // ==========================================

            if (solicitud.Tipo == TipoSolicitud.Adopcion)
            {
                // CASA -> cualquier tamaño
                if (solicitud.Usuario.TipoVivienda == TipoVivienda.Casa)
                {
                    aceptada = true;
                }

                // DEPARTAMENTO -> chico o mediano
                else if (
                    solicitud.Usuario.TipoVivienda == TipoVivienda.Departamento
                    &&
                    (
                        solicitud.Mascota.Tamanio == Tamanio.Chico
                        ||
                        solicitud.Mascota.Tamanio == Tamanio.Mediano
                    )
                )
                {
                    aceptada = true;
                }
                else
                {
                    solicitud.DetalleRechazo =
                        "Las personas que viven en departamento solo pueden adoptar mascotas chicas o medianas.";
                }

                // SI SE ACEPTA
                if (aceptada)
                {
                    solicitud.Mascota.Adoptado = true;

                    solicitud.Mascota.UsuarioId = solicitud.UsuarioId;
                }
            }

            // ==========================================
            // VALIDAR SOLICITUD DE TRÁNSITO
            // ==========================================

            else if (solicitud.Tipo == TipoSolicitud.Transito)
            {
                bool yaTieneTransito = await _context.Mascotas
                    .AnyAsync(m =>
                        m.UsuarioId == solicitud.UsuarioId
                        && m.Transitado);

                // SOLO UN TRÁNSITO
                if (!yaTieneTransito)
                {
                    aceptada = true;

                    solicitud.Mascota.Transitado = true;

                    solicitud.Mascota.UsuarioId = solicitud.UsuarioId;
                }
                else
                {
                    solicitud.DetalleRechazo =
                        "El usuario ya tiene una mascota en tránsito.";
                }
            }

            // ==========================================
            // RESULTADO FINAL
            // ==========================================

            if (aceptada)
            {
                solicitud.Estado = EstadoSolicitud.Aceptada;

                solicitud.FechaAuditoria = DateTime.Now;

                // empleado que auditó
                solicitud.AuditadoPorId = 3;
            }
            else
            {
                solicitud.Estado = EstadoSolicitud.Rechazada;

                solicitud.FechaAuditoria = DateTime.Now;

                solicitud.AuditadoPorId = 3;

                // motivo rechazo
                if (solicitud.Tipo == TipoSolicitud.Adopcion)
                {
                    solicitud.DetalleRechazo =
                        "No cumple los requisitos de vivienda para el tamaño de la mascota.";
                }
                else
                {
                    solicitud.DetalleRechazo =
                        "El usuario ya posee una mascota en tránsito.";
                }

               
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
