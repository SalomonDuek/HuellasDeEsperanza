using Microsoft.EntityFrameworkCore;
using HuellasDeEsperanza.Models;
using HuellasDeEsperanza.Data;

namespace HuellasDeEsperanza.Context
{
    public static class DbSeeder
    {

        public static async Task SeedAsync(HDEDbContext context)
        {
            await context.Database.MigrateAsync();

            var usuariosBase = new List<Usuario>
            {
                new Usuario
                {
                    Nombre = "Master",
                    Edad = 30,
                    Mail = "master@huellas.com",
                    Contrasenia = "Master123",
                    Rol = RolUsuario.Admin,
                    TipoVivienda = TipoVivienda.Casa,
                    CantidadDeMascotas = 0,
                    Direccion = "Sin dirección"
                },
                new Usuario
                {
                    Nombre = "Yako",
                    Edad = 25,
                    Mail = "yako@huellas.com",
                    Contrasenia = "Yako123",
                    Rol = RolUsuario.Empleado,
                    TipoVivienda = TipoVivienda.Casa,
                    CantidadDeMascotas = 0,
                    Direccion = "Sin dirección"
                },
                new Usuario
                {
                    Nombre = "Mica",
                    Edad = 25,
                    Mail = "mica@huellas.com",
                    Contrasenia = "Mica123",
                    Rol = RolUsuario.Empleado,
                    TipoVivienda = TipoVivienda.Casa,
                    CantidadDeMascotas = 0,
                    Direccion = "Sin dirección"
                },
                new Usuario
                {
                    Nombre = "Salo",
                    Edad = 25,
                    Mail = "salo@huellas.com",
                    Contrasenia = "Salo123",
                    Rol = RolUsuario.Empleado,
                    TipoVivienda = TipoVivienda.Casa,
                    CantidadDeMascotas = 0,
                    Direccion = "Sin dirección"
                }
            };

            foreach (var usuario in usuariosBase)
            {
                bool yaExiste = await context.Usuarios
                    .AnyAsync(u => u.Mail == usuario.Mail);

                if (!yaExiste)
                {
                    context.Usuarios.Add(usuario);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}