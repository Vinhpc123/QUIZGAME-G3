
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PHelper
{
    public interface ISessionManager
    {
        /// <summary>
        /// An event that indicates that a participant has been connected.
        /// </summary>
        event EventHandler<ParticipantConnectedEventArgs> ParticipantConnected;

        /// <summary>
        /// Start advertising. 
        /// </summary>
        Task<bool> StartAdvertisingAsync();

        /// <summary>
        /// Stop advertising.
        /// </summary>
        bool StopAdvertising();

        /// <summary>
        /// Creates an ICommunicationChannel object and returns it so that app developers can send custom messages to the participant.
        /// </summary>
        ICommunicationChannel CreateCommunicationChannel(Guid participant, int flags = 0);

        /// <summary>
        /// Removes a participant from a participants list.
        /// </summary>
        bool RemoveParticipant(Guid participant);
    }

    public class ParticipantConnectedEventArgs : EventArgs
    {
        public Guid Id { get; set; }

        public object Message { get; set; }
    }
}
    