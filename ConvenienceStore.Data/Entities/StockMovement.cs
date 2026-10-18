using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ConvenienceStore.Data.Entities
{
    public class StockMovement
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        [Required, MaxLength(100)] public string ProductName { get; set; }
        public StockMovementKind Kind { get; set; }
        // Quantity có dấu: nhập dương, bán âm.
        public int Quantity { get; set; }
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }
        [Required, MaxLength(40)] public string ReferenceNumber { get; set; }
        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }
        public int? PurchaseReceiptId { get; set; }
        public virtual PurchaseReceipt PurchaseReceipt { get; set; }
        public int? SaleInvoiceId { get; set; }
        public virtual SaleInvoice SaleInvoice { get; set; }
    }
}
