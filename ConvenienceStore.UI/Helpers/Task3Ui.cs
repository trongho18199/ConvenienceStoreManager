using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ConvenienceStore.Business.Models;
namespace ConvenienceStore.UI.Helpers
{
    internal static class Task3Ui
    {
        public static bool IsDesigner(Control form)
        { return LicenseManager.UsageMode == LicenseUsageMode.Designtime || (form.Site != null && form.Site.DesignMode); }
        public static bool Run(IWin32Window owner, Action action)
        {
            try { action(); return true; }
            catch (Task3Exception ex) { MessageBox.Show(owner, ex.Message, "Kiểm tra dữ liệu / quyền", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (DbUpdateConcurrencyException) { MessageBox.Show(owner, "Tồn kho vừa thay đổi ở máy khác. Hãy tải lại và kiểm tra giỏ hàng.", "Dữ liệu đã thay đổi"); }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(ex.ToString());
                MessageBox.Show(owner, "Chưa thực hiện được thao tác. Kiểm tra kết nối và migration Task3Operations. Nếu lỗi xảy ra khi lưu, giữ nguyên phiếu và thử lại để kiểm tra mã giao dịch; không tạo phiếu khác ngay.", "Không thực hiện được", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
        public static void Grid(DataGridView grid, params string[] specs)
        {
            grid.AutoGenerateColumns = false; grid.ReadOnly = true;
            grid.AllowUserToAddRows = false; grid.AllowUserToDeleteRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; grid.MultiSelect = false;
            grid.RowHeadersVisible = false; grid.BackgroundColor = Color.White; grid.BorderStyle = BorderStyle.None;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(28, 57, 73);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersHeight = 36; grid.RowTemplate.Height = 30;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(241, 247, 247);
            grid.Columns.Clear();
            foreach (var spec in specs)
            {
                var parts = spec.Split('|');
                var col = new DataGridViewTextBoxColumn { DataPropertyName = parts[0], Name = parts[0], HeaderText = parts[1], FillWeight = parts.Length > 2 ? float.Parse(parts[2], CultureInfo.InvariantCulture) : 100f };
                if (parts.Length > 3) col.DefaultCellStyle.Format = parts[3];
                grid.Columns.Add(col);
            }
        }
        public static void ExportCsv(IWin32Window owner, DataGridView grid, string filename)
        {
            using (var dialog = new SaveFileDialog { Filter = "CSV UTF-8 (*.csv)|*.csv", FileName = filename })
            {
                if (dialog.ShowDialog(owner) != DialogResult.OK) return;
                using (var writer = new StreamWriter(dialog.FileName, false, new UTF8Encoding(true)))
                {
                    Func<object, string> field = x => {
                        string value = Convert.ToString(x, CultureInfo.InvariantCulture) ?? "";
                        // Tránh Excel hiểu chuỗi người dùng nhập là công thức.
                        if (value.Length > 0 && "=+@".Contains(value[0])) value = "'" + value;
                        return "\"" + value.Replace("\"", "\"\"") + "\"";
                    };
                    writer.WriteLine(string.Join(",", grid.Columns.Cast<DataGridViewColumn>().Select(x => field(x.HeaderText))));
                    foreach (DataGridViewRow row in grid.Rows)
                        writer.WriteLine(string.Join(",", row.Cells.Cast<DataGridViewCell>().Select(x => field(x.Value))));
                }
            }
        }
    }
}
