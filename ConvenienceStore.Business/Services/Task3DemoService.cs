using System;
using System.Linq;
using ConvenienceStore.Business.Models;
using ConvenienceStore.Data;
using ConvenienceStore.Data.Entities;
using ConvenienceStore.Data.Helpers;
namespace ConvenienceStore.Business.Services
{
    // Chỉ được công cụ chạy thử riêng sử dụng; không thay thế module đăng nhập của nhóm.
    public sealed class Task3DemoService
    {
        public User Login(string username, string password)
        {
            string hash = PasswordHasher.Hash(password ?? "");
            using (var db = new CSDbContext())
            {
                var user = db.Users.Include("Role").SingleOrDefault(x => x.Username == username && x.IsActive);
                if (user == null || !string.Equals(user.PasswordHash, hash, StringComparison.Ordinal))
                    throw new Task3Exception("Tên đăng nhập hoặc mật khẩu không đúng.");
                return user;
            }
        }
        public void CreateSampleCatalog(int actorId)
        {
            using (var db = new CSDbContext())
            using (var tx = db.Database.BeginTransaction())
            {
                if (!db.Users.Any(x => x.Id == actorId && x.IsActive && x.Role.Name == "Admin"))
                    throw new Task3Exception("Chỉ Admin được tạo danh mục mẫu.");
                if (!db.Suppliers.Any(x => x.Name == "NV3 - Nhà cung cấp mẫu"))
                    db.Suppliers.Add(new Supplier { Name = "NV3 - Nhà cung cấp mẫu" });
                if (!db.Customers.Any(x => x.Name == "NV3 - Khách VIP mẫu"))
                    db.Customers.Add(new Customer { Name = "NV3 - Khách VIP mẫu", IsVip = true });
                var category = db.Categories.SingleOrDefault(x => x.Name == "NV3 - Hàng mẫu");
                if (category == null) { category = new Category { Name = "NV3 - Hàng mẫu" }; db.Categories.Add(category); }
                if (!db.Products.Any(x => x.Barcode == "NV3-001"))
                    db.Products.Add(new Product { Name = "NV3 - Sữa hộp", Barcode = "NV3-001", Price = 20000m, Stock = 0, Category = category });
                if (!db.Products.Any(x => x.Barcode == "NV3-002"))
                    db.Products.Add(new Product { Name = "NV3 - Bánh quy", Barcode = "NV3-002", Price = 15000m, Stock = 0, Category = category });
                db.SaveChanges(); tx.Commit();
            }
        }
    }
}
