using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventSourcing.Accounting.Commands;
using EventSourcing.Accounting.Model;
using EventSourcing.Accounting.Services;
using EventSourcing.Library.Persistence;
using EventSourcing.Persistence.SqlServerCompact;

namespace WriteSide.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var appendOnlyStore = new SqlServerCompactAppendOnlyStore();
            appendOnlyStore.Initialize(new Dictionary<string, string>()
            {
                { "DatabasePath", Environment.GetFolderPath(Environment.SpecialFolder.Personal) },
                { "DatabaseName", "EventStore" }
            });

            var eventStore = new EventStore(appendOnlyStore);

            var accountAppService = new AccountApplicationService(eventStore);

            var accountId = new AccountId(Guid.NewGuid());
            accountAppService.Execute(new CreateAccountCommand(accountId));
            accountAppService.Execute(new PayToAccountCommand(accountId, new decimal(100.0)));
            accountAppService.Execute(new PayToAccountCommand(accountId, new decimal(50.0)));
            accountAppService.Execute(new WithdrawFromAccountCommand(accountId, new decimal(50.0)));
        }
    }
}
