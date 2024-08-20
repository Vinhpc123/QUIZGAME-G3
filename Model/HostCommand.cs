
using System.Runtime.Serialization;

namespace QuizGame.Model
{
    public class HostCommand
    {
        public Command Command { get; set; }
        public string PlayerName { get; set; }
        public int QuestionAnswer { get; set; }
    }

    public enum Command
    {
        Join = 0,
        Leave,
        Answer
    };
}
