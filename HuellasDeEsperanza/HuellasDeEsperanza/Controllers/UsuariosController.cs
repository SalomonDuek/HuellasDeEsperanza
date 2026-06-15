using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellasDeEsperanza.Data;
using HuellasDeEsperanza.Models;

namespace HuellasDeEsperanza.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly HDEDbContext _context;
        private readonly IWebHostEnvironment _env;

        // Carpeta donde se guardan las imágenes
        private const string CARPETA_FOTOS = "images/usuarios";

        public UsuariosController(HDEDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =========================
        // GET: Usuarios
        // =========================
        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuarios.ToListAsync());
        }

        // =========================
        // GET: Usuarios/Details/5
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios
                .Include(u => u.Mascotas)
                .Include(u => u.Solicitudes)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // =========================
        // GET: Usuarios/Create
        // =========================
        public IActionResult Create()
        {
            return View();
        }

        // =========================
        // POST: Usuarios/Create
        // =========================
        [HttpPost]
      
        public async Task<IActionResult> Create(
            [Bind("Id,Nombre,Edad,Mail,Contrasenia,Rol,TipoVivienda,CantidadDeMascotas,Direccion")]
            Usuario usuario,
            IFormFile? imagenArchivo)
        {
            // Validaciones adicionales
            if (usuario.Edad < 18)
            {
                ModelState.AddModelError("Edad", "Debe ser mayor de edad");
            }

            if (usuario.CantidadDeMascotas < 0)
            {
                ModelState.AddModelError("CantidadDeMascotas", "La cantidad no puede ser negativa");
            }

            if (ModelState.IsValid)
            {
                // Guardado de imagen
                if (imagenArchivo != null && imagenArchivo.Length > 0)
                {
                    usuario.Imagen = await GuardarImagenAsync(imagenArchivo);
                }
                else
                {
                    usuario.Imagen = string.Empty;
                }

                _context.Add(usuario);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        // =========================
        // GET: Usuarios/Edit/5
        // =========================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // =========================
        // POST: Usuarios/Edit/5
        // =========================
        [HttpPost]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Nombre,Edad,Mail,Contrasenia,Rol,TipoVivienda,CantidadDeMascotas,Direccion")]
            Usuario usuario,
            IFormFile? imagenArchivo)
        {
            if (id != usuario.Id)
                return NotFound();

            if (usuario.Edad < 18)
            {
                ModelState.AddModelError("Edad", "Debe ser mayor de edad");
            }

            
            if (ModelState.IsValid)
            {
                try
                {
                    // Recuperamos el usuario desde DB
                    var usuarioDb = await _context.Usuarios.FindAsync(id);

                    if (usuarioDb == null)
                        return NotFound();

                    // Actualizamos campos permitidos
                    usuarioDb.Nombre = usuario.Nombre;
                    usuarioDb.Edad = usuario.Edad;
                    usuarioDb.Mail = usuario.Mail;
                    usuarioDb.Contrasenia = usuario.Contrasenia;
                    usuarioDb.Rol = usuario.Rol;
                    usuarioDb.TipoVivienda = usuario.TipoVivienda;
                    usuarioDb.CantidadDeMascotas = usuario.CantidadDeMascotas;
                    usuarioDb.Direccion = usuario.Direccion;

                    // Nueva imagen
                    if (imagenArchivo != null && imagenArchivo.Length > 0)
                    {
                        EliminarImagenExistente(usuarioDb.Imagen);

                        usuarioDb.Imagen = await GuardarImagenAsync(imagenArchivo);
                    }

                    _context.Update(usuarioDb);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(id))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        // =========================
        // GET: Usuarios/Delete/5
        // =========================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // =========================
        // POST: Usuarios/Delete/5
        // =========================
        [HttpPost, ActionName("Delete")]
       
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario != null)
            {
                // Eliminar imagen
                EliminarImagenExistente(usuario.Imagen);

                _context.Usuarios.Remove(usuario);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // MÉTODOS AUXILIARES PARA IMÁGENES
        // ==========================================

        private async Task<string> GuardarImagenAsync(IFormFile archivo)
        {
            var carpeta = Path.Combine(_env.WebRootPath, CARPETA_FOTOS);

            // Crear carpeta si no existe
            if (!Directory.Exists(carpeta))
            {
                Directory.CreateDirectory(carpeta);
            }

            var extension = Path.GetExtension(archivo.FileName);

            // Nombre único
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";

            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using var stream = new FileStream(rutaCompleta, FileMode.Create);

            await archivo.CopyToAsync(stream);

            return nombreArchivo;
        }

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

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}