using Microsoft.EntityFrameworkCore;
using PetCare360.Domain.Entities;

namespace PetCare360.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Tutor> Tutores { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Clinica> Clinicas { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<Vacina> Vacinas { get; set; }
        public DbSet<Medicamento> Medicamentos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tutor>().HasIndex(t => t.Cpf).IsUnique();
            modelBuilder.Entity<Tutor>().HasIndex(t => t.Email).IsUnique();
            modelBuilder.Entity<Clinica>().HasIndex(c => c.Cnpj).IsUnique();
            modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Pet>().Property(p => p.Peso).HasPrecision(6, 2);

            modelBuilder.Entity<Pet>().HasOne(p => p.Tutor).WithMany(t => t.Pets).HasForeignKey(p => p.IdTutor).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Consulta>().HasOne(c => c.Clinica).WithMany(cl => cl.Consultas).HasForeignKey(c => c.IdClinica).OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}