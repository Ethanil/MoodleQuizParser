using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class OrderingQuestion : Question
    {
        /// <value>Property <c>NumberingStyle</c> represents with what prefix the Answers should be numbered.</value>
        public string NumberingStyle;
        /// <value>Property <c>CorrectFeedback</c> represents the Feedback for correctly answering the Question.</value>
        public string CorrectFeedback;
        /// <value>Property <c>PartiallyCorrectFeedback</c> represents the Feedback for partially correctly answering the Question.</value>
        public string PartiallyCorrectFeedback;
        /// <value>Property <c>IncorrectFeedback</c> represents the Feedback for incorrectly answering the Question.</value>
        public string IncorrectFeedback;
        /// <value>Property <c>ShowNumCorrect</c> indicates wether the Number of Answers that were chosen correctly should be shown.</value>
        public bool ShowNumCorrect;
        /// <value>Property <c>LayoutType</c></value>
        public string LayoutType;
        /// <value>Property <c>SelectType</c></value>
        public string SelectType;
        /// <value>Property <c>SelectCount</c></value>
        public int SelectCount;
        /// <value>Property <c>GradingType</c></value>
        public string GradingType;
        /// <value>Property <c>ShowGrading</c></value>
        public string ShowGrading;
        /// <value>Property <c>Answers</c></value>
        public List<OrderingAnswer> Answers = new List<OrderingAnswer>();

        public override int CalculateMaxPoints()
        {
            return Answers.Count;
        }
    }
}