using System;
using System.Windows.Forms;
using ConvenienceStore.Business.Services;
using ConvenienceStore.UI.Helpers;
namespace ConvenienceStore.UI.Forms
{
    public partial class FrmRevenueReport : Form
    {
        private readonly int actorId; private readonly Task3Service service;
        private readonly DataGridView dgvDays = new DataGridView { Dock = DockStyle.Fill };
        private readonly DataGridView dgvInvoices = new DataGridView { Dock = DockStyle.Fill };
        public FrmRevenueReport() : this(0) { }
        public FrmRevenueReport(int currentUserId) : this(currentUserId, new Task3Service()) { }
        public FrmRevenueReport(int currentUserId, Task3Service task3Service)
        {
            actorId = currentUserId; service = task3Service; InitializeComponent();
            var first = new TabPage("Doanh thu theo ngày"); first.Controls.Add(dgvDays); tabs.TabPages.Add(first);
            var second = new TabPage("Danh sách hóa đơn"); second.Controls.Add(dgvInvoices); tabs.TabPages.Add(second);
            btnFilter.Click += (s, e) => Task3Ui.Run(this, LoadReport);
            btnExport.Click += (s, e) => Task3Ui.Run(this, () => Task3Ui.ExportCsv(this, tabs.SelectedIndex == 0 ? dgvDays : dgvInvoices, "DoanhThu.csv"));
        }
        private void Form_Load(object sender, EventArgs e)
        {
            if (Task3Ui.IsDesigner(this)) return;
            dtpFrom.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); dtpTo.Value = DateTime.Today;
            Task3Ui.Grid(dgvDays, "Date|Ngày|110|dd/MM/yyyy", "InvoiceCount|Số hóa đơn|85|N0", "Subtotal|Tiền hàng|130|N2", "DiscountAmount|Giảm giá|120|N2", "Revenue|Doanh thu|140|N2");
            Task3Ui.Grid(dgvInvoices, "Number|Hóa đơn|190", "CreatedAt|Thời gian|125|dd/MM/yyyy HH:mm", "CustomerName|Khách hàng|130", "Subtotal|Tiền hàng|100|N2", "DiscountAmount|Giảm giá|85|N2", "Total|Thanh toán|110|N2");
            Task3Ui.Run(this, LoadReport);
        }
        private void LoadReport()
        {
            var data = service.Revenue(actorId, dtpFrom.Value, dtpTo.Value);
            dgvDays.DataSource = data.Days; dgvInvoices.DataSource = data.Invoices;
            lblSummary.Text = data.Invoices.Count + " hóa đơn     |     Tiền hàng: " + data.Subtotal.ToString("N2") + " đ     |     Giảm: " + data.Discount.ToString("N2") + " đ     |     Doanh thu: " + data.Revenue.ToString("N2") + " đ";
        }
    }
}
