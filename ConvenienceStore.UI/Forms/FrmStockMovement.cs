using System;
using System.Linq;
using System.Windows.Forms;
using ConvenienceStore.Business.Models;
using ConvenienceStore.Business.Services;
using ConvenienceStore.Data.Entities;
using ConvenienceStore.UI.Helpers;
namespace ConvenienceStore.UI.Forms
{
    public partial class FrmStockMovement : Form
    {
        private readonly int actorId; private readonly Task3Service service;
        private readonly DataGridView dgvMovements = new DataGridView { Dock = DockStyle.Fill };
        private readonly DataGridView dgvBalances = new DataGridView { Dock = DockStyle.Fill };
        public FrmStockMovement() : this(0) { }
        public FrmStockMovement(int currentUserId) : this(currentUserId, new Task3Service()) { }
        public FrmStockMovement(int currentUserId, Task3Service task3Service)
        {
            actorId = currentUserId; service = task3Service; InitializeComponent();
            var first = new TabPage("Lịch sử giao dịch"); first.Controls.Add(dgvMovements); tabs.TabPages.Add(first);
            var second = new TabPage("Nhập - Xuất - Tồn theo kỳ"); second.Controls.Add(dgvBalances); tabs.TabPages.Add(second);
            btnFilter.Click += (s, e) => Task3Ui.Run(this, LoadReport);
            btnExport.Click += (s, e) => Task3Ui.Run(this, () => Task3Ui.ExportCsv(this, tabs.SelectedIndex == 0 ? dgvMovements : dgvBalances, "TonKho.csv"));
        }
        private void Form_Load(object sender, EventArgs e)
        {
            if (Task3Ui.IsDesigner(this)) return;
            dtpFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); dtpTo.Value = DateTime.Today;
            cboKind.Items.AddRange(new object[] { "Tất cả nhập / bán", "Nhập hàng", "Bán hàng" }); cboKind.SelectedIndex = 0;
            Task3Ui.Grid(dgvMovements, "CreatedAt|Thời gian|120|dd/MM/yyyy HH:mm", "ReferenceNumber|Chứng từ|180", "ProductName|Sản phẩm|155", "Kind|Loại|80", "Quantity|Biến động|65|N0", "StockBefore|Trước|60|N0", "StockAfter|Sau|60|N0");
            Task3Ui.Grid(dgvBalances, "ProductName|Sản phẩm|210", "Opening|Tồn đầu|90|N0", "Purchased|Nhập|90|N0", "Sold|Bán|90|N0", "Closing|Tồn cuối kỳ|100|N0", "Current|Tồn hiện tại|100|N0");
            Task3Ui.Run(this, () => {
                var options = service.LoadOptions(actorId, Task3Action.Stock).Products;
                options.Insert(0, new ProductOption { Id = 0, Name = "Tất cả sản phẩm" });
                cboProduct.DataSource = options; cboProduct.DisplayMember = "Name"; cboProduct.ValueMember = "Id"; LoadReport();
            });
        }
        private void LoadReport()
        {
            var product = cboProduct.SelectedItem as ProductOption;
            var data = service.Stock(actorId, dtpFrom.Value, dtpTo.Value, product == null || product.Id == 0 ? (int?)null : product.Id,
                cboKind.SelectedIndex <= 0 ? (StockMovementKind?)null : (StockMovementKind)cboKind.SelectedIndex);
            dgvMovements.DataSource = data.Movements; dgvBalances.DataSource = data.Balances;
        }
    }
}
