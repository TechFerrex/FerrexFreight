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
        public DbSet<VigaVariant> VigaVariants { get; set; }
        public DbSet<FreightQuotation> FreightQuotations { get; set; } // Nueva propiedad para la tabla de cotizaciones de flete  
        public DbSet<EmailVerification> EmailVerifications { get; set; }
        public DbSet<FreightConfirmation> FreightConfirmations { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<FreightStop> FreightStops { get; set; }



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
            modelBuilder.Entity<AluzincVariant>()
        .Property(a => a.PricePerPie)
        .HasPrecision(18, 4); // Ajusta según necesites

            // Configuración para VigaVariant
            modelBuilder.Entity<VigaVariant>()
                .Property(v => v.WeightPerFoot)
                .HasPrecision(18, 4); // Ajusta según necesites

            // Configuración para FreightQuotation
            modelBuilder.Entity<FreightQuotation>()
                .Property(f => f.BaseCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FreightQuotation>()
                .Property(f => f.CostPerKm)
                .HasPrecision(18, 4);

            modelBuilder.Entity<FreightQuotation>()
                .Property(f => f.DistanceKm)
                .HasPrecision(10, 2);

            modelBuilder.Entity<FreightQuotation>()
                .Property(f => f.FreightLatitude)
                .HasPrecision(10, 8);

            modelBuilder.Entity<FreightQuotation>()
                .Property(f => f.FreightLongitude)
                .HasPrecision(11, 8);

            modelBuilder.Entity<FreightQuotation>()
                .Property(f => f.InsuranceCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FreightQuotation>()
                .Property(f => f.TotalCost)
                .HasPrecision(18, 2);

            // Configuración para FreightStop
            modelBuilder.Entity<FreightStop>()
                .Property(f => f.ExtraCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FreightStop>()
                .Property(f => f.Latitude)
                .HasPrecision(10, 8);

            modelBuilder.Entity<FreightStop>()
                .Property(f => f.Longitude)
                .HasPrecision(11, 8);

            // Configuraciones adicionales recomendadas:

            // Configurar claves foráneas si no las tienes

            // Configurar índices para mejor performance
            modelBuilder.Entity<FreightQuotation>()
                .HasIndex(f => f.CreatedDate);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Índices adicionales para optimización de consultas frecuentes
            modelBuilder.Entity<FreightQuotation>()
                .HasIndex(f => f.UserId);

            modelBuilder.Entity<FreightQuotation>()
                .HasIndex(f => f.Status);

            modelBuilder.Entity<FreightStop>()
                .HasIndex(fs => fs.FreightQuotationId);

            modelBuilder.Entity<Products>()
                .HasIndex(p => p.CategoriaID);

            modelBuilder.Entity<Products>()
                .HasIndex(p => p.id_subcategory);

            modelBuilder.Entity<Products>()
                .HasIndex(p => p.Codigo);

            modelBuilder.Entity<Quotation>()
                .HasIndex(q => q.UserID);

            modelBuilder.Entity<Order>()
                .HasIndex(o => o.UserId);

            modelBuilder.Entity<SubCategory>()
                .HasIndex(s => s.id_categories);

            modelBuilder.Entity<Subcategory2>()
                .HasIndex(s => s.id_subcategory);
        }
    }
}
