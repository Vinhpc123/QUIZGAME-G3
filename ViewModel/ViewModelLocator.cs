
using P2PHelper;
using QuizGame.Model;
using System.Collections.Generic;
using Windows.ApplicationModel;

namespace QuizGame.ViewModel
{
    public static class ViewModelLocator
    {
        private static IClientCommunicator clientCommunicator;
#if LOCALTESTMODEON
        private static IClientCommunicator clientCommunicator2;
#endif
        private static IHostCommunicator hostCommunicator;

        static ViewModelLocator()
        {
#if LOCALTESTMODEON
            hostCommunicator = new MockHostCommunicator();
            var mockHostCommunicator = hostCommunicator as MockHostCommunicator;
            clientCommunicator = new MockClientCommunicator { Host = mockHostCommunicator };
            clientCommunicator2 = new MockClientCommunicator { Host = mockHostCommunicator };
            mockHostCommunicator.Client1 = clientCommunicator as MockClientCommunicator;
            mockHostCommunicator.Client2 = clientCommunicator2 as MockClientCommunicator;
#else
            clientCommunicator = new ClientCommunicator();
            hostCommunicator = new HostCommunicator();
#endif
        }

        public static ClientViewModel ClientViewModel
        {
            get
            {
                return clientViewModel ?? (clientViewModel = 
                    new ClientViewModel(clientCommunicator) { IsJoined = DesignMode.DesignModeEnabled });
            }
        }
        private static ClientViewModel clientViewModel;

#if LOCALTESTMODEON
        public static ClientViewModel ClientViewModel2
        {
            get
            {
                return clientViewModel2 ?? (clientViewModel2 = clientViewModel2 =
                    new ClientViewModel(clientCommunicator2) { IsJoined = DesignMode.DesignModeEnabled });
            }
        }
        private static ClientViewModel clientViewModel2;
#endif

        public static HostViewModel HostViewModel
        {
            get
            {
#if LOCALTESTMODEON
                var game = GetSampleGame();
#else
                var game = DesignMode.DesignModeEnabled ? GetSampleGame() : GetSampleGame();
#endif
                return hostViewModel ?? (hostViewModel = new HostViewModel(game, hostCommunicator)
                {
                    GameState = DesignMode.DesignModeEnabled ? GameState.GameUnderway : GameState.Lobby
                });
            }
        }
        private static HostViewModel hostViewModel;

        private static Game GetSampleGame()
        {
            var questions = new List<Question>
            {
                new Question 
                { 
                    Text = "Trong lập trình Socket TCP, khi muốn gửi dữ liệu từ Client tới Server thông qua luồng xuất (OutputStream) thì ta sử dụng phương thức làm việc nào:", 
                    Options = new List<string> 
                    {   "receive()",
                        "read()",
                        "write()",
                        "send()" 
                    }, 
                    CorrectAnswerIndex = 3
                },
                new Question
                {
                    Text = "Phương thức nào sau đây không thuộc lớp DatagramSocket: ",
                    Options = new List<string>
                    {
                        "send()",
                        "receive()",
                        "close()",
                        "accept()"
                    },
                    CorrectAnswerIndex = 4
                },
                new Question
                {
                    Text = "Trong lập trình Socket TCP/IP, Server muốn gửi dữ liệu đến Client thì phải sử dụng phương thức nào của luồng xuất (OutputStream) :",
                    Options = new List<string>
                    {   "receive()",
                        "read()",
                        "write()",
                        "send()"
                    },
                    CorrectAnswerIndex = 3
                },
                new Question
                {
                    Text = "Package là tập hợp của:",
                    Options = new List<string>
                    {
                        "Lớp và Interface",
                        "Lớp",
                        "Interface",
                        "Các công cụ biên dịch"
                    },
                    CorrectAnswerIndex = 1
                },
                new Question
                {
                    Text = "UDP được gọi là giao thức:",
                    Options = new List<string>
                    {
                        "Hướng không kết nối, không tin cậy ",
                        "Hướng không kết nối, tin cậy",
                        "Hướng kết nối, không tin cậy",
                        "Tất cả đều sai"
                    },
                    CorrectAnswerIndex = 1
                },
                new Question
                {
                    Text = "Cơ chế giao tiếp 1-đến-tất cả (one-to-all) giữa một nguồn đến tất cả các hosts trong một mạng được xem là: ",
                    Options = new List<string>
                    {
                        "Các câu trên đều sai ",
                        "Broadcast",
                        "Multicast",
                        "Unicast"
                    },
                    CorrectAnswerIndex = 2
                },
                new Question
                {
                    Text = "Dịch vụ SMTP sử dụng giao thức: ",
                    Options = new List<string>
                    {
                        "TCP",
                        "IP",
                        "Các câu trên đều sai",
                        "UDP"
                    },
                    CorrectAnswerIndex = 1
                },
                new Question
                {
                    Text = "UDP là viết tắt của: ",
                    Options = new List<string>
                    {
                        "Các câu trên đều sai ",
                        "User Delivery Protocol",
                        "User Datagram Procedure",
                        "User Datagram Protocol "
                    },
                    CorrectAnswerIndex = 4
                },
                new Question
                {
                    Text = "Dịch vụ HTTP sử dụng giao thức: ",
                    Options = new List<string>
                    {
                        "TCP và UDP ",
                        "TCP",
                        "UDP",
                        "Các câu trên đều sai "
                    },
                    CorrectAnswerIndex = 2
                },
                new Question
                {
                    Text = "Trong mô hình lập trình TCP client/server, thì: ",
                    Options = new List<string>
                    {
                        "Server phải chạy trước Client để chờ kết nối  ",
                        "Client và Server phải cùng địa chỉ IP",
                        "Server bắt buộc gửi dữ liệu trước",
                        "Client bắt buộc gửi dữ liệu trước "
                    },
                    CorrectAnswerIndex = 1
                },
            };
            return new Game(questions);
        }
    }
}
