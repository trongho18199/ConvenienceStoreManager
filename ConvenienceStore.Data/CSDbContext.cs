using ConvenienceStore.Data.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvenienceStore.Data
{
    public class CSDbContext : DbContext
    {
        public CSDbContext() : base("name=ConvenienceStoreDB") { }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<PurchaseReceipt> PurchaseReceipts { get; set; }
        public DbSet<PurchaseReceiptLine> PurchaseReceiptLines { get; set; }
        public DbSet<SaleInvoice> SaleInvoices { get; set; }
        public DbSet<SaleInvoiceLine> SaleInvoiceLines { get; set; }
        public DbSet<StockMovement> StockMovements { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Properties<decimal>().Configure(p => p.HasPrecision(18, 2));
            modelBuilder.Entity<PurchaseReceipt>().HasRequired(x => x.Supplier).WithMany().HasForeignKey(x => x.SupplierId).WillCascadeOnDelete(false);
            modelBuilder.Entity<PurchaseReceipt>().HasRequired(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).WillCascadeOnDelete(false);
            modelBuilder.Entity<PurchaseReceiptLine>().HasRequired(x => x.PurchaseReceipt).WithMany(x => x.Lines).HasForeignKey(x => x.PurchaseReceiptId).WillCascadeOnDelete(false);
            modelBuilder.Entity<PurchaseReceiptLine>().HasRequired(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).WillCascadeOnDelete(false);
            modelBuilder.Entity<SaleInvoice>().HasOptional(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).WillCascadeOnDelete(false);
            modelBuilder.Entity<SaleInvoice>().HasRequired(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).WillCascadeOnDelete(false);
            modelBuilder.Entity<SaleInvoiceLine>().HasRequired(x => x.SaleInvoice).WithMany(x => x.Lines).HasForeignKey(x => x.SaleInvoiceId).WillCascadeOnDelete(false);
            modelBuilder.Entity<SaleInvoiceLine>().HasRequired(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).WillCascadeOnDelete(false);
            modelBuilder.Entity<StockMovement>().HasRequired(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).WillCascadeOnDelete(false);
            modelBuilder.Entity<StockMovement>().HasRequired(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).WillCascadeOnDelete(false);
            modelBuilder.Entity<StockMovement>().HasOptional(x => x.PurchaseReceipt).WithMany().HasForeignKey(x => x.PurchaseReceiptId).WillCascadeOnDelete(false);
            modelBuilder.Entity<StockMovement>().HasOptional(x => x.SaleInvoice).WithMany().HasForeignKey(x => x.SaleInvoiceId).WillCascadeOnDelete(false);
            base.OnModelCreating(modelBuilder);
        }

    }
}
