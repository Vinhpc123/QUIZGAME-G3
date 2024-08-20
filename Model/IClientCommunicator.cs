
using System;
using System.Threading.Tasks;

namespace QuizGame.Model
{
    public interface IClientCommunicator
    {
        Task InitializeAsync();
        
        // Adds the specified player to the current game.
        Task JoinGameAsync(string playerName);

        // Removes the specified player from the current game.
        Task LeaveGameAsync(string playerName);

        // Submits an answer to the current question.
        Task AnswerQuestionAsync(string playerName, int option);

        // Occurs when a game is available for joining. 
        event EventHandler GameAvailable;

        // Occurs when new question data has arrived.
        event EventHandler<QuestionEventArgs> NewQuestionAvailable;

        // Occurs when the server has received a join request and either acknowledges it, or denies it contingent on uniqueness of client name.
        event EventHandler<PlayerJoinedEventArgs> PlayerJoined;
    }
}
