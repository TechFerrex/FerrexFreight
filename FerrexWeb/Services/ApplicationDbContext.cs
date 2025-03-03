using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using FerrexWeb.Models;

namespace FerrexWeb.Services
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationDetail> QuotationDetails { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<QuotationNumber> QuotationNumbers { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<ProductVariant> ProductVariants { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }
        public DbSet<Visitor> Visitor { get; set; }
        public DbSet<AluzincVariant> AluzincVariants { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Subcategory2> SubCategories2 { get; set; }



        public bool CanConnect()
        {
            return this.Database.CanConnect();
        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Products>().HasKey(p => p.IdProducto);
            modelBuilder.Entity<User>();
            modelBuilder.Entity<Products>()
                .Property(p => p.Precio)
                .HasPrecision(18, 2);
            modelBuilder.Entity<Products>(entity =>
            {
                entity.Property(e => e.Unit)
                    .HasColumnName("Unit"); // Mapear la columna si es necesario
            });







            modelBuilder.Entity<OrderDetail>()
       .HasKey(od => new { od.OrderId, od.Line });

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderedItems)
                .HasForeignKey(od => od.OrderId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId);
            modelBuilder.Entity<Quotation>()
                .Property(q => q.Freight)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Quotation>()
                .Property(q => q.ISV)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Quotation>()
                .Property(q => q.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Quotation>()
                .Property(q => q.Total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<QuotationDetail>()
                .HasKey(qd => new { qd.QuotationId, qd.Line });

            modelBuilder.Entity<QuotationDetail>()
                .HasOne(qd => qd.Quotation)
                .WithMany(q => q.QuotedItems)
                .HasForeignKey(qd => qd.QuotationId);

            modelBuilder.Entity<QuotationDetail>()
                .HasOne(qd => qd.Product)
                .WithMany()
                .HasForeignKey(qd => qd.ProductId);

            modelBuilder.Entity<QuotationDetail>()
                .Property(qd => qd.Price)
                .HasPrecision(18, 2);
        }
    }
}
