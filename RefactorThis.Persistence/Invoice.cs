using System.Collections.Generic;
using System.Linq;

namespace RefactorThis.Persistence
{
    public class Invoice
    {
        public Invoice() { }

        public Invoice(decimal amount, decimal amountPaid, params Payment[] payments)
        {
            Amount = amount;
            AmountPaid = amountPaid;

            if (payments.Any())
                Payments = new List<Payment>(payments);
        }

        public InvoiceType Type { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Payment> Payments { get; set; }
    }

    public enum InvoiceType
    {
        Standard,
        Commercial
    }
}