namespace RefactorThis.Persistence
{
    public class Payment
    {
        public Payment() {}

        public Payment(decimal amount, string reference = null)
        {
            Amount = amount;
            Reference = reference;
        }

        public decimal Amount { get; set; }
        public string Reference { get; set; }
    }
}