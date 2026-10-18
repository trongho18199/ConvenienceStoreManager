using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using ConvenienceStore.Data.Entities;
namespace ConvenienceStore.Data.Repositories
{
    public sealed class EfTask3Session : ITask3Session
    {
        private readonly CSDbContext db = new CSDbContext();
        public IQueryable<Product> Products { get { return db.Products; } }
        public IQueryable<User> Users { get { return db.Users; } }
        public IQueryable<Supplier> Suppliers { get { return db.Suppliers; } }
        public IQueryable<Customer> Customers { get { return db.Customers; } }
        public IQueryable<PurchaseReceipt> Purchases { get { return db.PurchaseReceipts; } }
        public IQueryable<SaleInvoice> Sales { get { return db.SaleInvoices; } }
        public IQueryable<StockMovement> Movements { get { return db.StockMovements; } }
        public ITask3Transaction BeginTransaction()
        {
            // Đọc tồn và cập nhật nằm cùng giao dịch, chống hai quầy cùng bán vượt tồn.
            return new Transaction(db.Database.BeginTransaction(IsolationLevel.Serializable));
        }
        public void Add(PurchaseReceipt x) { db.PurchaseReceipts.Add(x); }
        public void Add(SaleInvoice x) { db.SaleInvoices.Add(x); }
        public void Add(StockMovement x) { db.StockMovements.Add(x); }
        public void SaveChanges() { db.SaveChanges(); }
        public void Dispose() { db.Dispose(); }
        private sealed class Transaction : ITask3Transaction
        {
            private readonly DbContextTransaction transaction;
            private bool committed;
            public Transaction(DbContextTransaction value) { transaction = value; }
            public void Commit() { transaction.Commit(); committed = true; }
            public void Dispose()
            {
                try { if (!committed) transaction.Rollback(); }
                finally { transaction.Dispose(); }
            }
        }
    }
}
