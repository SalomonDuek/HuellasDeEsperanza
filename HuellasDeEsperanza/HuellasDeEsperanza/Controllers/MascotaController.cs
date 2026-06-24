using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellasDeEsperanza.Data;
using HuellasDeEsperanza.Models;
using HuellasDeEsperanza.Helpers;

namespace HuellasDeEsperanza.Controllers
{
    public class MascotaController : Controller
    {
        private readonly HDEDbContext _context;
        private readonly IWebHostEnvironment _env;

        private const string CARPETA_FOTOS = "images/mascotas";

        public MascotaController(HDEDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // INDEX GENERAL
        public async Task<IActionResult> Index()
        {
            return View(await _context.Mascotas
                .Include(m => m.Usuario)
                .Where(m => !m.Adoptado && !m.Transitado)
                .ToListAsync());
        }

        // MASCOTAS PARA ADOPCIÓN
        public async Task<IActionResult> Adopcion()
        {
            ViewBag.TipoVista = "Adopcion";

            var mascotas = await _context.Mascotas
                .Where(m =>
                    m.Disponibilidad == Disponibilidad.Adoptable
                    && !m.Adoptado)
                .ToListAsync();

            return View("Index", mascotas);
        }

        // MASCOTAS PARA TRÁNSITO
        public async Task<IActionResult> Transito()
        {
            ViewBag.TipoVista = "Transito";

            var mascotas = await _context.Mascotas
                .Where(m =>
                    m.Disponibilidad == Disponibilidad.Transitable
                    && !m.Transitado)
                .ToListAsync();

            return View("Index", mascotas);
        }

        // DETAILS
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var mascota = await _context.Mascotas
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mascota == null)
                return NotFound();

            return View(mascota);
        }

        // CREATE GET

        public IActionResult Create()
        {
            if (!HttpContext.Session.EsEmpleadoOAdmin())
                return RedirectToAction("Login", "Usuarios");

            return View();
        }

        // CREATE POST

