using System;
using System.Linq;
using ConvenienceStore.Data.Entities;
namespace ConvenienceStore.Data.Repositories
{
    public interface ITask3Transaction : IDisposable { void Commit(); }
    // Một session cho mỗi thao tác; không giữ DbContext theo vòng đời Form.
    public interface ITask3Session : IDisposable
    {
        IQueryable<Product> Products { get; }
        IQueryable<User> Users { get; }
        IQueryable<Supplier> Suppliers { get; }
        IQueryable<Customer> Customers { get; }
        IQueryable<PurchaseReceipt> Purchases { get; }
        IQueryable<SaleInvoice> Sales { get; }
        IQueryable<StockMovement> Movements { get; }
        ITask3Transaction BeginTransaction();
        void Add(PurchaseReceipt receipt);
        void Add(SaleInvoice invoice);
        void Add(StockMovement movement);
        void SaveChanges();
    }
}
