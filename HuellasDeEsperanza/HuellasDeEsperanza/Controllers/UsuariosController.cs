using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellasDeEsperanza.Data;
using HuellasDeEsperanza.Models;
using HuellasDeEsperanza.ViewModels;
using HuellasDeEsperanza.Helpers;

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
        // GET: Usuarios/Register
        // =========================
        public IActionResult Register()
        {
            // Si ya está logueado, no tiene sentido que se registre de nuevo
            if (HttpContext.Session.GetInt32("UsuarioId") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // =========================
        // POST: Usuarios/Register
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel modelo)
        {
            // El mail no puede estar repetido
            bool mailExiste = await _context.Usuarios
                .AnyAsync(u => u.Mail == modelo.Mail);

            if (mailExiste)
            {
                ModelState.AddModelError("Mail", "Ya existe una cuenta con ese mail");
            }

            if (!ModelState.IsValid)
                return View(modelo);

            // Por seguridad, todo el que se registra solo entra como Adoptante.
            // Empleado/Admin se asignan a mano desde el panel.
            var usuario = new Usuario
            {
                Nombre = modelo.Nombre,
                Edad = modelo.Edad,
                Mail = modelo.Mail,
                Contrasenia = modelo.Contrasenia,
                Rol = RolUsuario.Adoptante,
                TipoVivienda = modelo.TipoVivienda,
                CantidadDeMascotas = modelo.CantidadDeMascotas,
                Direccion = modelo.Direccion
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Lo logueamos directo después de registrarse
            IniciarSesion(usuario);

            return RedirectToAction("Index", "Home");
        }

        // =========================
        // GET: Usuarios/Login
        // =========================
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UsuarioId") != null)
                return RedirectToAction("Index", "Home");

            return View();
        }

        // =========================
        // POST: Usuarios/Login
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Mail == modelo.Mail);

            // OJO: comparación en texto plano. Sirve para el TP, pero no es
            // así como se haría en un sistema real (ahí se usa hash, ej. BCrypt).
            if (usuario == null || usuario.Contrasenia != modelo.Contrasenia)
            {
                ModelState.AddModelError(string.Empty, "Mail o contraseña incorrectos");
                return View(modelo);
            }

            IniciarSesion(usuario);

            return RedirectToAction("Index", "Home");
        }

        // =========================
        // POST: Usuarios/Logout
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // =========================
        // Helper privado: guarda los datos clave en sesión
        // =========================
        private void IniciarSesion(Usuario usuario)
        {
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol.ToString());
        }

        // =========================
        // GET: Usuarios
        // =========================
        public async Task<IActionResult> Index()
        {
            if (!HttpContext.Session.EsEmpleadoOAdmin())
                return RedirectToAction("Login", "Usuarios");

            return View(await _context.Usuarios.ToListAsync());
        }

        // =========================
        // GET: Usuarios/Details/5
        // =========================
        public async Task<IActionResult> Details(int? id)
        {
            if (!HttpContext.Session.EstaLogueado())
                return RedirectToAction("Login", "Usuarios");

            if (id == null) return NotFound();

            // Un Adoptante solo puede ver su propio perfil, no el de otros
            if (!HttpContext.Session.EsEmpleadoOAdmin() &&
                HttpContext.Session.GetUsuarioId() != id)
            {
                return Forbid();
            }

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
            if (!HttpContext.Session.EsEmpleadoOAdmin())
                return RedirectToAction("Login", "Usuarios");

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
            if (!HttpContext.Session.EsEmpleadoOAdmin())
                return RedirectToAction("Login", "Usuarios");

            // Un Empleado no puede crear otro Empleado ni un Admin
            if (HttpContext.Session.GetUsuarioRol() == RolUsuario.Empleado &&
                usuario.Rol != RolUsuario.Adoptante)
            {
                ModelState.AddModelError("Rol", "No tenés permiso para crear usuarios con ese rol");
            }

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
            if (!HttpContext.Session.EstaLogueado())
                return RedirectToAction("Login", "Usuarios");

            if (id == null)
                return NotFound();

            // Un Adoptante solo puede editar su propio perfil
            if (!HttpContext.Session.EsEmpleadoOAdmin() &&
                HttpContext.Session.GetUsuarioId() != id)
            {
                return Forbid();
            }

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
            if (!HttpContext.Session.EstaLogueado())
                return RedirectToAction("Login", "Usuarios");

            if (id != usuario.Id)
                return NotFound();

            bool esEmpleadoOAdmin = HttpContext.Session.EsEmpleadoOAdmin();

            // Un Adoptante solo puede editar su propio perfil, y no puede cambiarse el Rol
            if (!esEmpleadoOAdmin)
            {
                if (HttpContext.Session.GetUsuarioId() != id)
                    return Forbid();

                usuario.Rol = RolUsuario.Adoptante;
            }

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
            if (!HttpContext.Session.EsEmpleadoOAdmin())
                return RedirectToAction("Login", "Usuarios");

            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            // Un Empleado no puede eliminar a otro Empleado (solo Admin podría)
            if (HttpContext.Session.GetUsuarioRol() == RolUsuario.Empleado &&
                usuario.Rol != RolUsuario.Adoptante)
            {
                return Forbid();
            }

            return View(usuario);
        }

        // =========================
        // POST: Usuarios/Delete/5
        // =========================
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!HttpContext.Session.EsEmpleadoOAdmin())
                return RedirectToAction("Login", "Usuarios");

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario != null)
            {
                // Un Empleado no puede eliminar a otro Empleado
                if (HttpContext.Session.GetUsuarioRol() == RolUsuario.Empleado &&
                    usuario.Rol != RolUsuario.Adoptante)
                {
                    return Forbid();
                }

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