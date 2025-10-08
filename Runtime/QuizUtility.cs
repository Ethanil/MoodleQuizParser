using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class QuizUtility
    {
        static Regex imageRegex = new Regex("<img.*?>");

        [System.Serializable]
        public class FeedbackParts
        {
            public string[] FirstPart;
            public string[] SecondPart;
            public string[] ThirdPart;
        }

        [System.Serializable]
        public class FeedbackData
        {
            public FeedbackParts PerfektFeedback;
            public FeedbackParts MediumFeedback;
            public FeedbackParts LowFeedback;
        }
        public static void LoadJson()
        {
            TextAsset jsonAsset = Resources.Load<TextAsset>("JSON-Files/GeneralFeedback");
            data = JsonUtility.FromJson<FeedbackData>(jsonAsset.text);
        }
        static FeedbackData data = null;
        private static void GenerateGeneralFeedback(Questiontext questionText, VisualElement parent, QuizResult quizResult)
        {
            if (data == null)
            {
                LoadJson();
            }
            var percentage = (float)quizResult.Points / quizResult.MaxPoints;
            VisualElement currentTextRow = new VisualElement();
            //currentTextRow.AddToClassList("question-text-row");
            parent.Add(currentTextRow);
            Label label = new Label();
            label.style.whiteSpace = WhiteSpace.Normal;
            //label.AddToClassList("question-text-row-label");
            currentTextRow.Add(label);
            if (percentage >= 0.95)
            {
                label.text = GenerateFromList(data.PerfektFeedback);
            }
            else if (percentage >= 0.5)
            {
                label.text = GenerateFromList(data.MediumFeedback);
            }
            else
            {
                label.text = GenerateFromList(data.LowFeedback);
            }
        }
        private static string GenerateFromList(FeedbackParts feedbackParts)
        {
            var first = feedbackParts.FirstPart[random.Next(0, feedbackParts.FirstPart.Length)];
            var second = feedbackParts.SecondPart[random.Next(0, feedbackParts.SecondPart.Length)];
            var third = feedbackParts.ThirdPart[random.Next(0, feedbackParts.ThirdPart.Length)];
            return first + " " + second + " " + third;
        }
        public static void GenerateFeedbackText(Questiontext questionText, VisualElement parent, QuizResult quizResult)
        {
            if (questionText.Text != "")
            {
                GenerateQuestionText(questionText, parent);
            }
            GenerateGeneralFeedback(questionText, parent, quizResult);

        }
        public static void GenerateQuestionText(Questiontext questionText, VisualElement parent)
        {
            if (parent.GetType() == typeof(ScrollView))
            {
                (parent as ScrollView).mouseWheelScrollSize = 400;
            }
            VisualElement currentTextRow = new VisualElement();
            currentTextRow.AddToClassList("question-text-row");
            parent.Add(currentTextRow);
            string text = questionText.Text;
            int currentIndex = 0;
            // Find all img-tags
            ExtractImages(questionText.Files, parent, ref currentTextRow, text, ref currentIndex);
        }
        private static VisualElement AddWords(string[] wordsToAdd, VisualElement questionText, VisualElement currentTextRow)
        {
            foreach (var wordToAdd in wordsToAdd)
            {
                List<string> result = new List<string>();
                Regex newLineRegex = new Regex("\\n");
                int newLineLastIndex = 0;
                foreach (Match newLineMatch in newLineRegex.Matches(wordToAdd))
                {
                    if (newLineMatch.Index > newLineLastIndex)
                    {
                        Label preText = new Label();
                        preText.AddToClassList("question-text-row-label");
                        preText.text = wordToAdd.Substring(newLineLastIndex, newLineMatch.Index - newLineLastIndex);
                        currentTextRow.Add(preText);
                    }
                    if (currentTextRow.childCount > 0)
                    {
                        currentTextRow = new VisualElement();
                        currentTextRow.AddToClassList("question-text-row");
                        questionText.Add(currentTextRow);
                    }

                    newLineLastIndex = newLineMatch.Index + newLineMatch.Length;

                }
                if (newLineLastIndex < wordToAdd.Length)
                {
                    Label postText = new Label();
                    postText.AddToClassList("question-text-row-label");
                    postText.text = wordToAdd.Substring(newLineLastIndex);
                    currentTextRow.Add(postText);
                }
            }
            return currentTextRow;
        }
        private static readonly Dictionary<string, Func<double>> noArgFunctions = new Dictionary<string, Func<double>>() {
        {"pi", () => Math.PI },
    };
        private static readonly Dictionary<string, Func<double, double>> singleArgFunctions = new Dictionary<string, Func<double, double>>()
    {
        { "abs", Math.Abs }, { "acos", Math.Acos }, { "acosh", Math.Acosh }, { "asin", Math.Asin }, { "asinh", Math.Asinh }, { "atan", Math.Atan }, { "atanh", Math.Atanh },
        { "bindec", value => Convert.ToInt64(value.ToString(), 2) }, { "decbin", value => Convert.ToInt64(Convert.ToString((int)value, 2)) },
        { "decoct", value => Convert.ToInt64(Convert.ToString((int)value, 8)) }, { "octdec", value => Convert.ToInt64(value.ToString(), 8) },
        { "deg2rad", value => value * (Math.PI / 180) }, { "rad2deg", value => value * (180 / Math.PI) },
        { "ceil", Math.Ceiling },
        { "cos", Math.Cos }, { "cosh", Math.Cosh }, { "exp", Math.Exp }, { "expm1", Math.Exp }, // You'll need to adjust expm1 for accuracy
        { "floor", Math.Floor },
        { "log", Math.Log }, { "log10", Math.Log10 }, { "sin", Math.Sin }, { "sinh", Math.Sinh }, { "sqrt", Math.Sqrt }, {"round", Math.Round },
        { "tan", Math.Tan }, { "tanh", Math.Tanh }, // add other functions as needed
    };
        private static readonly Dictionary<string, Func<double, double, double>> doubleArgFunctions = new Dictionary<string, Func<double, double, double>>()
    {

        { "atan2", Math.Atan2 }, {"fmod", (val1, val2) => val1 % val2 }, {"max", Math.Max }, {"min", Math.Min }, {"pow", Math.Pow },
    };

        public static double Evaluate(string formula, Dictionary<string, double> variables)
        {
            foreach (var variable in variables)
            {
                formula = formula.Replace($"{{{variable.Key}}}", variable.Value.ToString(CultureInfo.InvariantCulture));
            }


            while (formula.Contains("**"))
            {
                int index = formula.IndexOf("**");
                int start = index - 1;
                int end = index + 2;

                // Find the base
                while (start >= 0 && (char.IsDigit(formula[start]) || formula[start] == '.'))
                {
                    start--;
                }
                start++;

                // Find the exponent
                while (end < formula.Length && (char.IsDigit(formula[end]) || formula[end] == '.'))
                {
                    end++;
                }

                string baseStr = formula.Substring(start, index - start);
                string expStr = formula.Substring(index + 2, end - index - 2);

                double baseValue = Convert.ToDouble(baseStr, CultureInfo.InvariantCulture);
                double expValue = Convert.ToDouble(expStr, CultureInfo.InvariantCulture);

                double powResult = Math.Pow(baseValue, expValue);
                formula = formula.Substring(0, start) + powResult.ToString(CultureInfo.InvariantCulture) + formula.Substring(end);

            }
            foreach (var func in noArgFunctions)
            {
                formula = Regex.Replace(formula, $@"\b{func.Key}\b\(\)", match =>
                {
                    return func.Value().ToString(CultureInfo.InvariantCulture);
                });
            }
            foreach (var func in singleArgFunctions)
            {
                formula = ReplaceFunction(formula, func.Key, (str) => func.Value(Evaluate(str, variables)));
            }
            foreach (var func in doubleArgFunctions)
            {
                formula = ReplaceFunction(formula, func.Key, (str) =>
                {
                    Match match = new Regex(@"([^,]+),\s*([^)]+)").Match(str);
                    return func.Value(
                    Evaluate(match.Groups[1].Value, variables),
                    Evaluate(match.Groups[2].Value, variables));
                });
            }

            // Use DataTable to compute the final result
            var table = new DataTable();
            try
            {
                return Convert.ToDouble(table.Compute(formula, string.Empty));
            }
            catch (Exception)
            {
                return 0;
            }

        }

        private static string ReplaceFunction(string formula, string functionKey, Func<string, double> eval)
        {
            int startIndex, endIndex;
            startIndex = endIndex = formula.IndexOf(functionKey);
            while (endIndex < formula.Length && endIndex != -1)
            {
                endIndex = startIndex + functionKey.Length;
                int startIndexOfBrackets = startIndex + functionKey.Length;
                endIndex = CaptureBrackets(formula, endIndex, '(', ')');
                string innerFormula = formula.Substring(startIndexOfBrackets + 1, endIndex - startIndexOfBrackets - 1);
                double innerResult = eval(innerFormula);
                formula = formula.Substring(0, startIndex) + innerResult.ToString(CultureInfo.InvariantCulture) + formula.Substring(endIndex + 1);
                startIndex = endIndex = formula.IndexOf(functionKey);
            }

            return formula;
        }

        public static string EvaluateFeedbackText(string feedbacktext, Dictionary<string, double> variables)
        {
            feedbacktext = Regex.Replace(feedbacktext, $@"", match =>
            {
                return "";
            });

            return feedbacktext;
        }

        public static string EvaluateEqualBracketsAndReplaceVariables(string text, Dictionary<string, double> variables)
        {
            int startIndex, endIndex;
            startIndex = endIndex = text.IndexOf("{=");
            while (endIndex < text.Length && endIndex != -1)
            {
                endIndex = CaptureBrackets(text, endIndex, '{', '}');
                string formula = text.Substring(startIndex + 2, endIndex - startIndex - 2);
                double result = Evaluate(formula, variables);
                text = text.Substring(0, startIndex) + result.ToString(CultureInfo.InvariantCulture) + text.Substring(endIndex + 1);
                startIndex = endIndex = text.IndexOf("{=");
            }
            foreach (var variable in variables)
            {
                text = text.Replace($"{{{variable.Key}}}", variable.Value.ToString(CultureInfo.InvariantCulture));
            }
            return text;
        }

        private static int CaptureBrackets(string text, int endIndex, char openingBracket, char closingBracket)
        {
            int numberOfOpenBrackets = 1;
            while (numberOfOpenBrackets > 0)
            {
                endIndex++;
                char charAtEndIndex = text[endIndex];
                if (charAtEndIndex == openingBracket)
                    numberOfOpenBrackets++;
                else if (charAtEndIndex == closingBracket)
                    numberOfOpenBrackets--;
            }
            return endIndex;
        }

        static System.Random random = new System.Random();

        public static Dictionary<string, double> GenerateRandomVariables(List<DatasetDefinition> datasetDefinitions)
        {
            var result = new Dictionary<string, double>();
            foreach (var datasetDefinition in datasetDefinitions)
            {
                int min = (int)(datasetDefinition.Minimum * Math.Pow(10, datasetDefinition.Decimals));
                int max = (int)(datasetDefinition.Maximum * Math.Pow(10, datasetDefinition.Decimals));
                double ran = ((double)random.Next(min, max)) / Math.Pow(10, datasetDefinition.Decimals);
                result.Add(datasetDefinition.Name, ran);
            }
            return result;
        }






        public static CalculatedQuestionWithUnit ConvertToCalculatedQuestion(NumericQuestion question)
        {
            CalculatedQuestionWithUnit calculatedQuestion = new CalculatedQuestionWithUnit
            {
                Name = question.Name,
                Questiontext = question.Questiontext,
                GeneralFeedback = question.GeneralFeedback,
                Defaultgrade = question.Defaultgrade,
                Penalty = question.Penalty,
                Hidden = question.Hidden,
                IdNumber = question.IdNumber,
                Tags = question.Tags.CloneViaSerialization(),
                QuestionID = question.QuestionID,
                UnitGradingType = question.UnitGradingType,
                UnitPenalty = question.UnitPenalty,
                ShowUnits = question.ShowUnits,
                UnitsLeft = question.UnitsLeft,
                Units = question.Units.CloneViaSerialization()
            };
            foreach (var answer in question.Answers)
            {
                calculatedQuestion.Answers.Add(
                    new CalculatedAnswer
                    {
                        Answertext = answer.Answertext,
                        Feedback = answer.Feedback,
                        Fraction = answer.Fraction,
                        Tolerance = answer.Tolerance
                    });
            }



            return calculatedQuestion;
        }



        public static void ExtractImages(List<MoodleFile> images, VisualElement parent, ref VisualElement currentTextRow, string text, ref int currentIndex)
        {
            foreach (Match match in imageRegex.Matches(text))
            {
                if (currentIndex < match.Index)
                {
                    currentTextRow = AddWords(text.Substring(currentIndex, match.Index - currentIndex).Split(""), parent, currentTextRow);
                }
                string imageName = match.Value.Substring(25, match.Length - 27);
                currentIndex = match.Index + match.Length;
                var imageFile = images.Find((file) => file.Name == imageName);
                if (imageFile == null)
                    continue;
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(imageFile.Data);
                Image image = new Image();
                image.image = tex;
                currentTextRow.Add(image);
            }
            // Add rest of text (or all text if there is no image)
            currentTextRow = AddWords(text.Substring(currentIndex, text.Length - currentIndex).Split(""), parent, currentTextRow);
        }
    }
}