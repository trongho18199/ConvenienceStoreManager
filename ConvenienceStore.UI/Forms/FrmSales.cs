using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using ConvenienceStore.Business.Models;
using ConvenienceStore.Business.Services;
using ConvenienceStore.UI.Helpers;
namespace ConvenienceStore.UI.Forms
{
    public partial class FrmSales : Form
    {
        private readonly int actorId;
        private readonly Task3Service service;
        private readonly BindingList<CartLine> cart = new BindingList<CartLine>();
        private List<ProductOption> products = new List<ProductOption>();
        private Guid operationId = Guid.NewGuid();
        private decimal total;
        public FrmSales() : this(0) { }
        public FrmSales(int currentUserId) : this(currentUserId, new Task3Service()) { }
        public FrmSales(int currentUserId, Task3Service task3Service)
        {
            actorId = currentUserId; service = task3Service; InitializeComponent();
            btnReload.Click += (s, e) => Task3Ui.Run(this, ReloadOptions);
            btnAdd.Click += (s, e) => Task3Ui.Run(this, AddLine);
            btnRemove.Click += (s, e) => { if (dgvCart.CurrentRow != null) { cart.Remove((CartLine)dgvCart.CurrentRow.DataBoundItem); RefreshTotal(); } };
            btnNew.Click += (s, e) => { if (ConfirmClear()) ClearCart(); };
            btnSave.Click += Save_Click;
            cboCustomer.SelectedIndexChanged += (s, e) => RefreshTotal(); numCash.ValueChanged += (s, e) => RefreshTotal();
            FormClosing += (s, e) => { if (cart.Count > 0 && !ConfirmClear()) e.Cancel = true; };
        }
        private bool ConfirmClear()
        { return cart.Count == 0 || MessageBox.Show(this, "Bỏ các dòng chưa lưu của phiếu hiện tại?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes; }
        private void Form_Load(object sender, EventArgs e)
        {
            if (Task3Ui.IsDesigner(this)) return;
            Task3Ui.Grid(dgvCart, "ProductName|Sản phẩm|220", "Quantity|Số lượng|75|N0", "UnitPrice|Đơn giá bán|100|N2", "LineTotal|Thành tiền|110|N2");
            dgvCart.DataSource = cart;
            Task3Ui.Run(this, ReloadOptions); RefreshTotal();
        }
        private void ReloadOptions()
        {
            var data = service.LoadOptions(actorId, Task3Action.Sale);
            products = data.Products;
            cboProduct.DataSource = products; cboProduct.DisplayMember = "Display"; cboProduct.ValueMember = "Id";
            var selected = cboCustomer.SelectedValue; cboCustomer.DataSource = data.Customers; cboCustomer.DisplayMember = "Display"; cboCustomer.ValueMember = "Id"; if (selected != null) cboCustomer.SelectedValue = selected;
            foreach (var line in cart) { var p = products.FirstOrDefault(x => x.Id == line.ProductId); if (p != null) { line.UnitPrice = p.Price; line.ProductName = p.Name; } }
            cart.ResetBindings(); RefreshTotal();
        }
        private void AddLine()
        {
            var product = cboProduct.SelectedItem as ProductOption;
            if (product == null) throw new Task3Exception("Chưa có sản phẩm. Nhờ bạn phụ trách danh mục tạo sản phẩm trước.");
            var line = cart.FirstOrDefault(x => x.ProductId == product.Id);
            int quantity = (int)numQuantity.Value + (line == null ? 0 : line.Quantity);
            if (quantity > 1000000) throw new Task3Exception("Số lượng mỗi sản phẩm tối đa 1.000.000.");
            if (quantity > product.Stock) throw new Task3Exception(product.Name + ": tồn kho chỉ còn " + product.Stock + ".");
            decimal price = product.Price;
            if (price <= 0m) throw new Task3Exception("Đơn giá phải lớn hơn 0.");
            if (line == null) cart.Add(new CartLine { ProductId = product.Id, ProductName = product.Name, Quantity = quantity, UnitPrice = price });
            else { line.Quantity = quantity; line.UnitPrice = price; }
            cart.ResetBindings(); RefreshTotal();
        }
        private void ClearCart()
        { cart.Clear(); operationId = Guid.NewGuid(); numCash.Value = 0m; RefreshTotal(); }
        private void RefreshTotal()
        {
            Task3Ui.Run(this, () => {
                decimal subtotal = Pricing.Money(cart.Sum(x => x.LineTotal));
                var customer = cboCustomer.SelectedItem as PartyOption; decimal discount = Pricing.Discount(subtotal, customer != null && customer.IsVip);
                total = Pricing.Money(subtotal - discount); lblSubtotal.Text = "Tiền hàng: " + subtotal.ToString("N2") + " đ"; lblDiscount.Text = "Giảm giá VIP: " + discount.ToString("N2") + " đ"; lblTotal.Text = "Phải trả: " + total.ToString("N2") + " đ"; lblChange.Text = numCash.Value >= total ? "Tiền thừa: " + (numCash.Value - total).ToString("N2") + " đ" : "Còn thiếu: " + (total - numCash.Value).ToString("N2") + " đ";
            });
        }
        private void Save_Click(object sender, EventArgs e)
        {
            btnSave.Enabled = false;
            try
            {
                Task3Ui.Run(this, () => {
                    var customer = cboCustomer.SelectedItem as PartyOption;
                    var result = service.Checkout(actorId, new SaleRequest { OperationId = operationId, CustomerId = customer == null || customer.Id == 0 ? (int?)null : customer.Id, Lines = cart.ToList(), ExpectedTotal = total, CashReceived = numCash.Value });
                    MessageBox.Show(this, (result.AlreadySaved ? "Giao dịch đã được lưu trước đó: " : "Đã lưu thành công: ") + result.Number + "\nTổng tiền: " + result.Total.ToString("N2") + " đ" + "\nTiền thừa: " + result.Change.ToString("N2") + " đ", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearCart();
                    ReloadOptions();
                });
            }
            finally { btnSave.Enabled = true; }
        }
    }
}
