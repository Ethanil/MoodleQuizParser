namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class Answer
    {
        /// <value>Property <c>Feedback</c> represents the Feedback for the Answer.</value>
        public string Feedback;
        /// <value>Property <c>Fraction</c> represents the Fraction of the maximum Points this answer is worth. Wrong answers have a Fraction of 0.</value>
        public float Fraction;
        /// <value>Property <c>Answertext</c> represents the Textbody of the Answer.</value>
        public string Answertext;
    }
}