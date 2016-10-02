using EventSourcing.Accounting.Model;
using EventSourcing.Library.Model;

namespace EventSourcing.Accounting.Commands
{
    public class WithdrawFromAccountCommand : ICommand
    {
        public WithdrawFromAccountCommand(AccountId id, decimal amount)
        {
            Id = id;
            Amount = amount;
        }

        public AccountId Id { get; private set; }

        public decimal Amount { get; private set; }
    }
}