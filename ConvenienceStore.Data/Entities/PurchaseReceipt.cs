using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ConvenienceStore.Data.Entities
{
    public class PurchaseReceipt
    {
        public int Id { get; set; }
        [Index(IsUnique = true)] public Guid OperationId { get; set; }
        [Required, MaxLength(40), Index(IsUnique = true)] public string Number { get; set; }
        public DateTime CreatedAt { get; set; }
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }
        [Required, MaxLength(100)] public string SupplierName { get; set; }
        [MaxLength(300)] public string Note { get; set; }
        public decimal Total { get; set; }
        public virtual ICollection<PurchaseReceiptLine> Lines { get; set; } = new List<PurchaseReceiptLine>();
    }
}
