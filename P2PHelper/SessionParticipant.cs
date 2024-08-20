
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P2PHelper
{
    public abstract class SessionParticipant : ISessionParticipant
    {
        /// <summary>
        /// The managers that are available.
        /// </summary>
        public ConcurrentDictionary<Guid, object> Managers { get; set; } = new ConcurrentDictionary<Guid, object>();

        public event EventHandler<ManagerFoundEventArgs> ManagerFound = delegate { };

        public abstract Task<bool> StartListeningAsync();

        public abstract bool StopListening();

        public abstract Task ConnectToManagerAsync(Guid manager);

        public abstract ICommunicationChannel CreateCommunicationChannel(Guid manager, int flags);

        public bool RemoveManager(Guid manager)
        {
            object obj;
            return Managers.TryRemove(manager, out obj);
        }

        /// <summary>
        /// Adds a manager to the AvailableManagers list.
        /// </summary>
        protected void AddManager(object manager, string managerMessage)
        {
            // Add the manager to the list of Managers if it's not already in the list.
            if (!Managers.Values.Contains(manager))
            {
                // Generate a new GUID, so that app developers can reference this particular manager.
                var guid = Guid.NewGuid();
                bool isAdded = Managers.TryAdd(guid, manager);

                if (isAdded)
                {
                    // Notify that ManagerFound handlers so the app developers are aware of a new Manager.
                    ManagerFound(this, new ManagerFoundEventArgs { Id = guid, Message = managerMessage });
                }
            }
        }
    }
}
