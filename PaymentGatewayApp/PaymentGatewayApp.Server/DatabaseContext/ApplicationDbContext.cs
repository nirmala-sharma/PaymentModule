using Microsoft.EntityFrameworkCore;
using PaymentGatewayApp.Server.Model;

namespace PaymentGatewayApp.Server.DatabaseContext
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshToken { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);

                entity.Property(u => u.UserName)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(u => u.PasswordHash)
               .IsRequired()
               .HasMaxLength(500);

                entity.Property(u => u.CreatedOn)
                    .IsRequired();
            });
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.TransactionId);

                entity.HasOne(u => u.User)
                    .WithMany(c => c.Transactions)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(t => t.FullName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(t => t.Email)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(t => t.Amount)
                    .IsRequired()
                    .HasColumnType("decimal(18,2)");

                entity.Property(t => t.Currency)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsFixedLength();

                entity.Property(t => t.PaymentMode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(t => t.Status)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(t => t.CreatedOn)
                    .IsRequired();
            });
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Token)
                    .IsRequired();

                entity
               .HasOne(rt => rt.User)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(rt => rt.UserId);

                entity.Property(u => u.UserId)
                    .IsRequired();
            });
            modelBuilder.Entity<IdempotencyKey>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.ResponseBody)
                   .IsRequired();
            });
        }

    }
}
