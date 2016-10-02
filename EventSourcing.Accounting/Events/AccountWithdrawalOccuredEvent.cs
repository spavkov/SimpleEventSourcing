using EventSourcing.Accounting.Model;
using EventSourcing.Library.Model;

namespace EventSourcing.Accounting.Events
{
    public class AccountWithdrawalOccuredEvent : IEvent
    {
        public AccountWithdrawalOccuredEvent(AccountId id, decimal amount)
        {
            Id = id;
            Amount = amount;
        }

        public AccountId Id { get; private set; }
        public decimal Amount { get; private set; }
    }
}