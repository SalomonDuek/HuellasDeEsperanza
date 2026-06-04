using Microsoft.EntityFrameworkCore;
using HuellasDeEsperanza.Models;
namespace HuellasDeEsperanza.Data

{

    public class HDEDbContext :DbContext
    {
        public HDEDbContext(DbContextOptions<HDEDbContext> options) : base(options)
        {
        }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

    }

}