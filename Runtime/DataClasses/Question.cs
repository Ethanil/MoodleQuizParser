using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class Question
    {
        /// <value>Property <c>Name</c> represents the Title of the Question.</value>
        public string Name;
        /// <value>Property <c>Questiontext</c> represents the Textbody of the Question.</value>
        public Questiontext Questiontext;
        /// <value>Property <c>GeneralFeedback</c> represents the Feedback that is always shown after the Questions gets evaluated.</value>
        public Questiontext GeneralFeedback;
        /// <value>Property <c>Defaultgrade</c> represents the Number of Points the Question is worth.</value>
        public float Defaultgrade;
        /// <value>Property <c>Penalty</c> represents the penalty in points for a wrong answer.</value>
        public float Penalty;
        /// <value>Property <c>Hidden</c> indicates wether the Question is hidden or not.</value>
        public bool Hidden;
        /// <value>Property <c>IdNumber</c> represents some sort of Identification of this Question and is, besides it's name indicating, not a number.</value>
        public string IdNumber;
        /// <value>Property <c>Tags</c> represents the Tags of the Question.</value>
        public List<string> Tags = new List<string>();
        /// <value>Property <c>questionID</c> represents the unique ID given by moodle(saved in the comment at the top of the question).</value>
        public int QuestionID;

        virtual public int CalculateMaxPoints()
        {
            return (int)Defaultgrade;
        }
    }
}