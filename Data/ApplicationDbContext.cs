using APISeasonalMedic.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace APISeasonalMedic.Data
{
    public class ApplicationDbContext : IdentityDbContext<User,IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Abono> Abonos { get; set; }
        public DbSet<MovimientosAbono> MovimientosAbonos { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<ConsultaMedica> ConsultasMedicas { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(c => c.DNI)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasOne(u => u.Abono)
                .WithOne(a => a.User)
                .HasForeignKey<Abono>(a => a.UserId);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Cards)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Abono>()
                .HasMany(a => a.Movimientos)
                .WithOne(m => m.Abono)
                .HasForeignKey(m => m.AbonoId);

            // Aquí definís la precisión para propiedades decimal
            modelBuilder.Entity<Abono>(entity =>
            {
                entity.Property(e => e.MontoMensual).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<MovimientosAbono>(entity =>
            {
                entity.Property(e => e.Monto).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<UserSubscription>(entity =>
            {
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            });
            modelBuilder.Entity<User>()
                .HasMany(u => u.Consultas)
                .WithOne(c => c.Usuario)
                .HasForeignKey(c => c.UserId);
        }

    }
}
