
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Networking.ServiceDiscovery.Dnssd;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using System.IO;

namespace P2PHelper
{
    public class DnsSdManager : SessionManager
    {
        /// <summary>
        /// Tin nhắn nhận được cho biết đang có bài kiểm tra.
        /// </summary>
        private const string TEST_MESSAGE = "connection_test";

        /// <summary>
        /// Cổng mặc định được sử dụng khi kết nối lại với trình quản lý.
        /// </summary>
        private const string DEFAULT_PORT = "56788";

        /// <summary>
        /// Tên phiên bản mặc định khi đăng ký dịch vụ DNS-SD.
        /// </summary>
        private const string INSTANCE_NAME = "DnssdManager";

        /// <summary>
        /// Giao thức mạng sẽ chấp nhận kết nối để phản hồi.
        /// </summary>
        private const string NETWORK_PROTOCOL = "_tcp";

        /// <summary>
        /// Tên miền đăng ký DNS-SD.
        /// </summary>
        private const string DOMAIN = "local";

        /// <summary>
        /// Loại dịch vụ đăng ký DNS-SD.
        /// </summary>
        private const string SERVICE_TYPE = "_p2phelper";

        /// <summary>
        /// Tên của dịch vụ DNS-SD đang được đăng ký. Bạn có thể cung cấp bất kỳ tên nào bạn muốn.
        /// </summary>
        public string InstanceName { get; set; } = INSTANCE_NAME;

        /// <summary>
        /// Đối tượng dịch vụ DNS-SD.
        /// </summary>
        private DnssdServiceInstance _service;

        /// <summary>
        /// Socket TCP sẽ chấp nhận kết nối cho phản hồi dịch vụ DSN-SD.
        /// </summary>
        private StreamSocketListener _socket;

        /// <summary>
        /// Cổng sử dụng khi kết nối lại với máy chủ.
        /// </summary>
        public string Port { get; set; } = DEFAULT_PORT;

        /// <summary>
        /// Đăng ký dịch vụ DNS-SD.
        /// </summary>
        public override async Task<bool> StartAdvertisingAsync()
        {
            bool status = false;

            if (_socket == null && _service == null)
            {
                _socket = new StreamSocketListener();
                _socket.ConnectionReceived += MessageToConnectReceivedFromParticipantAsync;
                await _socket.BindServiceNameAsync(Port);

                _service = new DnssdServiceInstance(
                    $"{InstanceName}.{SERVICE_TYPE}.{NETWORK_PROTOCOL}.{DOMAIN}.",
                NetworkInformation.GetHostNames()
                .FirstOrDefault(x => x.Type == HostNameType.DomainName && x.RawName.Contains("local")),
                    UInt16.Parse(_socket.Information.LocalPort)
                );

                var operationStatus = await _service.RegisterStreamSocketListenerAsync(_socket);

                status = true;
            }

            return status;
        }

        /// <summary>
        /// Hủy đăng ký và xóa dịch vụ DNS-SD. Nếu phương pháp này không được gọi,
        /// việc đăng ký sẽ vẫn có thể phát hiện được, ngay cả khi ứng dụng không chạy.
        /// </summary>
        public override bool StopAdvertising()
        {
            bool status = false;

            if (_socket != null && _service != null)
            {
                _socket.ConnectionReceived -= MessageToConnectReceivedFromParticipantAsync;
                _socket.Dispose();
                _socket = null;
                _service = null;

                status = true;
            }

            return status;
        }

        /// <summary>
        /// Tạo đối tượng TcpCommunicationChannel và trả về đối tượng này để các nhà phát triển ứng dụng có thể gửi tin nhắn TCP tùy chỉnh đến trình quản lý.
        /// Trả về tên máy chủ từ xa null trong đối tượng TcpCommunicationChannel nếu trình quản lý không tồn tại.       
        /// </summary>
        public override ICommunicationChannel CreateCommunicationChannel(Guid participant, int flags = 0)
        {
            TcpCommunicationChannel channel = new TcpCommunicationChannel();
            channel.RemoteHostname = (Participants[participant] as DnssdParticipantInformation).Host;
            return channel;
        }

        /// <summary>
        /// Khi nhận được tin nhắn mới, người tham gia sẽ được thêm vào danh sách Người tham gia.        /// </summary>
        private async void MessageToConnectReceivedFromParticipantAsync(
            StreamSocketListener sender, 
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            var participant = new DnssdParticipantInformation { Host = args.Socket.Information.RemoteAddress };

            // Đọc tin nhắn của người đăng ký.
            using (var reader = new StreamReader(args.Socket.InputStream.AsStreamForRead()))
            {
                string message = await reader.ReadLineAsync();

                if (message != TEST_MESSAGE)
                {
                    // Thêm người tham gia.
                    base.AddParticipant(participant, message);
                }
            }
        }
    }
//////
    public class DnssdParticipantInformation
    {
        public HostName Host { get; set; }

        public override bool Equals(object obj)
        {
            var objCast = obj as DnssdParticipantInformation;
            return objCast != null ? Host.IsEqual(objCast.Host) : false;
        }

        public override int GetHashCode() => Host.GetHashCode();
    }
}
