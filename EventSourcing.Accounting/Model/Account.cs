using System;
using System.Collections.Generic;
using EventSourcing.Accounting.Events;
using EventSourcing.Library.Model;

namespace EventSourcing.Accounting.Model
{
    public class Account
    {
        public readonly IList<IEvent> Changes = new List<IEvent>();

        private readonly AccountState state;

        public Account(List<IEvent> events)
        {
            state = new AccountState(events);
        }

        public void Create(AccountId id)
        {
            if (state.Created)
            {
                throw new InvalidOperationException(string.Format("Customer {0} was already created", id));
            }

            Apply(new AccountCreatedEvent(id));
        }

        public void IncreaseAmount(AccountId id, decimal amount)
        {
            Apply(new AccountReceivedPaymentEvent(id, amount));
        }

        public void DecreaseAmount(AccountId id, decimal amount)
        {
            Apply(new AccountWithdrawalOccuredEvent(id, amount));
        }

        private void Apply(IEvent @event)
        {
            state.Mutate((dynamic)@event);

            Changes.Add(@event);
        }
    }
}