        [HttpPost]
        public async Task<IActionResult> Create(
            [Bind("Id,Nombre,Descripcion,TipoMascota,Tamanio,EstaVacunada,EstaCastrada,EstaDesparasitada,UsuarioId")]
            Mascota mascota,
            IFormFile? imagenArchivo)
        {
            if (!HttpContext.Session.EsEmpleadoOAdmin())
                return RedirectToAction("Login", "Usuarios");

            mascota.FechaIngreso = DateTime.Now;

            // CALCULAR ESTADO MÉDICO
            if (mascota.EstaVacunada &&
                mascota.EstaCastrada &&
                mascota.EstaDesparasitada)
            {
                mascota.Estado = EstadoMedico.AltaMedica;
                mascota.Disponibilidad = Disponibilidad.Adoptable;
            }
            else
            {
                mascota.Estado = EstadoMedico.EnTratamiento;
                mascota.Disponibilidad = Disponibilidad.Transitable;
            }

            mascota.EstaDisponible = true;

            if (ModelState.IsValid)
            {
                // GUARDAR IMAGEN

                if (imagenArchivo != null && imagenArchivo.Length > 0)
                {
                    mascota.Imagen = await GuardarImagenAsync(imagenArchivo);
                }
                else
                {
                    mascota.Imagen = string.Empty;
                }

                _context.Add(mascota);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View("Index");
        }

        // EDIT GET

        public async Task<IActionResult> Edit(int? id)
        {
            if (!HttpContext.Session.EsEmpleadoOAdmin())
                return RedirectToAction("Login", "Usuarios");

            if (id == null)
                return NotFound();

            var mascota = await _context.Mascotas.FindAsync(id);

            if (mascota == null)
                return NotFound();

            return View(mascota);
        }

        // EDIT POST
        [HttpPost]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Nombre,Descripcion,TipoMascota,Tamanio,EstaVacunada,EstaCastrada,EstaDesparasitada,UsuarioId,Adoptado,Transitado")]
            Mascota mascota,
            IFormFile? imagenArchivo)
        {
            if (!HttpContext.Session.EsEmpleadoOAdmin())
                return RedirectToAction("Login", "Usuarios");

            if (id != mascota.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var mascotaDb = await _context.Mascotas.FindAsync(id);

                    if (mascotaDb == null)
                        return NotFound();

                    mascotaDb.Nombre = mascota.Nombre;
                    mascotaDb.Descripcion = mascota.Descripcion;
                    mascotaDb.TipoMascota = mascota.TipoMascota;
                    mascotaDb.Tamanio = mascota.Tamanio;

                    mascotaDb.EstaVacunada = mascota.EstaVacunada;
                    mascotaDb.EstaCastrada = mascota.EstaCastrada;
                    mascotaDb.EstaDesparasitada = mascota.EstaDesparasitada;

                    mascotaDb.UsuarioId = mascota.UsuarioId;

                    mascotaDb.Adoptado = mascota.Adoptado;
                    mascotaDb.Transitado = mascota.Transitado;

                    // RECALCULAR ESTADO Y DISPONIBILIDAD

                    if (mascotaDb.EstaVacunada &&
                        mascotaDb.EstaCastrada &&
                        mascotaDb.EstaDesparasitada)
                    {
                        mascotaDb.Estado = EstadoMedico.AltaMedica;
                        mascotaDb.Disponibilidad = Disponibilidad.Adoptable;
                    }
                    else
                    {
                        mascotaDb.Estado = EstadoMedico.EnTratamiento;
                        mascotaDb.Disponibilidad = Disponibilidad.Transitable;
                    }

                    if (imagenArchivo != null && imagenArchivo.Length > 0)
                    {
                        EliminarImagenExistente(mascotaDb.Imagen);

                        mascotaDb.Imagen = await GuardarImagenAsync(imagenArchivo);
                    }

                    _context.Update(mascotaDb);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MascotaExists(id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(mascota);
        }

        // DELETE GET
        public async Task<IActionResult> Delete(int? id)
        {
            if (!HttpContext.Session.EsEmpleadoOAdmin())
                return RedirectToAction("Login", "Usuarios");

            if (id == null)
                return NotFound();

            var mascota = await _context.Mascotas
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mascota == null)
                return NotFound();

            return View(mascota);
        }

        // DELETE POST

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!HttpContext.Session.EsEmpleadoOAdmin())
                return RedirectToAction("Login", "Usuarios");

            var mascota = await _context.Mascotas.FindAsync(id);

            if (mascota != null)
            {
                var solicitudes = _context.Solicitudes
                    .Where(s => s.MascotaId == mascota.Id);

                _context.Solicitudes.RemoveRange(solicitudes);

                EliminarImagenExistente(mascota.Imagen);

                _context.Mascotas.Remove(mascota);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GUARDAR IMAGEN
        private async Task<string> GuardarImagenAsync(IFormFile archivo)
        {
            var carpeta = Path.Combine(_env.WebRootPath, CARPETA_FOTOS);

            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            var extension = Path.GetExtension(archivo.FileName);

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";

            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using var stream = new FileStream(rutaCompleta, FileMode.Create);

            await archivo.CopyToAsync(stream);

            return nombreArchivo;
        }

        // ELIMINAR IMAGEN
        private void EliminarImagenExistente(string? nombreImagen)
        {
            if (string.IsNullOrEmpty(nombreImagen))
                return;

            var rutaArchivo = Path.Combine(
                _env.WebRootPath,
                CARPETA_FOTOS,
                nombreImagen);

            if (System.IO.File.Exists(rutaArchivo))
            {
                System.IO.File.Delete(rutaArchivo);
            }
        }

        // EXISTS

        private bool MascotaExists(int id)
        {
            return _context.Mascotas.Any(e => e.Id == id);
        }
    }
}