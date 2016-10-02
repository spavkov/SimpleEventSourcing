using EventSourcing.Accounting.Model;
using EventSourcing.Library.Model;

namespace EventSourcing.Accounting.Commands
{
    public class PayToAccountCommand : ICommand
    {
        public PayToAccountCommand(AccountId id, decimal amount)
        {
            Id = id;
            Amount = amount;
        }

        public AccountId Id { get; private set; }

        public decimal Amount { get; private set; }
    }
}