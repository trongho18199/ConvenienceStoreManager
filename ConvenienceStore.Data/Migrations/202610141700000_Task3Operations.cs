namespace ConvenienceStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Task3Operations : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Customers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Phone = c.String(maxLength: 30),
                        IsVip = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.PurchaseReceipts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OperationId = c.Guid(nullable: false),
                        Number = c.String(nullable: false, maxLength: 40),
                        CreatedAt = c.DateTime(nullable: false),
                        SupplierId = c.Int(nullable: false),
                        CreatedById = c.Int(nullable: false),
                        SupplierName = c.String(nullable: false, maxLength: 100),
                        Note = c.String(maxLength: 300),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CreatedById)
                .ForeignKey("dbo.Suppliers", t => t.SupplierId)
                .Index(t => t.OperationId, unique: true)
                .Index(t => t.Number, unique: true)
                .Index(t => t.SupplierId)
                .Index(t => t.CreatedById);

            CreateTable(
                "dbo.PurchaseReceiptLines",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PurchaseReceiptId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        ProductName = c.String(nullable: false, maxLength: 100),
                        Quantity = c.Int(nullable: false),
                        UnitCost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LineTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .ForeignKey("dbo.PurchaseReceipts", t => t.PurchaseReceiptId)
                .Index(t => t.PurchaseReceiptId)
                .Index(t => t.ProductId);

            CreateTable(
                "dbo.Suppliers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Phone = c.String(maxLength: 30),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.SaleInvoices",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OperationId = c.Guid(nullable: false),
                        Number = c.String(nullable: false, maxLength: 40),
                        CreatedAt = c.DateTime(nullable: false),
                        CustomerId = c.Int(),
                        CustomerName = c.String(nullable: false, maxLength: 100),
                        CreatedById = c.Int(nullable: false),
                        Subtotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountPercent = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Total = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CashReceived = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Change = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CreatedById)
                .ForeignKey("dbo.Customers", t => t.CustomerId)
                .Index(t => t.OperationId, unique: true)
                .Index(t => t.Number, unique: true)
                .Index(t => t.CustomerId)
                .Index(t => t.CreatedById);

            CreateTable(
                "dbo.SaleInvoiceLines",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SaleInvoiceId = c.Int(nullable: false),
                        ProductId = c.Int(nullable: false),
                        ProductName = c.String(nullable: false, maxLength: 100),
                        Quantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LineTotal = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .ForeignKey("dbo.SaleInvoices", t => t.SaleInvoiceId)
                .Index(t => t.SaleInvoiceId)
                .Index(t => t.ProductId);

            CreateTable(
                "dbo.StockMovements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CreatedAt = c.DateTime(nullable: false),
                        ProductId = c.Int(nullable: false),
                        ProductName = c.String(nullable: false, maxLength: 100),
                        Kind = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        StockBefore = c.Int(nullable: false),
                        StockAfter = c.Int(nullable: false),
                        ReferenceNumber = c.String(nullable: false, maxLength: 40),
                        CreatedById = c.Int(nullable: false),
                        PurchaseReceiptId = c.Int(),
                        SaleInvoiceId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.CreatedById)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .ForeignKey("dbo.PurchaseReceipts", t => t.PurchaseReceiptId)
                .ForeignKey("dbo.SaleInvoices", t => t.SaleInvoiceId)
                .Index(t => t.ProductId)
                .Index(t => t.CreatedById)
                .Index(t => t.PurchaseReceiptId)
                .Index(t => t.SaleInvoiceId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.StockMovements", "SaleInvoiceId", "dbo.SaleInvoices");
            DropForeignKey("dbo.StockMovements", "PurchaseReceiptId", "dbo.PurchaseReceipts");
            DropForeignKey("dbo.StockMovements", "ProductId", "dbo.Products");
            DropForeignKey("dbo.StockMovements", "CreatedById", "dbo.Users");
            DropForeignKey("dbo.SaleInvoiceLines", "SaleInvoiceId", "dbo.SaleInvoices");
            DropForeignKey("dbo.SaleInvoiceLines", "ProductId", "dbo.Products");
            DropForeignKey("dbo.SaleInvoices", "CustomerId", "dbo.Customers");
            DropForeignKey("dbo.SaleInvoices", "CreatedById", "dbo.Users");
            DropForeignKey("dbo.PurchaseReceipts", "SupplierId", "dbo.Suppliers");
            DropForeignKey("dbo.PurchaseReceiptLines", "PurchaseReceiptId", "dbo.PurchaseReceipts");
            DropForeignKey("dbo.PurchaseReceiptLines", "ProductId", "dbo.Products");
            DropForeignKey("dbo.PurchaseReceipts", "CreatedById", "dbo.Users");
            DropIndex("dbo.StockMovements", new[] { "SaleInvoiceId" });
            DropIndex("dbo.StockMovements", new[] { "PurchaseReceiptId" });
            DropIndex("dbo.StockMovements", new[] { "CreatedById" });
            DropIndex("dbo.StockMovements", new[] { "ProductId" });
            DropIndex("dbo.SaleInvoiceLines", new[] { "ProductId" });
            DropIndex("dbo.SaleInvoiceLines", new[] { "SaleInvoiceId" });
            DropIndex("dbo.SaleInvoices", new[] { "CreatedById" });
            DropIndex("dbo.SaleInvoices", new[] { "CustomerId" });
            DropIndex("dbo.SaleInvoices", new[] { "Number" });
            DropIndex("dbo.SaleInvoices", new[] { "OperationId" });
            DropIndex("dbo.PurchaseReceiptLines", new[] { "ProductId" });
            DropIndex("dbo.PurchaseReceiptLines", new[] { "PurchaseReceiptId" });
            DropIndex("dbo.PurchaseReceipts", new[] { "CreatedById" });
            DropIndex("dbo.PurchaseReceipts", new[] { "SupplierId" });
            DropIndex("dbo.PurchaseReceipts", new[] { "Number" });
            DropIndex("dbo.PurchaseReceipts", new[] { "OperationId" });
            DropTable("dbo.StockMovements");
            DropTable("dbo.SaleInvoiceLines");
            DropTable("dbo.SaleInvoices");
            DropTable("dbo.Suppliers");
            DropTable("dbo.PurchaseReceiptLines");
            DropTable("dbo.PurchaseReceipts");
            DropTable("dbo.Customers");
        }
    }
}
