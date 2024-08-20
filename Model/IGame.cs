
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace QuizGame.Model
{
    public interface IGame : INotifyPropertyChanged
    {
        event EventHandler<QuestionEventArgs> NewQuestionAvailable;
        void AddPlayer(string playerName);
        Question CurrentQuestion { get; }
        GameState GameState { get; set; }
        Dictionary<string, int> GetResults();
        bool IsGameOver { get; }
        void NextQuestion();
        ObservableCollection<string> PlayerNames { get; set; }
        List<Question> Questions { get; }
        void RemovePlayer(string playerName);
        void StartGame();
        bool SubmitAnswer(string playerName, int answerIndex);
        Dictionary<string, Dictionary<Question, int?>> SubmittedAnswers { get; }
        string Winner { get; }
    }

    public class QuestionEventArgs : EventArgs { public Question Question { get; set; } }

    public class PlayerJoinedEventArgs : EventArgs { public bool IsJoined { get; set; } }
}
