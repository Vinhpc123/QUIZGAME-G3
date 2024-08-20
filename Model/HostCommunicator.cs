﻿
using P2PHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace QuizGame.Model
{
    /// <summary>
    /// Provides a game-oriented adapter to the P2PSessionHost class. 
    /// </summary>
    public sealed class HostCommunicator : IHostCommunicator
    {
        /// <summary>
        /// Maps the player names to their guids.
        /// </summary>
        private Dictionary<string, Guid> _playerToParticipantMap = new Dictionary<string, Guid>();

        /// <summary>
        /// The manager that sends UDP advertisement messages and manages a list of participants.
        /// </summary>
        private ISessionManager _manager = new UdpManager();

        /// <summary>
        /// The list of communication channels to send messages to participants.
        /// </summary>
        private List<ICommunicationChannel> _participantCommunicationChannels = new List<ICommunicationChannel>();

        /// <summary>
        /// The communication channel to listen for messages from participants.
        /// </summary>
        private ICommunicationChannel _managerCommunicationChannel = new TcpCommunicationChannel();

        public event EventHandler<PlayerEventArgs> PlayerJoined = delegate { };

        public event EventHandler<PlayerEventArgs> PlayerDeparted = delegate { };

        public event EventHandler<AnswerReceivedEventArgs> AnswerReceived = delegate { };

        public HostCommunicator()
        {
            _manager.ParticipantConnected += (async (sender, e) =>
            {
                // A player has joined the game.
                _participantCommunicationChannels.Add(_manager.CreateCommunicationChannel(e.Id));

                HostCommand data = new HostCommand
                {
                    PlayerName = e.Message.ToString(),
                    QuestionAnswer = 0,
                    Command = Command.Join
                };

                await OnPlayerJoinedAsync(data, e.Id);
            });

            _managerCommunicationChannel.MessageReceived += ((sender, e) =>
            {
                // De serialize the message and put it in a command variable.
                object data = new HostCommand();
                e.GetDeserializedMessage(ref data);
                var command = data as HostCommand;

                // Place the indicated actions in an array, so we can index them based on the Command enum value.
                Action[] actions = new Action[]{
                    // Do nothing if Command == Join, because we already do this on ParticipantConnected.
                    () => { },
                    // if Command == Leave
                    () => {OnPlayerDeparted(command, _playerToParticipantMap[command.PlayerName]); },
                    // if Command == Answer
                    () => {OnAnswerReceived(command, _playerToParticipantMap[command.PlayerName]); }
                };

                // Index the array from the Command and call the associated Action lambda.
                actions[(int)command.Command]();
            });
        }

        public void OnPlayerDeparted(HostCommand commandData, Guid guid)
        {
            PlayerDeparted(this, new PlayerEventArgs { PlayerName = commandData.PlayerName });
            _manager.RemoveParticipant(guid);
            _playerToParticipantMap.Remove(commandData.PlayerName);
        }

        public void OnAnswerReceived(HostCommand commandData, Guid guid)
        {
            AnswerReceived(this, new AnswerReceivedEventArgs
            {
                PlayerName = commandData.PlayerName,
                AnswerIndex = (int)commandData.QuestionAnswer
            });
        }

        public async Task EnterLobbyAsync()
        {
            await _manager.StartAdvertisingAsync();
            await _managerCommunicationChannel.StartListeningAsync();
        }

        public void LeaveLobby() => _manager.StopAdvertising();

        public async Task SendQuestionAsync(Question question)
        {
            var clientList = new List<Guid>();
            clientList.AddRange(_playerToParticipantMap.Values);

            var messageTasks = clientList.Select(client =>
                _manager.CreateCommunicationChannel(client).SendRemoteMessageAsync(question));

            await Task.WhenAll(messageTasks);
        }

        /// <summary>
        /// When a message is received from the participant, this method will attempt to add the participant.
        /// </summary>
        private async Task OnPlayerJoinedAsync(HostCommand commandData, Guid guid)
        {
            Guid duplicatePlayerID;

            if (!_playerToParticipantMap.TryGetValue(commandData.PlayerName, out duplicatePlayerID))
            {
                _playerToParticipantMap.Add(commandData.PlayerName, guid);
                PlayerJoined(this, new PlayerEventArgs { PlayerName = commandData.PlayerName });
            }
        }

    }
}
