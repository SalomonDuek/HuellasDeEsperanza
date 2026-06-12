using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HuellasDeEsperanza.Data;
using HuellasDeEsperanza.Models;
using HuellasDeEsperanza.ViewModels;

namespace HuellasDeEsperanza.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly HDEDbContext _context;

        public UsuariosController(HDEDbContext context)
        {
            _context = context;
        }

        // ─── AUTH ─────────────────────────────────────────────────────────────

        // GET: Usuarios/Login
        public IActionResult Login() => View();

        // POST: Usuarios/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Mail == model.Mail
                                       && u.Contrasenia == model.Contrasenia);

            if (usuario == null)
            {
                ModelState.AddModelError("", "Mail o contraseña incorrectos");
                return View(model);
            }

            // Guardar datos en Session
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol.ToString());

            return RedirectToAction("Index", "Home");
        }

        // GET: Usuarios/Register
        public IActionResult Register() => View();

        // POST: Usuarios/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Solo se puede registrar como Tránsito o Adoptante
            if (model.Rol != RolUsuario.Transito && model.Rol != RolUsuario.Adoptante)
            {
                ModelState.AddModelError("Rol", "Solo podés registrarte como Tránsito o Adoptante");
                return View(model);
            }

            var existe = await _context.Usuarios.AnyAsync(u => u.Mail == model.Mail);
            if (existe)
            {
                ModelState.AddModelError("Mail", "Ya existe una cuenta con ese mail");
                return View(model);
            }

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Edad = model.Edad,
                Mail = model.Mail,
                Contrasenia = model.Contrasenia,
                Rol = model.Rol,
                TipoVivienda = model.TipoVivienda,
                CantidadDeMascotas = model.CantidadDeMascotas,
                Direccion = model.Direccion
            };

            _context.Add(usuario);
            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre);
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol.ToString());

            return RedirectToAction("Index", "Home");
        }

        // POST: Usuarios/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ─── LISTADO (Admin y Empleado) ───────────────────────────────────────

        // GET: Usuarios/Index
        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Admin" && rol != "Empleado")
                return RedirectToAction("Login");

            return View(await _context.Usuarios.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Admin" && rol != "Empleado")
                return RedirectToAction("Login");

            if (id == null) return NotFound();
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // ─── PERFIL PROPIO ────────────────────────────────────────────────────

        // GET: Usuarios/MiPerfil
        public async Task<IActionResult> MiPerfil()
        {
            var id = HttpContext.Session.GetInt32("UsuarioId");
            if (id == null) return RedirectToAction("Login");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var idSesion = HttpContext.Session.GetInt32("UsuarioId");
            var rol = HttpContext.Session.GetString("UsuarioRol");

            // Solo puede editar su propio perfil, salvo Admin/Empleado
            if (idSesion != id && rol != "Admin" && rol != "Empleado")
                return RedirectToAction("Login");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Nombre,Edad,Mail,Contrasenia,TipoVivienda,CantidadDeMascotas,Direccion,Rol")]
            Usuario usuario)
        {
            if (id != usuario.Id) return NotFound();

            var idSesion = HttpContext.Session.GetInt32("UsuarioId");
            var rol = HttpContext.Session.GetString("UsuarioRol");

            if (idSesion != id && rol != "Admin" && rol != "Empleado")
                return RedirectToAction("Login");

            // Empleado solo puede editar Tránsito o Adoptante
            if (rol == "Empleado" &&
                usuario.Rol != RolUsuario.Transito &&
                usuario.Rol != RolUsuario.Adoptante)
            {
                ModelState.AddModelError("", "Un empleado solo puede editar usuarios Tránsito o Adoptante");
                return View(usuario);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Usuarios.Any(u => u.Id == id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // ─── ASIGNAR ROL (solo Admin) ─────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarRol(int id, RolUsuario nuevoRol)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Admin") return RedirectToAction("Login");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.Rol = nuevoRol;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ─── ELIMINAR ─────────────────────────────────────────────────────────

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Admin" && rol != "Empleado")
                return RedirectToAction("Login");

            if (id == null) return NotFound();
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rol = HttpContext.Session.GetString("UsuarioRol");
            if (rol != "Admin" && rol != "Empleado")
                return RedirectToAction("Login");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            // Empleado solo puede eliminar Tránsito o Adoptante
            if (rol == "Empleado" &&
                usuario.Rol != RolUsuario.Transito &&
                usuario.Rol != RolUsuario.Adoptante)
            {
                ModelState.AddModelError("", "Un empleado solo puede eliminar Tránsito o Adoptante");
                return View(usuario);
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}