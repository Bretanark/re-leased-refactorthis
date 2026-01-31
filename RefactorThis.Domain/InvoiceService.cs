using System;
using System.Collections.Generic;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
    public class InvoiceService
    {
        private const decimal TaxRate = 0.14m;
        private readonly InvoiceRepository _invoiceRepository;

        public InvoiceService( InvoiceRepository invoiceRepository )
        {
            _invoiceRepository = invoiceRepository;
        }

        public string ProcessPayment( Payment payment )
        {
            var inv = _invoiceRepository.GetInvoice( payment.Reference );
            var responseMessage = ProcessPayment(inv, payment);
            _invoiceRepository.SaveInvoice(inv);
            return responseMessage;
        }

        private static string ProcessPayment(Invoice inv, Payment payment)
        {
            if (inv == null)
                throw new InvalidOperationException( "There is no invoice matching this payment" );

            // Validate invoice type
            switch (inv.Type)
            {
                case InvoiceType.Standard:
                case InvoiceType.Commercial: break;
                default: throw new ArgumentOutOfRangeException();
            }

            // If the amount is 0, no payment is required
            if ( inv.Amount == 0 )
            {
                if (inv.Payments != null && inv.Payments.Any())
                    throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");

                return "no payment needed";
            }

            // If there's existing payments...
            if ( inv.Payments != null && inv.Payments.Any( ) )
            {
                var previousSum = inv.Payments.Sum( x => x.Amount );
                if ( previousSum != 0 && inv.Amount == previousSum )
                    return "invoice was already fully paid";

                var previousAmountRemaining = inv.Amount - inv.AmountPaid;
                if ( previousSum != 0 && payment.Amount > previousAmountRemaining )
                    return "the payment is greater than the partial amount remaining";

                inv.AmountPaid += payment.Amount;
                inv.Payments.Add(payment);
                if (inv.Type == InvoiceType.Commercial)
                    inv.TaxAmount += payment.Amount * TaxRate; // ToDo: not covered by tests

                return previousAmountRemaining == payment.Amount
                    ? "final partial payment received, invoice is now fully paid"
                    : "another partial payment received, still not fully paid";
            }

            // There's no existing payments
            if ( payment.Amount > inv.Amount )
                return "the payment is greater than the invoice amount";

            // Ensure Payments is initialized to prevent NullReferenceExceptions
            if (inv.Payments == null) inv.Payments = new List<Payment>();

            inv.AmountPaid = payment.Amount;
            inv.TaxAmount = payment.Amount * TaxRate;
            inv.Payments.Add(payment);

            return inv.Amount == payment.Amount
                ? "invoice is now fully paid" // ToDo: not covered by tests
                : "invoice is now partially paid";
        }
    }
}