using Microsoft.EntityFrameworkCore;

namespace iBarber.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Barbearia> Barbearias { get; set; }
        public DbSet<Profissional> Profissionais { get; set; }
    }
}
