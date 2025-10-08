using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class StackQuestionPRT
    {
        public string Name;
        public float Value;
        public bool AutoSimplify;
        public int FeedbackStyle;
        public string FeedbackVariables;
        public List<PRTNode> Nodes = new List<PRTNode>();
    }
}