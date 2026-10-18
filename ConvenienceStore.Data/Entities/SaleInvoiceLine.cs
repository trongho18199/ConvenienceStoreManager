using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ConvenienceStore.Data.Entities
{
    public class SaleInvoiceLine
    {
        public int Id { get; set; }
        public int SaleInvoiceId { get; set; }
        public virtual SaleInvoice SaleInvoice { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        [Required, MaxLength(100)] public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
