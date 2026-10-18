using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using ConvenienceStore.Data.Entities;
using ConvenienceStore.Data.Repositories;
using ConvenienceStore.Business.Models;
using ConvenienceStore.Business.Services;
using ConvenienceStore.UI.Forms;

public sealed class MemorySession : ITask3Session
{
    public List<Product> ProductData = new List<Product>();
    public List<User> UserData = new List<User>();
    public List<Supplier> SupplierData = new List<Supplier>();
    public List<Customer> CustomerData = new List<Customer>();
    public List<PurchaseReceipt> PurchaseData = new List<PurchaseReceipt>();
    public List<SaleInvoice> SaleData = new List<SaleInvoice>();
    public List<StockMovement> MovementData = new List<StockMovement>();
    public bool FailSave;
    public bool FailCommit;
    public IQueryable<Product> Products { get { return ProductData.AsQueryable(); } }
    public IQueryable<User> Users { get { return UserData.AsQueryable(); } }
    public IQueryable<Supplier> Suppliers { get { return SupplierData.AsQueryable(); } }
    public IQueryable<Customer> Customers { get { return CustomerData.AsQueryable(); } }
    public IQueryable<PurchaseReceipt> Purchases { get { return PurchaseData.AsQueryable(); } }
    public IQueryable<SaleInvoice> Sales { get { return SaleData.AsQueryable(); } }
    public IQueryable<StockMovement> Movements { get { return MovementData.AsQueryable(); } }
    public void Add(PurchaseReceipt x) { x.Id = PurchaseData.Count + 1; PurchaseData.Add(x); }
    public void Add(SaleInvoice x) { x.Id = SaleData.Count + 1; SaleData.Add(x); }
    public void Add(StockMovement x) { x.Id = MovementData.Count + 1; MovementData.Add(x); }
    public void SaveChanges() { if (FailSave) throw new InvalidOperationException("Injected save failure"); }
    public void Dispose() { }
    public ITask3Transaction BeginTransaction() { return new Tx(this); }
    private sealed class Tx : ITask3Transaction
    {
        readonly MemorySession db;
        readonly Dictionary<int,int> stocks;
        readonly int p,s,m;
        bool committed;
        public Tx(MemorySession value) { db=value;stocks=db.ProductData.ToDictionary(x=>x.Id,x=>x.Stock);p=db.PurchaseData.Count;s=db.SaleData.Count;m=db.MovementData.Count; }
        public void Commit() { if(db.FailCommit) throw new InvalidOperationException("Injected commit failure");committed=true; }
        public void Dispose() { if(committed)return;foreach(var x in db.ProductData)x.Stock=stocks[x.Id];db.PurchaseData.RemoveRange(p,db.PurchaseData.Count-p);db.SaleData.RemoveRange(s,db.SaleData.Count-s);db.MovementData.RemoveRange(m,db.MovementData.Count-m); }
    }
    public static MemorySession Seed()
    {
        var db=new MemorySession();
        db.UserData.AddRange(new[]{new User{Id=1,IsActive=true,Role=new Role{Name="Admin"}},new User{Id=2,IsActive=true,Role=new Role{Name="ThuKho"}},new User{Id=3,IsActive=true,Role=new Role{Name="ThuNgan"}},new User{Id=4,IsActive=false,Role=new Role{Name="Admin"}}});
        db.ProductData.AddRange(new[]{new Product{Id=1,Name="Sữa hộp",Barcode="NV3-001",Price=20000m,Stock=20},new Product{Id=2,Name="Bánh quy",Barcode="NV3-002",Price=15000m,Stock=30}});
        db.SupplierData.Add(new Supplier{Id=1,Name="Nhà cung cấp mẫu"});db.CustomerData.Add(new Customer{Id=1,Name="Khách VIP",IsVip=true});return db;
    }
}
public static class Tests
{
    static int count;
    static void Check(bool ok,string name) { if(!ok)throw new Exception("FAIL "+name);Console.WriteLine("PASS "+name);count++; }
    static void Reject(Action action,string name) { try{action();}catch(Task3Exception){Check(true,name);return;}throw new Exception("Did not reject: "+name); }
    static List<CartLine> Lines(int quantity=2) { return new List<CartLine>{new CartLine{ProductId=1,Quantity=quantity,UnitPrice=20000m}}; }
    static SaleRequest Sale(int quantity=2) { return new SaleRequest{OperationId=Guid.NewGuid(),Lines=Lines(quantity),ExpectedTotal=quantity*20000m,CashReceived=1000000m}; }
    [STAThread] public static void Main(string[] args)
    {
        if(args.Length>0 && args[0]=="--sql") { SqlSmoke.Run();return; }
        var db=MemorySession.Seed();var svc=new Task3Service(()=>db);
        var request=new PurchaseRequest{OperationId=Guid.NewGuid(),SupplierId=1,Lines=new List<CartLine>{new CartLine{ProductId=1,Quantity=5,UnitPrice=12000m},new CartLine{ProductId=2,Quantity=3,UnitPrice=10000m}}};
        var receipt=svc.Purchase(2,request);
        Check(receipt.Total==90000m && db.ProductData[0].Stock==25 && db.ProductData[1].Stock==33,"purchase totals and increases both stocks");
        Check(db.MovementData.Count==2 && db.MovementData[0].StockBefore==20 && db.MovementData[0].StockAfter==25,"purchase audit snapshots");
        Check(svc.Purchase(2,request).AlreadySaved && db.PurchaseData.Count==1 && db.ProductData[0].Stock==25,"purchase retry cannot add stock twice");
        var sale=Sale();sale.CustomerId=1;sale.ExpectedTotal=36000m;sale.CashReceived=50000m;
        var paid=svc.Checkout(3,sale);
        Check(paid.Total==36000m && paid.Change==14000m && db.ProductData[0].Stock==23,"VIP price discount payment change and stock");
        Check(db.SaleData[0].Subtotal==40000m && db.SaleData[0].DiscountAmount==4000m,"invoice stores price and discount snapshots");
        Check(svc.Checkout(3,sale).AlreadySaved && db.SaleData.Count==1 && db.ProductData[0].Stock==23,"checkout retry cannot sell twice");
        Reject(()=>svc.Checkout(3,Sale(100)),"insufficient stock");
        var shortCash=Sale();shortCash.CashReceived=1m;Reject(()=>svc.Checkout(3,shortCash),"insufficient payment");
        var bad=Sale();bad.Lines[0].Quantity=0;Reject(()=>svc.Checkout(3,bad),"zero quantity");
        bad=Sale();bad.Lines[0].Quantity=-1;Reject(()=>svc.Checkout(3,bad),"negative quantity");
        bad=Sale();bad.Lines.Add(new CartLine{ProductId=1,Quantity=1});Reject(()=>svc.Checkout(3,bad),"duplicate product rows");
        bad=Sale();bad.Lines[0].ProductId=999;Reject(()=>svc.Checkout(3,bad),"deleted product");
        bad=Sale();bad.ExpectedTotal=1m;Reject(()=>svc.Checkout(3,bad),"changed quoted total");
        bad=Sale();bad.Lines[0].UnitPrice=1m;Check(svc.Checkout(3,bad).Total==40000m,"server ignores forged cart price");
        Reject(()=>svc.Purchase(3,request),"cashier cannot purchase");
        Reject(()=>svc.Checkout(2,Sale()),"warehouse cannot sell");
        Reject(()=>svc.Checkout(4,Sale()),"inactive admin denied");
        Reject(()=>svc.Checkout(0,Sale()),"missing login denied");
        Reject(()=>svc.Revenue(3,DateTime.Today,DateTime.Today),"cashier cannot read revenue");
        int stock=db.ProductData[0].Stock,sales=db.SaleData.Count,moves=db.MovementData.Count;
        db.FailSave=true;try{svc.Checkout(1,Sale());throw new Exception("fault not injected");}catch(InvalidOperationException){}db.FailSave=false;
        Check(db.ProductData[0].Stock==stock && db.SaleData.Count==sales && db.MovementData.Count==moves,"save failure rolls back all service changes using transaction contract");
        db.FailCommit=true;try{svc.Checkout(1,Sale());throw new Exception("fault not injected");}catch(InvalidOperationException){}db.FailCommit=false;
        Check(db.ProductData[0].Stock==stock && db.SaleData.Count==sales && db.MovementData.Count==moves,"commit failure rolls back through transaction contract");
        var secondBad=new PurchaseRequest{OperationId=Guid.NewGuid(),SupplierId=1,Lines=new List<CartLine>{new CartLine{ProductId=1,Quantity=1,UnitPrice=100m},new CartLine{ProductId=2,Quantity=1,UnitPrice=-5m}}};
        Reject(()=>svc.Purchase(1,secondBad),"bad second purchase line");
        Check(db.ProductData[0].Stock==stock && db.PurchaseData.Count==1 && db.MovementData.Count==moves,"bad second line leaves no partial stock or receipt");
        db.SaleData.Add(new SaleInvoice{Id=200,Number="DATE-1",CreatedAt=DateTime.Today.AddHours(23).AddMinutes(59),CustomerName="Test",Subtotal=100m,DiscountAmount=10m,Total=90m});
        db.SaleData.Add(new SaleInvoice{Id=201,Number="DATE-2",CreatedAt=DateTime.Today.AddDays(1),CustomerName="Test",Subtotal=999m,Total=999m});
        var report=svc.Revenue(1,DateTime.Today,DateTime.Today);
        Check(report.Invoices.Any(x=>x.Number=="DATE-1") && !report.Invoices.Any(x=>x.Number=="DATE-2"),"report includes end day 23:59 and excludes following midnight");
        Check(report.Revenue==db.SaleData.Where(x=>x.CreatedAt>=DateTime.Today && x.CreatedAt<DateTime.Today.AddDays(1)).Sum(x=>x.Total),"daily and invoice revenue totals reconcile");
        Check(svc.Revenue(1,DateTime.Today.AddYears(-2),DateTime.Today.AddYears(-2)).Revenue==0m,"empty report returns zero");
        Reject(()=>svc.Revenue(1,DateTime.Today.AddDays(1),DateTime.Today),"reversed report dates");
        var inventory=svc.Stock(2,DateTime.Today,DateTime.Today,null,null);
        Check(inventory.Balances.All(x=>x.Opening+x.Purchased-x.Sold==x.Closing),"stock opening plus receipts minus sales equals closing");
        Check(svc.Stock(2,DateTime.Today,DateTime.Today,1,StockMovementKind.Sale).Movements.All(x=>x.Quantity<0 && x.ProductName=="Sữa hộp"),"stock product and movement type filters");
        if(args.Length>0)
        {
            Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);
            var forms=new Form[]{new FrmPurchaseReceipt(1,svc),new FrmSales(1,svc),new FrmStockMovement(1,svc),new FrmRevenueReport(1,svc)};
            string[] files={"01_NhapHang.png","02_BanHang.png","03_LichSuKho.png","04_DoanhThu.png"};
            for(int i=0;i<forms.Length;i++)
            {
                using(var form=forms[i])
                {
                    form.Show();Application.DoEvents();
                    if(i<2)
                    {
                        var cbo=(ComboBox)form.Controls.Find("cboProduct",true)[0];cbo.SelectedIndex=0;
                        ((NumericUpDown)form.Controls.Find("numQuantity",true)[0]).Value=2;
                        if(i==0)((NumericUpDown)form.Controls.Find("numUnitPrice",true)[0]).Value=12000m;
                        ((Button)form.Controls.Find("btnAdd",true)[0]).PerformClick();Application.DoEvents();
                        var grid=(DataGridView)form.Controls.Find("dgvCart",true)[0];Check(grid.Rows.Count==1,"actual form add button "+i);
                    }
                    System.Threading.Thread.Sleep(150); Application.DoEvents();
                    using(var bmp=new Bitmap(form.Width,form.Height)){using(var graphics=Graphics.FromImage(bmp))graphics.CopyFromScreen(form.Location,Point.Empty,bmp.Size);bmp.Save(System.IO.Path.Combine(args[0],files[i]));}
                    // Clear test cart before closing, avoiding the unsaved-changes dialog.
                    var field=form.GetType().GetField("cart",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
                    if(field!=null)((System.ComponentModel.BindingList<CartLine>)field.GetValue(form)).Clear();
                    form.Close();Application.DoEvents();Check(true,"form opens and closes "+i);
                }
            }
        }
        Console.WriteLine("ALL "+count+" CHECKS PASSED (memory session; SQL Server integration still requires Windows checks)");
    }
}
