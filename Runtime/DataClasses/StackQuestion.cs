using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class StackQuestion : Question
    {
        public string StackVersion;
        public string QuestionVariables;
        public string SpecificFeedback;
        public string QuestionNote;
        public string QuestionDescription;
        public bool QuestionSimplify;
        public bool AssumePositive;
        public bool AssumeReal;
        public string PRTCorrect;
        public string PRTPartiallyCorrect;
        public string PRTIncorrect;
        public string Decimals;
        public string MultiplicationSign;
        public int SquareRootSign;
        public string ComplexNumber;
        public string InverseTrigonomy;
        public string LogicSymbol;
        public string MatrixParenthesis;
        public bool VariantsSelectionSeed;
        public List<StackQuestionInput> Inputs = new List<StackQuestionInput>();
        public List<StackQuestionPRT> PRTs = new List<StackQuestionPRT>();
    }
}