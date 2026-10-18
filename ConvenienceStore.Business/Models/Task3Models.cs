using System;
using System.Collections.Generic;
namespace ConvenienceStore.Business.Models
{
    public enum Task3Action { Purchase, Sale, Stock, Revenue }
    public class Task3Exception : Exception { public Task3Exception(string message) : base(message) { } }
    public class ProductOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string Display { get { return Name + " | " + Barcode + " | Tồn: " + Stock; } }
    }
    public class PartyOption
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsVip { get; set; }
        public string Display { get { return Name + (IsVip ? " (VIP -10%)" : ""); } }
    }
    public class LookupData
    {
        public List<ProductOption> Products { get; set; }
        public List<PartyOption> Suppliers { get; set; }
        public List<PartyOption> Customers { get; set; }
    }
    public class CartLine
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get { return Pricing.Money(Quantity * UnitPrice); } }
    }
    public class SaleRequest
    {
        public Guid OperationId { get; set; }
        public int? CustomerId { get; set; }
        public List<CartLine> Lines { get; set; }
        public decimal ExpectedTotal { get; set; }
        public decimal CashReceived { get; set; }
    }
    public class PurchaseRequest
    {
        public Guid OperationId { get; set; }
        public int SupplierId { get; set; }
        public List<CartLine> Lines { get; set; }
        public string Note { get; set; }
    }
    public class DocumentResult
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public decimal Total { get; set; }
        public decimal Change { get; set; }
        public bool AlreadySaved { get; set; }
    }
    public class InvoiceReportRow
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CustomerName { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
    }
    public class DailyRevenueRow
    {
        public DateTime Date { get; set; }
        public int InvoiceCount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Revenue { get; set; }
    }
    public class RevenueData
    {
        public List<InvoiceReportRow> Invoices { get; set; }
        public List<DailyRevenueRow> Days { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal Revenue { get; set; }
    }
    public class MovementRow
    {
        public DateTime CreatedAt { get; set; }
        public string ReferenceNumber { get; set; }
        public string ProductName { get; set; }
        public string Kind { get; set; }
        public int Quantity { get; set; }
        public int StockBefore { get; set; }
        public int StockAfter { get; set; }
    }
    public class StockRow
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Opening { get; set; }
        public int Purchased { get; set; }
        public int Sold { get; set; }
        public int Closing { get; set; }
        public int Current { get; set; }
    }
    public class StockData
    {
        public List<MovementRow> Movements { get; set; }
        public List<StockRow> Balances { get; set; }
    }
    public static class Pricing
    {
        public const decimal VipPercent = 10m;
        public static decimal Money(decimal value)
        {
            if (value < 0m || value > 999999999999.99m)
                throw new Task3Exception("Giá trị tiền vượt phạm vi cho phép.");
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }
        public static decimal Discount(decimal subtotal, bool vip)
        { return Money(subtotal * (vip ? VipPercent : 0m) / 100m); }
    }
}
