namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class EssayQuestion : Question
    {
        public string ResponseFormat;
        public bool ResponseRequired;
        public int ResponseFieldLines;
        public int MinWordLimit;
        public int MaxWordLimit;
        public int Attachments;
        public bool AttachmentsRequired;
        public int MaxBytes;
        public string FileTypesList;
        public string GraderInfo;
        public string ResponseTemplate;
    }
}