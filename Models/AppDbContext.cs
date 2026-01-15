using Microsoft.EntityFrameworkCore;

namespace iBarber.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Barbearia> Barbearias { get; set; }
        public DbSet<Profissional> Profissionais { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<Agendamento> Agendamentos { get; set; }

        // ADICIONE ESTE MÉTODO ABAIXO
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configura as chaves estrangeiras do Agendamento
            // para não usar "DELETE CASCADE"

            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Profissional)
                .WithMany() // Se Profissional não tiver uma lista de Agendamentos
                .HasForeignKey(a => a.ProfissionalId)
                .OnDelete(DeleteBehavior.NoAction); // << MUDANÇA AQUI

            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Servico)
                .WithMany() // Se Servico não tiver uma lista de Agendamentos
                .HasForeignKey(a => a.ServicoId)
                .OnDelete(DeleteBehavior.NoAction); // << MUDANÇA AQUI

            modelBuilder.Entity<Agendamento>()
                .HasOne(a => a.Usuario)
                .WithMany() // Se Usuario não tiver uma lista de Agendamentos
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.NoAction); // << MUDANÇA AQUI
        }
    }
}