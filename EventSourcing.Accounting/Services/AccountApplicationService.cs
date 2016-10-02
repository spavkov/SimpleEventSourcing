using System;
using EventSourcing.Accounting.Commands;
using EventSourcing.Accounting.Model;
using EventSourcing.Library.Model;
using EventSourcing.Library.Persistence;
using EventSourcing.Library.Persistence.Exceptions;

namespace EventSourcing.Accounting.Services
{
    public class AccountApplicationService
    {
        private readonly IEventStore eventStore;

        public AccountApplicationService(IEventStore eventStore)
        {
            this.eventStore = eventStore;
        }

        public void Execute(ICommand cmd)
        {
            When((dynamic)cmd);
        }

        private void When(PayToAccountCommand c)
        {
            Update(c.Id, a => a.IncreaseAmount(c.Id, c.Amount));
        }

        private void When(WithdrawFromAccountCommand c)
        {
            Update(c.Id, a => a.DecreaseAmount(c.Id, c.Amount));
        }

        private void When(CreateAccountCommand cmd)
        {
            Update(cmd.Id, c => c.Create(cmd.Id));
        }

        private void Update(AccountId id, Action<Account> action)
        {
            // Load event stream from the store
            EventStream stream = eventStore.LoadEventStream(id);
            // create new Customer aggregate from the history
            Account account = new Account(stream.Events);
            // execute delegated action
            action(account);
            // append resulting changes to the stream
            eventStore.AppendToStream(id, stream.Version, account.Changes);
        }

        void UpdateWithSimpleConflictResolution(AccountId id, Action<Account> action)
        {
            while (true)
            {
                var eventStream = eventStore.LoadEventStream(id);
                Account customer = new Account(eventStream.Events);
                action(customer);

                try
                {
                    eventStore.AppendToStream(id, eventStream.Version, customer.Changes);
                }
                catch (OptimisticConcurrencyException ex)
                {
                    foreach (var clientEvent in customer.Changes)
                    {
                        foreach (var actualEvent in ex.ActualEvents)
                        {
                            if (ConflictsWith(clientEvent, actualEvent))
                            {
                                var msg = string.Format("Conflict between {0} and {1}", clientEvent, actualEvent);
                                throw new RealConcurrencyException(msg, ex);
                            }
                        }
                    }
                    // there are no conflicts and we can append
                    eventStore.AppendToStream(id, ex.ActualVersion, customer.Changes);
                }
            }
        }


        static bool ConflictsWith(IEvent x, IEvent y)
        {
            return x.GetType() == y.GetType();
        }
    }
}
