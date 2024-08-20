
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PHelper
{
    public abstract class SessionManager : ISessionManager
    {
        public Dictionary<Guid, object> Participants { get; set; } = new Dictionary<Guid, object>();

        public event EventHandler<ParticipantConnectedEventArgs> ParticipantConnected = delegate { };

        public abstract Task<bool> StartAdvertisingAsync();

        public abstract bool StopAdvertising();

        public abstract ICommunicationChannel CreateCommunicationChannel(Guid participant, int flags);

        public bool RemoveParticipant(Guid subscriber) => Participants.Remove(subscriber);

        /// <summary>
        /// Adds a participant.
        /// </summary>
        protected void AddParticipant(object participant, string participantMessage)
	    {                
            // Add the participant if it isn't already in the list of Participants.
		    if (!Participants.Values.Contains(participant))
            {
                // Generate a new GUID so that app developers can reference the participant.
                var guid = Guid.NewGuid();
                Participants.Add(guid, participant);

                // Notify that ParticipantConnected event handlers so app developers are aware when a new participant has been connected.
                ParticipantConnected(this, new ParticipantConnectedEventArgs { Id = guid, Message = participantMessage});
            }
	    }

    }
}
