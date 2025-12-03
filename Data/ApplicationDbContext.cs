using Microsoft.EntityFrameworkCore;
using quotation_generator_back_end.Models;

namespace quotation_generator_back_end.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientContact> ClientContacts { get; set; }
        public DbSet<Models.User> Users { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationItem> QuotationItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Item entity
            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Client entity
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClientName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ClientEmail).IsRequired().HasMaxLength(200);
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Client-ClientContact relationship
            modelBuilder.Entity<Client>()
                .HasMany(c => c.Contacts)
                .WithOne(cc => cc.Client)
                .HasForeignKey(cc => cc.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure User entity
            modelBuilder.Entity<Models.User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).HasMaxLength(200);
                entity.Property(e => e.LastName).HasMaxLength(200);
                entity.Property(e => e.Email).HasMaxLength(200);
                entity.Property(e => e.Language).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(1000);
                entity.Property(e => e.IdNumber).HasMaxLength(100);
                entity.Property(e => e.PasswordHash).HasMaxLength(200);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure Quotation entity
            modelBuilder.Entity<Quotation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.QuoteNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PoNumber).HasMaxLength(100);
                entity.Property(e => e.ClientName).HasMaxLength(200);
                entity.Property(e => e.DiscountType).HasMaxLength(20);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Project).HasMaxLength(200);
                entity.Property(e => e.AssignedUser).HasMaxLength(200);
                entity.Property(e => e.Vendor).HasMaxLength(200);
                entity.Property(e => e.Design).HasMaxLength(200);
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DiscountAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
                entity.Property(e => e.NetAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PartialDeposit).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Discount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ExchangeRate).HasColumnType("decimal(18,6)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

                // Relationship with Client (optional)
                entity.HasOne(q => q.Client)
                    .WithMany()
                    .HasForeignKey(q => q.ClientId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure QuotationItem entity
            modelBuilder.Entity<QuotationItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ItemName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.UnitCost).HasColumnType("decimal(18,2)");
                entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)");

                // Relationship with Quotation
                entity.HasOne(qi => qi.Quotation)
                    .WithMany(q => q.Items)
                    .HasForeignKey(qi => qi.QuotationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
