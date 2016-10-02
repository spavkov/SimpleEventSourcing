using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventSourcing.Library.Model;
using EventSourcing.Library.Persistence.Exceptions;
using Newtonsoft.Json;

namespace EventSourcing.Library.Persistence
{
    public class EventStore : IEventStore
    {
        private readonly IAppendOnlyStore appendOnlyStore;

        public EventStore(IAppendOnlyStore appendOnlyStore)
        {
            this.appendOnlyStore = appendOnlyStore;
        }

        public EventStream LoadEventStream(IIdentity id)
        {
            return LoadEventStream(id, 0, int.MaxValue);
        }

        public EventStream LoadEventStream(IIdentity id, long skip, int take)
        {
            var name = IdentityToString(id);
            var records = appendOnlyStore.ReadRecords(name, skip, take).ToList();
            var stream = new EventStream();

            foreach (var tapeRecord in records)
            {
                stream.Events.AddRange(DeserializeEvent(tapeRecord.Data));
                stream.Version = tapeRecord.Version;
            }
            return stream;
        }

        private string IdentityToString(IIdentity id)
        {
            return id.ToString();
        }

        public void AppendToStream(IIdentity id, int originalVersion, ICollection<IEvent> events)
        {
            if (events.Count == 0)
                return;
            var name = IdentityToString(id);
            var data = SerializeEvent(events.ToArray());
            try
            {
                appendOnlyStore.Append(name, data, originalVersion);
            }
            catch (AppendOnlyStoreConcurrencyException e)
            {
                // load server events
                var server = LoadEventStream(id, 0, int.MaxValue);
                // throw a real problem
                throw OptimisticConcurrencyException.Create(server.Version, e.ExpectedStreamVersion, id, server.Events);
            }

            // technically there should be a parallel process that queries new changes 
            // from the event store and sends them via messages (avoiding 2PC problem). 
            // however, for demo purposes, we'll just send them to the console from here
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            foreach (var @event in events)
            {
                Console.WriteLine("  {0} r{1} Event: {2}", id, originalVersion, @event);
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        byte[] SerializeEvent(IEvent[] e)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            };
            var resourceJSon = JsonConvert.SerializeObject(e, settings);
            return Encoding.UTF8.GetBytes(resourceJSon);
        }

        IEvent[] DeserializeEvent(byte[] data)
        {
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            };
            var json = Encoding.UTF8.GetString(data);
            var events = JsonConvert.DeserializeObject<IEvent[]>(json, settings);
            return events;
        }
    }
}