using EventSourcing.Accounting.Model;
using EventSourcing.Library.Model;

namespace EventSourcing.Accounting.Events
{
    public class AccountCreatedEvent : IEvent
    {
        public AccountCreatedEvent(AccountId id)
        {
            Id = id;
        }

        public AccountId Id { get; private set; }
    }
}