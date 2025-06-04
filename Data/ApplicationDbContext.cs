using APISeasonalTicket.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace APISeasonalTicket.Data
{
    public class ApplicationDbContext : IdentityDbContext<User,IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Abono> Abonos { get; set; }
        public DbSet<MovimientosAbono> MovimientosAbonos { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }

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
        }
    }
}
