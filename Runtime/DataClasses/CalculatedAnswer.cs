namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class CalculatedAnswer : NumericAnswer
    {
        /// <value>Property <c>ToleranceType</c> represents the Type of Tolerance(Relative, Nominell, Geometric) for the Answer.</value>
        public int ToleranceType;
        /// <value>Property <c>CorrectAnswerFormat</c> represents the Format in which the Correct Answer should be shown(Nachkommastellen, signifikante Stellen) for the Answer.</value>
        public int CorrectAnswerFormat;
        /// <value>Property <c>CorrectAnswerLength</c> represents the Number of digits shown for the Correct Answer for the Answer.</value>
        public int CorrectAnswerLength;
    }
}