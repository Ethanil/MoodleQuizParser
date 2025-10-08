using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class MolecularQuestion : Question
    {
        public int UseCase;
        public bool AllowSubscript;
        public bool AllowSuperscript;
        public bool ForceLength;
        public string ApplyDictionaryCheck;
        public bool ExendDictionary;
        public string SentenceDividers;
        public bool ConvertToSpace;
        public bool ModelAnswer;
        public List<MolecularAnswer> Answers = new List<MolecularAnswer>();
    }
}