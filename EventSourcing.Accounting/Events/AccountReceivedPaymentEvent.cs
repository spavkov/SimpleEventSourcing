using EventSourcing.Accounting.Model;
using EventSourcing.Library.Model;

namespace EventSourcing.Accounting.Events
{
    public class AccountReceivedPaymentEvent : IEvent
    {
        public AccountReceivedPaymentEvent(AccountId id, decimal amount)
        {
            this.Id = id;
            this.Amount = amount;
        }

        public AccountId Id { get; private set; }
        public decimal Amount { get; private set; } 
    }
}