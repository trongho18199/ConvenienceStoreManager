using System;
using System.Collections.Generic;
using System.Linq;
using ConvenienceStore.Business.Models;
using ConvenienceStore.Data.Entities;
using ConvenienceStore.Data.Repositories;
namespace ConvenienceStore.Business.Services
{
    public sealed class Task3Service
    {
        private readonly Func<ITask3Session> createSession;
        public Task3Service() : this(() => new EfTask3Session()) { }
        public Task3Service(Func<ITask3Session> sessionFactory)
        { createSession = sessionFactory ?? throw new ArgumentNullException("sessionFactory"); }

        public static bool Can(string role, Task3Action action)
        {
            if (role == "Admin" || role == "QuanLy") return true;
            if (role == "ThuKho") return action == Task3Action.Purchase || action == Task3Action.Stock;
            if (role == "ThuNgan") return action == Task3Action.Sale;
            return false;
        }
        private static void Require(ITask3Session db, int actorId, Task3Action action)
        {
            var user = db.Users.Where(x => x.Id == actorId && x.IsActive)
                .Select(x => new { Role = x.Role.Name }).SingleOrDefault();
            if (user == null || !Can(user.Role, action))
                throw new Task3Exception("Tài khoản chưa đăng nhập, bị khóa hoặc không có quyền thực hiện chức năng này.");
        }
        private static List<CartLine> ValidateLines(List<CartLine> lines)
        {
            if (lines == null || lines.Count == 0) throw new Task3Exception("Chưa có sản phẩm trong phiếu.");
            if (lines.Count > 500) throw new Task3Exception("Mỗi phiếu tối đa 500 dòng sản phẩm.");
            if (lines.Any(x => x == null || x.ProductId <= 0 || x.Quantity < 1 || x.Quantity > 1000000))
                throw new Task3Exception("Sản phẩm hoặc số lượng không hợp lệ (1 đến 1.000.000).");
            if (lines.Select(x => x.ProductId).Distinct().Count() != lines.Count)
                throw new Task3Exception("Một sản phẩm chỉ được có một dòng; hãy gộp số lượng trước khi lưu.");
            return lines.OrderBy(x => x.ProductId).ToList(); // Thứ tự khóa ổn định.
        }
        private static DateTime EndExclusive(DateTime from, DateTime to)
        {
            if (from.Date > to.Date || to.Date == DateTime.MaxValue.Date)
                throw new Task3Exception("Khoảng ngày không hợp lệ.");
            return to.Date.AddDays(1);
        }
        public LookupData LoadOptions(int actorId, Task3Action action)
        {
            using (var db = createSession())
            {
                Require(db, actorId, action);
                var result = new LookupData {
                    Products = db.Products.OrderBy(x => x.Name).Select(x => new ProductOption {
                        Id = x.Id, Name = x.Name, Barcode = x.Barcode, Price = x.Price, Stock = x.Stock }).ToList(),
                    Suppliers = db.Suppliers.Where(x => x.IsActive).OrderBy(x => x.Name)
                        .Select(x => new PartyOption { Id = x.Id, Name = x.Name }).ToList(),
                    Customers = db.Customers.Where(x => x.IsActive).OrderBy(x => x.Name)
                        .Select(x => new PartyOption { Id = x.Id, Name = x.Name, IsVip = x.IsVip }).ToList()
                };
                result.Customers.Insert(0, new PartyOption { Id = 0, Name = "Khách lẻ" });
                return result;
            }
        }

