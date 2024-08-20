
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace QuizGame.Model
{
    
    public class HostMessage
    {
        public HostMessageType Type { get; set; }
        public bool IsJoined { get; set; }
        public Question Question { get; set; }
    }

    
    public enum HostMessageType
    {
        Question,
        JoinStatus
    };

    public class Question
    {
        public string Text { get; set; }
        public List<string> Options { get; set; }
        public int CorrectAnswerIndex { get; set; }
    }
}
