namespace ConvenienceStore.Data.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using ConvenienceStore.Data.Entities;
    using ConvenienceStore.Data.Helpers;

    internal sealed class Configuration : DbMigrationsConfiguration<ConvenienceStore.Data.CSDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(ConvenienceStore.Data.CSDbContext context)
        {
            context.Roles.AddOrUpdate(r => r.Name,
                new Role { Name = "Admin" },
                new Role { Name = "ThuKho" },
                new Role { Name = "ThuNgan" });
            context.SaveChanges();

            var adminRole = context.Roles.First(r => r.Name == "Admin");

            context.Users.AddOrUpdate(u => u.Username,
                new User
                {
                    Username = "admin",
                    PasswordHash = PasswordHasher.Hash("Admin@123"),
                    FullName = "Quản trị viên",
                    IsActive = true,
                    RoleId = adminRole.Id
                });
            context.SaveChanges();
        }
    }
}