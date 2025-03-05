using GustaffWeb.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GustaffWeb.Data
{
    public class Context : IdentityDbContext
    {
        public Context(DbContextOptions<Context>options) : base(options) { }

        public DbSet<NotaModel> Notas { get; set; }

        public DbSet<DespesasModel> Despesas { get; set; }
    }
}
