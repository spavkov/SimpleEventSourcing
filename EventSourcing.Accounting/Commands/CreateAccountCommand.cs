using System;
using EventSourcing.Accounting.Model;
using EventSourcing.Library.Model;

namespace EventSourcing.Accounting.Commands
{
    public class CreateAccountCommand : ICommand
    {
        public CreateAccountCommand(AccountId id)
        {
            this.Id = id;
        }

        public AccountId Id { get; private set; }
    }
}