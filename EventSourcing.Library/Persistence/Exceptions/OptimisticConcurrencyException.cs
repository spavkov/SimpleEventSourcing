using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using EventSourcing.Library.Model;

namespace EventSourcing.Library.Persistence.Exceptions
{
    /// <summary>
    /// Is thrown by event store if there were changes since our last version
    /// </summary>
    [Serializable]
    public class OptimisticConcurrencyException : Exception
    {
        public int ActualVersion { get; private set; }
        public int ExpectedVersion { get; private set; }
        public IIdentity Id { get; private set; }
        public IList<IEvent> ActualEvents { get; private set; }

        OptimisticConcurrencyException(string message, int actualVersion, int expectedVersion, IIdentity id,
            IList<IEvent> serverEvents)
            : base(message)
        {
            ActualVersion = actualVersion;
            ExpectedVersion = expectedVersion;
            Id = id;
            ActualEvents = serverEvents;
        }

        public static OptimisticConcurrencyException Create(int actual, int expected, IIdentity id,
            IList<IEvent> serverEvents)
        {
            var message = string.Format("Expected v{0} but found v{1} in stream '{2}'", expected, actual, id);
            return new OptimisticConcurrencyException(message, actual, expected, id, serverEvents);
        }

        protected OptimisticConcurrencyException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context) { }
    }
}