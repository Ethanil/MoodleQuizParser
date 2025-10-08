namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public abstract class BaseMultipleQuestion : Question
    {
        /// <value>Property <c>Single</c> indicates wether the Question has a Single or Multiple Answers.</value>
        public bool Single;
        /// <value>Property <c>ShuffleAnswers</c> indicates wether the Answers should be shuffled or not.</value>
        public bool ShuffleAnswers;
        /// <value>Property <c>Answernumbering</c> represents with what prefix the Answers should be numbered.</value>
        public string Answernumbering;
        /// <value>Property <c>CorrectFeedback</c> represents the Feedback for correctly answering the Question.</value>
        public string CorrectFeedback;
        /// <value>Property <c>PartiallyCorrectFeedback</c> represents the Feedback for partially correctly answering the Question.</value>
        public string PartiallyCorrectFeedback;
        /// <value>Property <c>IncorrectFeedback</c> represents the Feedback for incorrectly answering the Question.</value>
        public string IncorrectFeedback;
        /// <value>Property <c>ShowNumCorrect</c> indicates wether the Number of Answers that were chosen correctly should be shown.</value>
        public bool ShowNumCorrect;
    }
}