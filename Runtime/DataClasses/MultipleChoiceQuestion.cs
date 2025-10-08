using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class MultipleChoiceQuestion : BaseMultipleQuestion
    {
        /// <value>Property <c>ShowStandardinstruction</c> indicates wether the standard instructions should be shown or not.</value>
        public bool ShowStandardinstruction;
        public List<MultipleChoiceAnswer> Answers = new List<MultipleChoiceAnswer>();
        public override int CalculateMaxPoints()
        {
            return Answers.Count;
        }
    }
}