        public DocumentResult Purchase(int actorId, PurchaseRequest request)
        {
            if (request == null || request.OperationId == Guid.Empty) throw new Task3Exception("Thiếu mã thao tác lưu phiếu.");
            var lines = ValidateLines(request.Lines);
            if ((request.Note ?? "").Length > 300) throw new Task3Exception("Ghi chú tối đa 300 ký tự.");
            using (var db = createSession())
            using (var tx = db.BeginTransaction())
            {
                Require(db, actorId, Task3Action.Purchase);
                var saved = db.Purchases.SingleOrDefault(x => x.OperationId == request.OperationId);
                if (saved != null) return new DocumentResult { Id = saved.Id, Number = saved.Number, Total = saved.Total, AlreadySaved = true };
                var supplier = db.Suppliers.SingleOrDefault(x => x.Id == request.SupplierId && x.IsActive);
                if (supplier == null) throw new Task3Exception("Hãy chọn nhà cung cấp đang hoạt động.");
                var ids = lines.Select(x => x.ProductId).ToList();
                var products = db.Products.Where(x => ids.Contains(x.Id)).ToDictionary(x => x.Id);
                if (products.Count != lines.Count) throw new Task3Exception("Có sản phẩm đã bị xóa. Hãy tải lại danh sách.");
                var receipt = new PurchaseReceipt { OperationId = request.OperationId,
                    Number = "PN-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + request.OperationId.ToString("N").Substring(0, 8),
                    CreatedAt = DateTime.Now, SupplierId = supplier.Id, SupplierName = supplier.Name,
                    CreatedById = actorId, Note = (request.Note ?? "").Trim() };
                foreach (var line in lines)
                {
                    var product = products[line.ProductId];
                    if (line.UnitPrice <= 0m) throw new Task3Exception("Đơn giá nhập phải lớn hơn 0.");
                    var cost = Pricing.Money(line.UnitPrice);
                    if (cost != line.UnitPrice) throw new Task3Exception("Đơn giá nhập chỉ có tối đa 2 chữ số thập phân.");
                    int before = product.Stock;
                    if (before < 0 || before > int.MaxValue - line.Quantity) throw new Task3Exception("Số tồn kho không hợp lệ hoặc vượt giới hạn.");
                    product.Stock += line.Quantity;
                    var detail = new PurchaseReceiptLine { ProductId = product.Id, ProductName = product.Name,
                        Quantity = line.Quantity, UnitCost = cost, LineTotal = Pricing.Money(cost * line.Quantity) };
                    receipt.Lines.Add(detail);
                    receipt.Total = Pricing.Money(receipt.Total + detail.LineTotal);
                    db.Add(new StockMovement { ProductId = product.Id, ProductName = product.Name,
                        CreatedAt = receipt.CreatedAt, Kind = StockMovementKind.Purchase, Quantity = line.Quantity,
                        StockBefore = before, StockAfter = product.Stock, ReferenceNumber = receipt.Number,
                        CreatedById = actorId, PurchaseReceipt = receipt });
                }
                db.Add(receipt);
                db.SaveChanges();
                tx.Commit();
                return new DocumentResult { Id = receipt.Id, Number = receipt.Number, Total = receipt.Total };
            }
        }

        public DocumentResult Checkout(int actorId, SaleRequest request)
        {
            if (request == null || request.OperationId == Guid.Empty) throw new Task3Exception("Thiếu mã thao tác thanh toán.");
            var lines = ValidateLines(request.Lines);
            using (var db = createSession())
            using (var tx = db.BeginTransaction())
            {
                Require(db, actorId, Task3Action.Sale);
                var saved = db.Sales.SingleOrDefault(x => x.OperationId == request.OperationId);
                if (saved != null) return new DocumentResult { Id = saved.Id, Number = saved.Number, Total = saved.Total, Change = saved.Change, AlreadySaved = true };
                Customer customer = null;
                if (request.CustomerId.HasValue)
                {
                    customer = db.Customers.SingleOrDefault(x => x.Id == request.CustomerId.Value && x.IsActive);
                    if (customer == null) throw new Task3Exception("Khách hàng không còn hoạt động. Hãy tải lại danh sách.");
                }
                var ids = lines.Select(x => x.ProductId).ToList();
                var products = db.Products.Where(x => ids.Contains(x.Id)).ToDictionary(x => x.Id);
                if (products.Count != lines.Count) throw new Task3Exception("Có sản phẩm đã bị xóa. Hãy tải lại giỏ hàng.");
                var invoice = new SaleInvoice { OperationId = request.OperationId,
                    Number = "HD-" + DateTime.Now.ToString("yyyyMMddHHmmss") + "-" + request.OperationId.ToString("N").Substring(0, 8),
                    CreatedAt = DateTime.Now, CustomerId = request.CustomerId, CustomerName = customer == null ? "Khách lẻ" : customer.Name,
                    CreatedById = actorId, DiscountPercent = customer != null && customer.IsVip ? Pricing.VipPercent : 0m };
                // Tính lại GIÁ và TỒN từ DB trong giao dịch; không tin đơn giá/giảm giá trên Form.
                foreach (var line in lines)
                {
                    var product = products[line.ProductId];
                    if (product.Stock < line.Quantity) throw new Task3Exception(product.Name + ": chỉ còn " + product.Stock + " sản phẩm.");
                    if (product.Price <= 0m) throw new Task3Exception(product.Name + ": chưa có giá bán hợp lệ.");
                    var detail = new SaleInvoiceLine { ProductId = product.Id, ProductName = product.Name,
                        Quantity = line.Quantity, UnitPrice = Pricing.Money(product.Price), LineTotal = Pricing.Money(product.Price * line.Quantity) };
                    invoice.Lines.Add(detail);
                    invoice.Subtotal = Pricing.Money(invoice.Subtotal + detail.LineTotal);
                }
                invoice.DiscountAmount = Pricing.Discount(invoice.Subtotal, invoice.DiscountPercent > 0m);
                invoice.Total = Pricing.Money(invoice.Subtotal - invoice.DiscountAmount);
                if (request.ExpectedTotal != invoice.Total) throw new Task3Exception("Giá bán hoặc hạng thành viên đã thay đổi. Hãy tải lại giỏ hàng rồi xác nhận số tiền mới.");
                invoice.CashReceived = Pricing.Money(request.CashReceived);
                if (request.CashReceived != invoice.CashReceived || invoice.CashReceived < invoice.Total)
                    throw new Task3Exception("Tiền khách đưa chưa đủ hoặc có quá 2 chữ số thập phân.");
                invoice.Change = Pricing.Money(invoice.CashReceived - invoice.Total);
                foreach (var line in lines)
                {
                    var product = products[line.ProductId];
                    int before = product.Stock;
                    product.Stock -= line.Quantity;
                    db.Add(new StockMovement { ProductId = product.Id, ProductName = product.Name,
                        CreatedAt = invoice.CreatedAt, Kind = StockMovementKind.Sale, Quantity = -line.Quantity,
                        StockBefore = before, StockAfter = product.Stock, ReferenceNumber = invoice.Number,
                        CreatedById = actorId, SaleInvoice = invoice });
                }
                db.Add(invoice);
                db.SaveChanges();
                tx.Commit();
                return new DocumentResult { Id = invoice.Id, Number = invoice.Number, Total = invoice.Total, Change = invoice.Change };
            }
        }

