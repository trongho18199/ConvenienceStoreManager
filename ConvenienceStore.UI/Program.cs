using ConvenienceStore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConvenienceStore.UI
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var db = new CSDbContext())
            {
                var admin = db.Users.Where(u => u.Username == "admin").FirstOrDefault();
                MessageBox.Show(admin != null
                    ? "Kết nối OK. Xin chào " + admin.FullName
                    : "Kết nối được nhưng chưa có user admin");
            }

            Application.Run(new Form1());
        }
    }
}
