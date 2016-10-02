using System;
using EventSourcing.Library.Model;

namespace EventSourcing.Accounting.Model
{
    public class AccountId : IIdentity
    {
        public readonly string Id;

        public AccountId(Guid id)
        {
            Id = id.ToString();
        }

        public override string ToString()
        {
            return string.Format("account-{0}", Id);
        }
    }
}