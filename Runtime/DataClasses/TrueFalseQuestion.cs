using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class TrueFalseQuestion : Question
    {
        public List<MultipleChoiceAnswer> Answers = new List<MultipleChoiceAnswer>();
        public override int CalculateMaxPoints()
        {
            return Answers.Count;
        }
    }
}