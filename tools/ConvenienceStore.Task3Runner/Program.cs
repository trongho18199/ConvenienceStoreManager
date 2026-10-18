using System;
using System.Drawing;
using System.Windows.Forms;
using ConvenienceStore.Business.Models;
using ConvenienceStore.Business.Services;
using ConvenienceStore.Data.Entities;
using ConvenienceStore.UI.Forms;
namespace ConvenienceStore.Task3Runner
{
    internal static class Program
    {
        [STAThread] private static void Main()
        {
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Task3Runner());
        }
    }
    // Cửa sổ test tích hợp: đăng nhập thật từ DB, không tự giả danh Admin.
    internal sealed class Task3Runner : Form
    {
        private User current;
        private readonly TextBox username = new TextBox { Location = new Point(160, 105), Width = 300 };
        private readonly TextBox password = new TextBox { Location = new Point(160, 150), Width = 300, UseSystemPasswordChar = true };
        private readonly Button login = new Button { Text = "Đăng nhập", Location = new Point(480, 102), Size = new Size(155, 77) };
        private readonly Label status = new Label { Text = "Đăng nhập bằng tài khoản trong database của nhóm.", Location = new Point(25, 200), Size = new Size(615, 45) };
        private readonly Button[] modules = new Button[4];
        private readonly Button seed = new Button { Text = "Tạo danh mục mẫu (chỉ database thử nghiệm)", Location = new Point(25, 405), Size = new Size(610, 42), Enabled = false };
        public Task3Runner()
        {
            Text = "Nhiệm vụ 3 - Công cụ chạy thử riêng"; ClientSize = new Size(665, 480); Font = new Font("Segoe UI", 10F); StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle; MaximizeBox = false;
            Controls.Add(new Label { Text = "CHẠY THỬ 4 FORM NHIỆM VỤ 3", Font = new Font("Segoe UI", 17F, FontStyle.Bold), Location = new Point(25, 25), Size = new Size(620, 40) });
            Controls.Add(new Label { Text = "Tài khoản", Location = new Point(25, 108), Width = 120 });
            Controls.Add(new Label { Text = "Mật khẩu", Location = new Point(25, 153), Width = 120 });
            Controls.AddRange(new Control[] { username, password, login, status, seed });
            string[] titles = { "Lập phiếu nhập", "Bán hàng", "Lịch sử kho", "Báo cáo doanh thu" };
            for (int i = 0; i < 4; i++)
            {
                int index = i;
                modules[i] = new Button { Text = titles[i], Location = new Point(25 + (i % 2) * 315, 255 + (i / 2) * 65), Size = new Size(295, 50), Enabled = false };
                modules[i].Click += (s, e) => {
                    Form form = index == 0 ? (Form)new FrmPurchaseReceipt(current.Id) : index == 1 ? new FrmSales(current.Id) : index == 2 ? new FrmStockMovement(current.Id) : (Form)new FrmRevenueReport(current.Id);
                    using (form) form.ShowDialog(this);
                };
                Controls.Add(modules[i]);
            }
            login.Click += (s, e) => {
                if (current != null) { current = null; password.Clear(); SetPermissions(); return; }
                try { current = new Task3DemoService().Login(username.Text.Trim(), password.Text); password.Clear(); SetPermissions(); }
                catch (Task3Exception ex) { MessageBox.Show(this, ex.Message); }
                catch (Exception ex) { System.Diagnostics.Trace.TraceError(ex.ToString()); MessageBox.Show(this, "Chưa kết nối được database. Kiểm tra App.config và chạy Update-Database theo hướng dẫn."); }
            };
            seed.Click += (s, e) => {
                if (MessageBox.Show(this, "Thêm danh mục NV3 vào database thử nghiệm hiện tại? Các sản phẩm bắt đầu với tồn 0.", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
                try { new Task3DemoService().CreateSampleCatalog(current.Id); MessageBox.Show(this, "Đã tạo danh mục mẫu. Hãy nhập hàng trước khi bán."); }
                catch (Exception ex) { System.Diagnostics.Trace.TraceError(ex.ToString()); MessageBox.Show(this, "Không tạo được danh mục mẫu. Kiểm tra kết nối và quyền Admin."); }
            };
            AcceptButton = login;
        }
        private void SetPermissions()
        {
            string role = current == null ? "" : current.Role.Name;
            for (int i = 0; i < 4; i++) modules[i].Enabled = current != null && Task3Service.Can(role, (Task3Action)i);
            seed.Enabled = current != null && role == "Admin";
            username.Enabled = current == null; password.Enabled = current == null;
            login.Text = current == null ? "Đăng nhập" : "Đăng xuất";
            status.Text = current == null ? "Đăng nhập bằng tài khoản trong database của nhóm." : "Đang dùng: " + current.FullName + " | Nhóm: " + role;
        }
    }
}
