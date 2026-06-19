using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellasDeEsperanza.Data;
using HuellasDeEsperanza.Models;

namespace HuellasDeEsperanza.Controllers
{
    public class MascotaController : Controller
    {
        private readonly HDEDbContext _context;
        private readonly IWebHostEnvironment _env;
        // Definimos la constante para que no haya errores de tipeo en las rutas
        private const string CARPETA_FOTOS = "images/mascotas";

        public MascotaController(HDEDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Mascotas
        public async Task<IActionResult> Index()
        {
            return View(await _context.Mascotas.Include(m => m.Usuario).ToListAsync());
        }

        // GET: Mascotas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var mascota = await _context.Mascotas
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (mascota == null) return NotFound();
            return View(mascota);
        }

        // GET: Mascotas/Create
        public IActionResult Create() => View();

        // POST: Mascotas/Create
        [HttpPost]
        public async Task<IActionResult> Create(
            [Bind("Id,Nombre,Descripcion,TipoMascota,Tamanio,Estado,EstaVacunada,EstaCastrada,EstaDesparasitada,EstaDisponible,UsuarioId")]
            Mascota mascota,
            IFormFile? imagenArchivo)
        {
            mascota.FechaIngreso = DateTime.Now;

            if (mascota.TipoMascota != TipoMascota.Perro && mascota.TipoMascota != TipoMascota.Gato)
            {
                ModelState.AddModelError("TipoMascota", "Solo se aceptan perros y gatos en el refugio");
            }

            if (mascota.Estado == EstadoMedico.AmbulatorioConBaja && mascota.EstaDisponible)
            {
                ModelState.AddModelError("EstaDisponible", "Una mascota con Ambulatorio con Baja no puede estar disponible para adopción/tránsito");
                mascota.EstaDisponible = false;
            }

            if (ModelState.IsValid)
            {
                // LÓGICA DE ALMACENAMIENTO EXCLUSIVO
                if (imagenArchivo != null && imagenArchivo.Length > 0)
                {
                    mascota.Imagen = await GuardarImagenAsync(imagenArchivo);
                }
                else
                {
                    mascota.Imagen = string.Empty; // O una imagen por defecto si lo prefiero
                }

                _context.Add(mascota);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mascota);
        }

        // GET: Mascotas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota == null) return NotFound();
            return View(mascota);
        }

        // POST: Mascotas/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Nombre,Descripcion,TipoMascota,Tamanio,Estado,EstaVacunada,EstaCastrada,EstaDesparasitada,EstaDisponible,UsuarioId")]
            Mascota mascota,
            IFormFile? imagenArchivo)
        {
            if (id != mascota.Id) return NotFound();

            if (mascota.Estado == EstadoMedico.AmbulatorioConBaja && mascota.EstaDisponible)
            {
                ModelState.AddModelError("EstaDisponible", "Una mascota con Ambulatorio con Baja no puede estar disponible");
                mascota.EstaDisponible = false;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Recuperamos la entidad desde DB para evitar problemas de binding (ej: FechaIngreso)
                    var mascotaDb = await _context.Mascotas.FindAsync(id);
                    if (mascotaDb == null) return NotFound();

                    // Actualizamos solo los campos permitidos
                    mascotaDb.Nombre = mascota.Nombre;
                    mascotaDb.Descripcion = mascota.Descripcion;
                    mascotaDb.TipoMascota = mascota.TipoMascota;
                    mascotaDb.Tamanio = mascota.Tamanio;
                    mascotaDb.Estado = mascota.Estado;
                    mascotaDb.EstaVacunada = mascota.EstaVacunada;
                    mascotaDb.EstaCastrada = mascota.EstaCastrada;
                    mascotaDb.EstaDesparasitada = mascota.EstaDesparasitada;
                    mascotaDb.EstaDisponible = mascota.EstaDisponible;
                    mascotaDb.UsuarioId = mascota.UsuarioId;

                    if (imagenArchivo != null && imagenArchivo.Length > 0)
                    {
                        // Borramos la foto anterior y guardamos la nueva
                        EliminarImagenExistente(mascotaDb.Imagen);
                        mascotaDb.Imagen = await GuardarImagenAsync(imagenArchivo);
                    }

                    _context.Update(mascotaDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MascotaExists(id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            // Si hay errores de validación, recargamos la vista con el modelo enviado (no guardado)
            return View(mascota);
        }

        // GET: Mascotas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var mascota = await _context.Mascotas.FirstOrDefaultAsync(m => m.Id == id);
            if (mascota == null) return NotFound();
            return View(mascota);
        }

        // POST: Mascotas/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mascota = await _context.Mascotas.FindAsync(id);
            if (mascota != null)
            {
                // Al eliminar la mascota, también limpiamos su foto del servidor
                EliminarImagenExistente(mascota.Imagen);
                _context.Mascotas.Remove(mascota);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // MÉTODOS DE INFRAESTRUCTURA PARA IMÁGENES
        // ==========================================

        private async Task<string> GuardarImagenAsync(IFormFile archivo)
        {
            // Asegura la ruta absoluta hacia wwwroot/images/mascotas
            var carpeta = Path.Combine(_env.WebRootPath, CARPETA_FOTOS);

            // Si la carpeta no existe, la crea automáticamente
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            var extension = Path.GetExtension(archivo.FileName);
            // Evitamos colisiones de nombres usando GUIDs
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using var stream = new FileStream(rutaCompleta, FileMode.Create);
            await archivo.CopyToAsync(stream);

            return nombreArchivo;
        }

        private void EliminarImagenExistente(string? nombreImagen)
        {
            if (string.IsNullOrEmpty(nombreImagen)) return;

            var rutaArchivo = Path.Combine(_env.WebRootPath, CARPETA_FOTOS, nombreImagen);
            if (System.IO.File.Exists(rutaArchivo))
            {
                System.IO.File.Delete(rutaArchivo);
            }
        }

        private bool MascotaExists(int id)
        {
            return _context.Mascotas.Any(e => e.Id == id);
        }
    }
}