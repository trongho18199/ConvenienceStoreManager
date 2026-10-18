using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ConvenienceStore.Data.Entities
{
    public class PurchaseReceiptLine
    {
        public int Id { get; set; }
        public int PurchaseReceiptId { get; set; }
        public virtual PurchaseReceipt PurchaseReceipt { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        [Required, MaxLength(100)] public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
    }
}
