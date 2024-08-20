
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PHelper
{
    public interface ISessionParticipant
    {
        /// <summary>
        /// An event indicating that a manager has been found.
        /// </summary>
        event EventHandler<ManagerFoundEventArgs> ManagerFound;

        /// <summary>
        /// Start listening for managers.
        /// </summary>
        Task<bool> StartListeningAsync();

        /// <summary>
        /// Stop listening for managers.
        /// </summary>
        bool StopListening();

        /// <summary>
        /// Connect to a manager that has already been found.
        /// </summary>
        Task ConnectToManagerAsync(Guid manager);

        /// <summary>
        /// Creates and returns an ICommunicationChannel object so that app developers can have direct communication with the manager and send custom messages.
        /// </summary>
        ICommunicationChannel CreateCommunicationChannel(Guid manager, int flags = 0);
        
        /// <summary>
        /// Removes a manager from the list of available managers.
        /// </summary>
        bool RemoveManager(Guid manager);
    }

    public class ManagerFoundEventArgs
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
    }
}
