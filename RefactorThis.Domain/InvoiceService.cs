using System;
using System.Collections.Generic;
using System.Linq;
using RefactorThis.Persistence;

namespace RefactorThis.Domain
{
	public class InvoiceService
	{
		private readonly InvoiceRepository _invoiceRepository;

		public InvoiceService( InvoiceRepository invoiceRepository )
		{
			_invoiceRepository = invoiceRepository;
		}

		public string ProcessPayment( Payment payment )
		{
			var inv = _invoiceRepository.GetInvoice( payment.Reference );

			string responseMessage;

            if (inv == null)
            {
                throw new InvalidOperationException( "There is no invoice matching this payment" );
            }

            // If the amount is 0, no payment is required
            if ( inv.Amount == 0 )
            {
                if (inv.Payments != null && inv.Payments.Any())
                {
                    throw new InvalidOperationException("The invoice is in an invalid state, it has an amount of 0 and it has payments.");
                }

                responseMessage = "no payment needed";
            }
            else
            {
                // If there's existing payments...
                if ( inv.Payments != null && inv.Payments.Any( ) )
                {
                    if ( inv.Payments.Sum( x => x.Amount ) != 0 && inv.Amount == inv.Payments.Sum( x => x.Amount ) )
                    {
                        responseMessage = "invoice was already fully paid";
                    }
                    else if ( inv.Payments.Sum( x => x.Amount ) != 0 && payment.Amount > inv.Amount - inv.AmountPaid )
                    {
                        responseMessage = "the payment is greater than the partial amount remaining";
                    }
                    else
                    {
                        // Is this a final payment?
                        if ( inv.Amount - inv.AmountPaid == payment.Amount )
                        {
                            switch ( inv.Type )
                            {
                                case InvoiceType.Standard:
                                    inv.AmountPaid += payment.Amount;
                                    inv.Payments.Add( payment );
                                    responseMessage = "final partial payment received, invoice is now fully paid";
                                    break;
                                case InvoiceType.Commercial:
                                    inv.AmountPaid += payment.Amount;
                                    inv.TaxAmount += payment.Amount * 0.14m; // ToDo: what's with this magic number?
                                    inv.Payments.Add( payment );
                                    responseMessage = "final partial payment received, invoice is now fully paid";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException( );
                            }
                        }
                        else // This is a partial payment
                        {
                            switch ( inv.Type )
                            {
                                case InvoiceType.Standard:
                                    inv.AmountPaid += payment.Amount;
                                    inv.Payments.Add( payment );
                                    responseMessage = "another partial payment received, still not fully paid";
                                    break;
                                case InvoiceType.Commercial:
                                    inv.AmountPaid += payment.Amount;
                                    inv.TaxAmount += payment.Amount * 0.14m;
                                    inv.Payments.Add( payment );
                                    responseMessage = "another partial payment received, still not fully paid";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException( );
                            }
                        }
                    }
                }
                else // There's no existing payments
                {
                    if ( payment.Amount > inv.Amount )
                    {
                        responseMessage = "the payment is greater than the invoice amount";
                    }
                    else
                    {
                        // Ensure Payments is initialized to prevent NullReferenceExceptions
                        if (inv.Payments == null)
                        {
                            inv.Payments = new List<Payment>();
                        }

                        // Is this a full payment?
                        if (inv.Amount == payment.Amount)
                        {
                            switch (inv.Type)
                            {
                                case InvoiceType.Standard:
                                    inv.AmountPaid = payment.Amount;
                                    inv.TaxAmount = payment.Amount * 0.14m;
                                    inv.Payments.Add(payment);
                                    responseMessage = "invoice is now fully paid";
                                    break;
                                case InvoiceType.Commercial:
                                    inv.AmountPaid = payment.Amount;
                                    inv.TaxAmount = payment.Amount * 0.14m;
                                    inv.Payments.Add(payment);
                                    responseMessage = "invoice is now fully paid";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                        else // It's a partial payment
                        {
                            switch (inv.Type)
                            {
                                case InvoiceType.Standard:
                                    inv.AmountPaid = payment.Amount;
                                    inv.TaxAmount = payment.Amount * 0.14m;
                                    inv.Payments.Add(payment);
                                    responseMessage = "invoice is now partially paid";
                                    break;
                                case InvoiceType.Commercial:
                                    inv.AmountPaid = payment.Amount;
                                    inv.TaxAmount = payment.Amount * 0.14m;
                                    inv.Payments.Add(payment);
                                    responseMessage = "invoice is now partially paid";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }
            }

            inv.Save();

			return responseMessage;
		}
	}
}