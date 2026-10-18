using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Threading.Tasks;
using ConvenienceStore.Data;
using ConvenienceStore.Data.Entities;
using ConvenienceStore.Data.Repositories;
using ConvenienceStore.Business.Models;
using ConvenienceStore.Business.Services;

public static class SqlSmoke
{
    static void Assert(bool condition, string message) { if(!condition)throw new Exception(message);Console.WriteLine("SQL PASS "+message); }
    public static void Run()
    {
        using(var db=new CSDbContext())
            if(db.Database.Connection.Database != "ConvenienceStore_Task3_Test")
                throw new InvalidOperationException("SQL checks only run against the dedicated ConvenienceStore_Task3_Test database.");
        var config=(DbMigrationsConfiguration)Activator.CreateInstance(typeof(CSDbContext).Assembly.GetType("ConvenienceStore.Data.Migrations.Configuration"),true);
        new DbMigrator(config).Update();
        int actor,productId,supplierId,customerId;
        string key=Guid.NewGuid().ToString("N").Substring(0,12);
        using(var db=new CSDbContext())
        {
            actor=db.Users.Where(x=>x.IsActive && x.Role.Name=="Admin").Select(x=>x.Id).First();
            var category=new Category{Name="SQL test "+key};
            var product=new Product{Name="SQL test "+key,Barcode="SQL-"+key,Price=20000m,Stock=0,Category=category};
            var supplier=new Supplier{Name="SQL test "+key};
            var customer=new Customer{Name="SQL VIP "+key,IsVip=true};
            db.Products.Add(product);db.Suppliers.Add(supplier);db.Customers.Add(customer);db.SaveChanges();
            productId=product.Id;supplierId=supplier.Id;customerId=customer.Id;
        }
        var service=new Task3Service();
        var purchase=new PurchaseRequest{OperationId=Guid.NewGuid(),SupplierId=supplierId,Lines=new List<CartLine>{new CartLine{ProductId=productId,Quantity=5,UnitPrice=12000m}}};
        service.Purchase(actor,purchase);service.Purchase(actor,purchase);
        using(var db=new CSDbContext()) Assert(db.Products.Find(productId).Stock==5 && db.PurchaseReceipts.Count(x=>x.OperationId==purchase.OperationId)==1,"actual purchase and retry");
        Func<SaleRequest> request=()=>new SaleRequest{OperationId=Guid.NewGuid(),CustomerId=customerId,Lines=new List<CartLine>{new CartLine{ProductId=productId,Quantity=2}},ExpectedTotal=36000m,CashReceived=50000m};
        var sale=request();var paid=service.Checkout(actor,sale);service.Checkout(actor,sale);
        using(var db=new CSDbContext()) Assert(paid.Change==14000m && db.Products.Find(productId).Stock==3 && db.SaleInvoices.Count(x=>x.OperationId==sale.OperationId)==1,"actual checkout VIP totals and retry");
        var failed=request();
        try { new Task3Service(()=>new FailAfterSaveSession()).Checkout(actor,failed); throw new Exception("No injected failure"); }
        catch(InvalidOperationException) { }
        using(var db=new CSDbContext())
        {
            Assert(db.Products.Find(productId).Stock==3,"SQL rollback restores stock after SaveChanges");
            Assert(!db.SaleInvoices.Any(x=>x.OperationId==failed.OperationId),"SQL rollback removes invoice after SaveChanges");
            Assert(db.StockMovements.Count(x=>x.ProductId==productId)==2,"SQL rollback removes movement after SaveChanges");
        }
        // Hai quầy cùng mua 2 sản phẩm trong khi chỉ còn 3: chỉ một giao dịch được thành công.
        Func<bool> attempt=()=> { try { service.Checkout(actor,request());return true; } catch(Exception ex) { Console.WriteLine("Expected competing checkout rejection: "+ex.GetType().Name);return false; } };
        var one=Task.Run(attempt);var two=Task.Run(attempt);Task.WaitAll(one,two);
        using(var db=new CSDbContext()) Assert((one.Result?1:0)+(two.Result?1:0)==1 && db.Products.Find(productId).Stock==1,"concurrent checkout cannot oversell");
        Console.WriteLine("SQL smoke checks passed on dedicated test database.");
    }
    private sealed class FailAfterSaveSession : ITask3Session
    {
        readonly EfTask3Session inner=new EfTask3Session();
        public IQueryable<Product> Products { get { return inner.Products; } }
        public IQueryable<User> Users { get { return inner.Users; } }
        public IQueryable<Supplier> Suppliers { get { return inner.Suppliers; } }
        public IQueryable<Customer> Customers { get { return inner.Customers; } }
        public IQueryable<PurchaseReceipt> Purchases { get { return inner.Purchases; } }
        public IQueryable<SaleInvoice> Sales { get { return inner.Sales; } }
        public IQueryable<StockMovement> Movements { get { return inner.Movements; } }
        public void Add(PurchaseReceipt x) { inner.Add(x); }
        public void Add(SaleInvoice x) { inner.Add(x); }
        public void Add(StockMovement x) { inner.Add(x); }
        public ITask3Transaction BeginTransaction() { return inner.BeginTransaction(); }
        public void SaveChanges() { inner.SaveChanges();throw new InvalidOperationException("Injected failure after SQL SaveChanges, before Commit"); }
        public void Dispose() { inner.Dispose(); }
    }
}
