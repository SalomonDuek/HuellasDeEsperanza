using HuellasDeEsperanza.Models;
using Microsoft.AspNetCore.Http;

namespace HuellasDeEsperanza.Helpers
{
    // Extensiones para leer fácil los datos de sesión desde cualquier Controller.
    // Uso: HttpContext.Session.GetUsuarioId()  /  HttpContext.Session.EstaLogueado()
    public static class SesionExtensions
    {
        public static int? GetUsuarioId(this ISession session)
        {
            return session.GetInt32("UsuarioId");
        }

        public static string? GetUsuarioNombre(this ISession session)
        {
            return session.GetString("UsuarioNombre");
        }

        public static RolUsuario? GetUsuarioRol(this ISession session)
        {
            var rolTexto = session.GetString("UsuarioRol");

            if (rolTexto == null)
                return null;

            return Enum.Parse<RolUsuario>(rolTexto);
        }

        public static bool EstaLogueado(this ISession session)
        {
            return session.GetUsuarioId() != null;
        }

        public static bool EsEmpleadoOAdmin(this ISession session)
        {
            var rol = session.GetUsuarioRol();
            return rol == RolUsuario.Empleado || rol == RolUsuario.Admin;
        }
    }
}