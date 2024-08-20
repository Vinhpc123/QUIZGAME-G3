
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace P2PHelper
{
    public interface ICommunicationChannel
    {
        /// <summary>
        /// An event handler that indicates a message was received.
        /// </summary>
        event EventHandler<IMessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Sends a serialized object to the remote.
        /// </summary>
        Task SendRemoteMessageAsync(object message);

        /// <summary>
        /// Starts listening for messages.
        /// </summary>
        Task<bool> StartListeningAsync();

        /// <summary>
        /// Stops listening for messages.
        /// </summary>
        Task<bool> StopListening();
    }

    public interface IMessageReceivedEventArgs
    {
        void GetDeserializedMessage(ref object message);
    }
}
