using System;
using System.Collections.Generic;
using NUnit.Framework;
using RefactorThis.Persistence;

namespace RefactorThis.Domain.Tests
{
    [TestFixture]
    public class InvoicePaymentProcessorTests
    {
        [Test]
        public void ProcessPayment_Should_ThrowException_When_NoInvoiceFoundForPaymentReference()
        {
            var repo = new InvoiceRepository();
            var paymentProcessor = new InvoiceService(repo);
            var payment = new Payment();

            var failureMessage = "";
            try
            {
                paymentProcessor.ProcessPayment(payment);
            }
            catch (InvalidOperationException e)
            {
                failureMessage = e.Message;
            }

            Assert.AreEqual("There is no invoice matching this payment", failureMessage);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPaymentNeeded()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(0, 0);
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment();
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("no payment needed", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_InvoiceAlreadyFullyPaid()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(10, 10, new Payment(10));
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment();
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_PartialPaymentExistsAndAmountPaidExceedsAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(10, 5, new Payment(5));
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment(6);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the partial amount remaining", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFailureMessage_When_NoPartialPaymentExistsAndAmountPaidExceedsInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(5, 0) { Payments = new List<Payment>() };
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment(6);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("the payment is greater than the invoice amount", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_PartialPaymentExistsAndAmountPaidEqualsAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(10, 5, new Payment(5));
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment(5);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("final partial payment received, invoice is now fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnFullyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidEqualsInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(10, 0, new Payment(10));
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment(10);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice was already fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_PartialPaymentExistsAndAmountPaidIsLessThanAmountDue()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(10, 5, new Payment(5));
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment(1);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("another partial payment received, still not fully paid", result);
        }

        [Test]
        public void ProcessPayment_Should_ReturnPartiallyPaidMessage_When_NoPartialPaymentExistsAndAmountPaidIsLessThanInvoiceAmount()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(10, 0) { Payments = new List<Payment>() };
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment(1);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice is now partially paid", result);
        }

        [Test]
        public void ProcessPayment_Should_InitializePaymentsList_When_Null()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(10, 0) { Payments = null }; // Note: explicitly setting Payments to null in case we default to not null
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment(1);
            var result = paymentProcessor.ProcessPayment(payment);

            Assert.AreEqual("invoice is now partially paid", result);
            Assert.IsNotNull(invoice.Payments);
            Assert.AreEqual(1, invoice.Payments.Count);
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_TypeIsInvalid()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(10, 0) { Type = (InvoiceType)999 };
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment();

            Assert.Throws<ArgumentOutOfRangeException>(() => paymentProcessor.ProcessPayment(payment));
        }

        [Test]
        public void ProcessPayment_Should_ThrowException_When_AmountIsZeroAndHasPayments()
        {
            var repo = new InvoiceRepository();
            var invoice = new Invoice(0, 0, new Payment());
            repo.Add(invoice);
            var paymentProcessor = new InvoiceService(repo);

            var payment = new Payment();

            Assert.Throws<InvalidOperationException>(() => paymentProcessor.ProcessPayment(payment));
        }

        // ToDo: Add tests for code coverage of commercial invoices.
        // ToDo: Use a code coverage tool to consider other cases to cover

    }
}