
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Xaml;

namespace P2PHelper
{
    public class TcpCommunicationChannel : ICommunicationChannel
    {
        /// <summary>
        /// The default port. 
        /// This port was chosen randomly in the ephemeral port range.
        /// </summary>
        private const string TCP_COMMUNICATION_PORT = "56789";

        /// <summary>
        /// The socket connection to the remote TCP server.
        /// </summary>
        private StreamSocket _remoteSocket;

        /// <summary>
        /// The local socket connection that will be listening for TCP messages.
        /// </summary>
        private StreamSocketListener _localSocket;

        /// <summary>
        /// The port number that the remote TCP server and local TCP server is listening to.
        /// </summary>
        public string CommunicationPort { get; set; } = TCP_COMMUNICATION_PORT;

        /// <summary>
        /// The hostname of the remote TCP server that is listeneing for TCP connections.
        /// </summary>
        public Windows.Networking.HostName RemoteHostname { get; set; }

        /// <summary>
        /// An event indicating that a message was received from the remote TCP server.
        /// </summary>
        public event EventHandler<IMessageReceivedEventArgs> MessageReceived = delegate { };

        /// <summary>
        /// Serializes the data object and sends it to the connected RemoteSocket.
        /// For more information on making an object serializable, see DataContractJsonSerializer.
        /// </summary>
        public async Task SendRemoteMessageAsync(object data)
        {
            // Connect to the remote host to ensure that the connection exists.
            await ConnectToRemoteAsync();

            using (var writer = new DataWriter(_remoteSocket.OutputStream))
            {
                byte[] serializedData = SerializeData(data);
                byte[] serializedDataLength = BitConverter.GetBytes(serializedData.Length);

                writer.WriteBytes(serializedDataLength);
                writer.WriteBytes(serializedData);

                await writer.StoreAsync();
                await writer.FlushAsync();
            }

            // Disconnect from the remote host.
            DisconnectFromRemote();
        }

        /// <summary>
        /// Creates a TCP socket and binds to the CommunicationPort.
        /// Use the default JsonSerializer if null is passed into the serializer parameter.
        /// Returns false if you are already listening.
        /// </summary>
        public async Task<bool> StartListeningAsync()
        {
            bool status = false;

            if (_localSocket == null)
            {
                _localSocket = new StreamSocketListener();
                _localSocket.ConnectionReceived += LocalSocketConnectionReceived;
                await _localSocket.BindServiceNameAsync(CommunicationPort);
                status = true;
            }

            return status;
        }

        /// <summary>
        /// Disposes of the TCP socket and sets it to null. Returns false if 
        /// you weren't listening.
        /// </summary>
        public async Task<bool> StopListening()
        {
            bool status = false;

            if (_localSocket != null)
            {
                await _localSocket.CancelIOAsync();
                _localSocket.ConnectionReceived -= LocalSocketConnectionReceived;
                _localSocket.Dispose();
                _localSocket = null;
                status = true;
            }

            return status;
        }

        /// <summary>
        /// The event handler for when a TCP connection has been received.
        /// </summary>
        private async void LocalSocketConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            using (var reader = new DataReader(args.Socket.InputStream))
            {
                reader.InputStreamOptions = InputStreamOptions.None;

                //Read the length of the payload that will be received.
                byte[] payloadSize = new byte[(uint)BitConverter.GetBytes(0).Length];
                await reader.LoadAsync((uint)payloadSize.Length);
                reader.ReadBytes(payloadSize);

                //Read the payload.
                int size = BitConverter.ToInt32(payloadSize, 0);
                byte[] payload = new byte[size];
                await reader.LoadAsync((uint)size);
                reader.ReadBytes(payload);

                // Notify subscribers that a message was received.
                MessageReceived(this, new TcpMessageReceivedEventArgs { Message = payload });
            }
        }

        /// <summary>
        /// Creates a RemoteSocket and establishes a connection to the RemoteHostname on CommunicationPort.
        /// </summary>
        private async Task ConnectToRemoteAsync()
        {
            _remoteSocket = _remoteSocket ?? new StreamSocket();
            await _remoteSocket.ConnectAsync(RemoteHostname, CommunicationPort);
        }

        /// <summary>
        /// Serializes an object by using DataContractJsonSerializer.
        /// </summary>
        private byte[] SerializeData(object data)
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(data.GetType()).WriteObject(stream, data);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Disposes the RemoteSocket.
        /// </summary>
        private void DisconnectFromRemote()
        {
            _remoteSocket.Dispose();
            _remoteSocket = null;
        }
    }

    /// <summary>
    /// The event args that contain the message.
    /// </summary>
    public class TcpMessageReceivedEventArgs : EventArgs, IMessageReceivedEventArgs
    {
        public byte[] Message { get; set; }

        /// <summary>
        /// Deserializes Message by using the DataContractJsonSerializer.
        /// </summary>
        void IMessageReceivedEventArgs.GetDeserializedMessage(ref object message)
        {
            using (var stream = new MemoryStream(Message))
            {
                message = new DataContractJsonSerializer(message.GetType()).ReadObject(stream);
            }
        }

        /// <summary>
        /// Converts the Message to a string.
        /// </summary>
        public override string ToString()
        {
            return System.Text.Encoding.ASCII.GetString(Message);
        }
    }
}
