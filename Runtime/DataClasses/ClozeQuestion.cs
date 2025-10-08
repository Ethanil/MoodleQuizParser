using System.Collections.Generic;


namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class ClozeQuestion : Question
    {
        public string PostText;
        public List<ClozeSubQuestion> SubQuestions = new List<ClozeSubQuestion>();
        public override int CalculateMaxPoints()
        {
            return SubQuestions.Count;
        }
    }

    public class ClozeSubQuestion
    {
        public enum ClozeType
        {
            SHORTANSWER,
            SHORTANSWER_C,
            NUMERICAL,
            MULTICHOICE,
            MULTICHOICE_S,
            MULTICHOICE_V,
            MULTICHOICE_VS,
            MULTICHOICE_H,
            MULTICHOICE_HS,
            MULTIRESPONSE,
            MULTIRESPONSE_S,
            MULTIRESPONSE_H,
            MULTIRESPONSE_HS,
        }
        public int Grade { get; set; }
        public ClozeType Type { get; set; }
        public List<Answer> Answers { get; set; } = new List<Answer>();

        public string PreText;
    }
}