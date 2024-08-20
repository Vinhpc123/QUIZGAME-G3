
using System;
using System.Threading.Tasks;

namespace QuizGame.Model
{
    public interface IHostCommunicator
    {
        // start broadcasting, accepting players
        Task EnterLobbyAsync();

        // stop broadcasting, stop accepting players
        void LeaveLobby();

        Task SendQuestionAsync(Question question);

        // sender = this, args = PlayerEventArgs
        event EventHandler<PlayerEventArgs> PlayerJoined;

        // sender = this, args = PlayerEventArgs
        event EventHandler<PlayerEventArgs> PlayerDeparted;

        // sender = this, args = AnswerReceivedEventArgs
        event EventHandler<AnswerReceivedEventArgs> AnswerReceived;
    }

    public class PlayerEventArgs : EventArgs { public string PlayerName { get; set; } }
    public class AnswerReceivedEventArgs : PlayerEventArgs { public int AnswerIndex { get; set; } }
}
