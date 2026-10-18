using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ConvenienceStore.Data.Entities
{
    public class SaleInvoice
    {
        public int Id { get; set; }
        [Index(IsUnique = true)] public Guid OperationId { get; set; }
        [Required, MaxLength(40), Index(IsUnique = true)] public string Number { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? CustomerId { get; set; }
        public virtual Customer Customer { get; set; }
        [Required, MaxLength(100)] public string CustomerName { get; set; }
        public int CreatedById { get; set; }
        public virtual User CreatedBy { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
        public decimal CashReceived { get; set; }
        public decimal Change { get; set; }
        public virtual ICollection<SaleInvoiceLine> Lines { get; set; } = new List<SaleInvoiceLine>();
    }
}
