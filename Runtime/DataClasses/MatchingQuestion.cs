using System.Collections.Generic;
using System.Linq;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class MatchingQuestion : BaseMultipleQuestion
    {
        public List<SubQuestion> SubQuestions = new List<SubQuestion>();
        public override int CalculateMaxPoints()
        {
            return SubQuestions.Where(x => x.Text != "").Count();
        }
    }
}