        public RevenueData Revenue(int actorId, DateTime from, DateTime to)
        {
            var end = EndExclusive(from, to); var start = from.Date;
            using (var db = createSession())
            {
                Require(db, actorId, Task3Action.Revenue);
                var rows = db.Sales.Where(x => x.CreatedAt >= start && x.CreatedAt < end)
                    .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                    .Select(x => new InvoiceReportRow { Id = x.Id, Number = x.Number, CreatedAt = x.CreatedAt,
                        CustomerName = x.CustomerName, Subtotal = x.Subtotal, DiscountAmount = x.DiscountAmount, Total = x.Total }).ToList();
                return new RevenueData { Invoices = rows,
                    Days = rows.GroupBy(x => x.CreatedAt.Date).OrderBy(x => x.Key).Select(g => new DailyRevenueRow {
                        Date = g.Key, InvoiceCount = g.Count(), Subtotal = g.Sum(x => x.Subtotal),
                        DiscountAmount = g.Sum(x => x.DiscountAmount), Revenue = g.Sum(x => x.Total) }).ToList(),
                    Subtotal = rows.Sum(x => x.Subtotal), Discount = rows.Sum(x => x.DiscountAmount), Revenue = rows.Sum(x => x.Total) };
            }
        }
        public StockData Stock(int actorId, DateTime from, DateTime to, int? productId, StockMovementKind? kind)
        {
            var end = EndExclusive(from, to); var start = from.Date;
            using (var db = createSession())
            using (var tx = db.BeginTransaction())
            {
                Require(db, actorId, Task3Action.Stock);
                var productQuery = db.Products;
                if (productId.HasValue)
                {
                    int selectedId = productId.Value;
                    productQuery = productQuery.Where(x => x.Id == selectedId);
                }
                var products = productQuery.OrderBy(x => x.Name).ToList();
                var ids = products.Select(x => x.Id).ToList();
                var movements = db.Movements.Where(x => ids.Contains(x.ProductId) && x.CreatedAt >= start).ToList();
                var balances = products.Select(p => {
                    var changes = movements.Where(x => x.ProductId == p.Id).ToList();
                    var period = changes.Where(x => x.CreatedAt < end).ToList();
                    return new StockRow { ProductId = p.Id, ProductName = p.Name,
                        Opening = p.Stock - changes.Sum(x => x.Quantity),
                        Purchased = period.Where(x => x.Kind == StockMovementKind.Purchase).Sum(x => x.Quantity),
                        Sold = -period.Where(x => x.Kind == StockMovementKind.Sale).Sum(x => x.Quantity),
                        Closing = p.Stock - changes.Where(x => x.CreatedAt >= end).Sum(x => x.Quantity), Current = p.Stock };
                }).ToList();
                var rows = movements.Where(x => x.CreatedAt < end && (!kind.HasValue || x.Kind == kind.Value))
                    .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Select(x => new MovementRow {
                        CreatedAt = x.CreatedAt, ReferenceNumber = x.ReferenceNumber, ProductName = x.ProductName,
                        Kind = x.Kind == StockMovementKind.Purchase ? "Nhập hàng" : "Bán hàng", Quantity = x.Quantity,
                        StockBefore = x.StockBefore, StockAfter = x.StockAfter }).ToList();
                tx.Commit();
                return new StockData { Movements = rows, Balances = balances };
            }
        }
    }
}
