using System.Collections.Generic;
using EventSourcing.Accounting.Commands;
using EventSourcing.Accounting.Events;
using EventSourcing.Library.Model;

namespace EventSourcing.Accounting.Model
{
    public class AccountState
    {
        public AccountId Id { get; private set; }

        public decimal Balance { get; private set; }

        public bool Created { get; private set; }

        public AccountState(List<IEvent> events)
        {
            foreach (var @event in events)
            {
                this.Mutate((dynamic) @event);
            }
        }

        public void Mutate(IEvent @event)
        {
            Mutate((dynamic)@event);
        }

        private void Mutate(AccountCreatedEvent e)
        {
            this.Id = e.Id;
            this.Created = true;
        }

        private void Mutate(AccountReceivedPaymentEvent e)
        {
            this.Balance += e.Amount;
        }

        private void Mutate(AccountWithdrawalOccuredEvent e)
        {
            this.Balance -= e.Amount;
        }
    }
}