using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
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

        // ─── HELPERS DE SESIÓN ────────────────────────────────────────────────

        private void GuardarSesion(Usuario usuario)
        {
            var datos = new
            {
                usuario.Id,
                usuario.Nombre,
                usuario.Mail,
                Rol = usuario.Rol.ToString()
            };
            HttpContext.Session.SetString("UsuarioSesion", JsonSerializer.Serialize(datos));
        }

        private string? ObtenerRol()
        {
            var json = HttpContext.Session.GetString("UsuarioSesion");
            if (json == null) return null;
            return JsonSerializer.Deserialize<JsonElement>(json)
                                 .GetProperty("Rol").GetString();
        }

        private int? ObtenerIdSesion()
        {
            var json = HttpContext.Session.GetString("UsuarioSesion");
            if (json == null) return null;
            return JsonSerializer.Deserialize<JsonElement>(json)
                                 .GetProperty("Id").GetInt32();
        }

        private bool EstaLogueado() =>
            HttpContext.Session.GetString("UsuarioSesion") != null;

        // ─── REGISTER ────────────────────────────────────────────────────────

        // GET: Usuarios/Register
        public IActionResult Register()
        {
            if (EstaLogueado()) return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: Usuarios/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Verificar mail único
            if (await _context.Usuarios.AnyAsync(u => u.Mail == model.Mail))
                ModelState.AddModelError("Mail", "Ya existe una cuenta con ese mail");

            if (!ModelState.IsValid) return View(model);

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Edad = model.Edad,
                Mail = model.Mail,
                Contrasenia = model.Contrasenia,
                Rol = RolUsuario.Adoptante, // siempre Adoptante al registrarse
                TipoVivienda = model.TipoVivienda,
                CantidadDeMascotas = model.CantidadDeMascotas,
                Direccion = model.Direccion
            };

            _context.Add(usuario);
            await _context.SaveChangesAsync();

            GuardarSesion(usuario);
            return RedirectToAction("Index", "Home");
        }

        // ─── LOGIN ────────────────────────────────────────────────────────────

        // GET: Usuarios/Login
        public IActionResult Login()
        {
            if (EstaLogueado()) return RedirectToAction("Index", "Home");
            return View();
        }

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

            GuardarSesion(usuario);
            return RedirectToAction("Index", "Home");
        }

        // POST: Usuarios/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("UsuarioSesion");
            return RedirectToAction("Login");
        }

        // ─── CRUD (Admin y Empleado) ──────────────────────────────────────────

        // GET: Usuarios/Index
        public async Task<IActionResult> Index()
        {
            var rol = ObtenerRol();
            if (rol != "Admin" && rol != "Empleado")
                return RedirectToAction("Login");

            return View(await _context.Usuarios.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            var rol = ObtenerRol();
            if (rol != "Admin" && rol != "Empleado")
                return RedirectToAction("Login");

            if (id == null) return NotFound();
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // GET: Usuarios/Create — solo Admin crea Empleados
        public IActionResult Create()
        {
            if (ObtenerRol() != "Admin") return RedirectToAction("Login");
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Nombre,Edad,Mail,Contrasenia,Rol,TipoVivienda,CantidadDeMascotas,Direccion")]
            Usuario usuario)
        {
            if (ObtenerRol() != "Admin") return RedirectToAction("Login");

            if (ModelState.IsValid)
            {
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var rol = ObtenerRol();
            if (rol != "Admin" && rol != "Empleado")
                return RedirectToAction("Login");

            if (id == null) return NotFound();
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Nombre,Edad,Mail,Contrasenia,Rol,TipoVivienda,CantidadDeMascotas,Direccion")]
            Usuario usuario)
        {
            if (id != usuario.Id) return NotFound();

            var rol = ObtenerRol();
            if (rol != "Admin" && rol != "Empleado")
                return RedirectToAction("Login");

            // Empleado solo puede editar Adoptantes
            if (rol == "Empleado" && usuario.Rol != RolUsuario.Adoptante)
            {
                ModelState.AddModelError("", "Un empleado solo puede editar Adoptantes");
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

        // GET: Usuarios/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var rol = ObtenerRol();
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
            var rol = ObtenerRol();
            if (rol != "Admin" && rol != "Empleado")
                return RedirectToAction("Login");

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            // Empleado solo puede eliminar Adoptantes
            if (rol == "Empleado" && usuario.Rol != RolUsuario.Adoptante)
            {
                ModelState.AddModelError("", "Un empleado solo puede eliminar Adoptantes");
                return View(usuario);
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}