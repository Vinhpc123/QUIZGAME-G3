
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;

namespace P2PHelper
{
    public class UdpParticipant : SessionParticipant
    {
        ///
        /// The default port to use when listening for UDP messages. 
        /// This port was chosen randomly in the ephemeral port range.
        /// </summary>
        private const string UDP_COMMUNICATION_PORT = "56788";

        /// <summary>
        /// The default IP to use when listening for multicast messages. 
        /// This IP was chosen randomly as part of the multicast IP range.
        /// </summary>
        private const string UDP_MULTICAST_IP = "237.1.3.37";

        /// <summary>
        /// The datagram socket that will be listening for incoming advertiser messages
        /// </summary>
        private DatagramSocket _listenerSocket;

        /// <summary>
        /// The port that the listener will be listening for UDP messages to and accept message from.
        /// </summary>
        public string ListenerPort { get; set; } = UDP_COMMUNICATION_PORT;

        /// <summary>
        /// The multicast group that the listener will be sending UDP messages to.
        /// </summary>
        public HostName ListenerGroupHost { get; set; } = new HostName(UDP_MULTICAST_IP);

        /// <summary>
        /// The message that will be sent when connecting to a manager.
        /// </summary>
        public String ListenerMessage { get; set; }

        /// <summary>
        /// Start listening.
        /// </summary>
        public override async Task<bool> StartListeningAsync()
        {
            bool status = false;

            if (_listenerSocket == null)
            {
                _listenerSocket = new DatagramSocket();
                _listenerSocket.MessageReceived += AdvertisementMessageReceivedFromManagerAsync;
                await _listenerSocket.BindServiceNameAsync(ListenerPort);
                _listenerSocket.JoinMulticastGroup(ListenerGroupHost);

                status = true;
            }

            return status;
        }

        /// <summary>
        /// Stop listening for subscriptions.
        /// </summary>
        public override bool StopListening()
        {
            bool status = false;

            if (_listenerSocket != null)
            {
                _listenerSocket.Dispose();
                _listenerSocket = null;

                status = true;
            }

            return status;
        }

        /// <summary>
        /// Sends a UDP message to the advertiser which indicates a join.
        /// </summary>
        public async override Task ConnectToManagerAsync(Guid manager)
        {
            var subscription = base.Managers[manager] as UdpManagerInformation;

            var outStream = (await _listenerSocket.GetOutputStreamAsync(subscription.Host, ListenerPort)).AsStreamForWrite();

            using (var writer = new StreamWriter(outStream))
            {
                await writer.WriteLineAsync(ListenerMessage);
                await writer.FlushAsync();
            }
        }

        /// <summary>
        /// Creates a TcpCommunicationChannel object and returns it so that app developers can send custom TCP messages to the manager.
        /// Returns a null remote host name in TcpCommunicationChannel object if the manager didn't exist.
        /// </summary>
        public override ICommunicationChannel CreateCommunicationChannel(Guid manager, int flags = 0)
        {
            var managerCast = Managers[manager] as UdpManagerInformation;
            return new TcpCommunicationChannel { RemoteHostname = managerCast == null ? null : managerCast.Host };
        }

        /// <summary>
        /// Adds the manager to the list of Managers, when a UDP advertisement message is received..
        /// </summary>
        private async void AdvertisementMessageReceivedFromManagerAsync(DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args)
        {
            // Read the advertisement message.
            using (var reader = new StreamReader(args.GetDataStream().AsStreamForRead()))
            {
                string message = await reader.ReadLineAsync();

                // Add the manager to the list of Managers.
                base.AddManager(new UdpManagerInformation { Host = args.RemoteAddress }, message);
            }
        }
    }

    public class UdpManagerInformation
    {
        public HostName Host { get; set; }

        public override bool Equals(object obj)
        {
            var objCast = obj as UdpManagerInformation;
            return objCast != null ? Host.IsEqual(objCast.Host) : false;
        }

        public override int GetHashCode() => Host.GetHashCode();
    }
}
