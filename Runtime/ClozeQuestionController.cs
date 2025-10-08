using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine.UIElements;
using static TUDarmstadt.SeriousGames.MoodleQuizParser.QuizUtility;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class ClozeQuestionController : AbstractQuestionController<ClozeQuestion>
    {
        VisualTreeAsset m_MultiplechoiceQuestionAnswerTemplate;
        List<VisualElement> m_SubQuestionVisualElements;

        public ClozeQuestionController(ClozeQuestion question, VisualElement root = null, VisualTreeAsset clozeQuestionTemplate = null, VisualTreeAsset multiplechoiceQuestionAnswerTemplate = null)
        {
            m_Root = root;
            m_Question = question;
            m_MultiplechoiceQuestionAnswerTemplate = multiplechoiceQuestionAnswerTemplate;
            m_QuestionTemplate = clozeQuestionTemplate;
            m_SubQuestionVisualElements = new List<VisualElement>();
        }

        override public ScrollView ConstructQuestionText()
        {
            if (m_QuestionText != null)
                return m_QuestionText;
            if (m_Root != null)
                m_QuestionText = m_Root.Q<ScrollView>("question-text");
            if (m_QuestionText == null)
                m_QuestionText = new ScrollView();
            CreateVisualElements(m_Question, m_QuestionText);
            return m_QuestionText;
        }

        static StyleLength inputWidth = new StyleLength { value = new Length { value = 35, unit = LengthUnit.Percent } };
        private void CreateVisualElements(ClozeQuestion question, ScrollView parent)
        {
            Questiontext questionText = question.Questiontext;
            VisualElement currentTextRow = new VisualElement();
            currentTextRow.AddToClassList("question-text-row");
            parent.Add(currentTextRow);
            int currentIndex = 0;
            for (int i = 0; i < question.SubQuestions.Count; i++)
            {
                var clozeSubQuestion = question.SubQuestions[i];
                currentIndex = 0;
                ExtractImages(questionText.Files, parent, ref currentTextRow, clozeSubQuestion.PreText, ref currentIndex);
                switch (clozeSubQuestion.Type)
                {
                    case ClozeSubQuestion.ClozeType.SHORTANSWER:
                    case ClozeSubQuestion.ClozeType.SHORTANSWER_C:
                    case ClozeSubQuestion.ClozeType.NUMERICAL:
                        TextField clozeTextField = new TextField();
                        clozeTextField.style.minWidth = inputWidth;
                        m_SubQuestionVisualElements.Add(clozeTextField);
                        currentTextRow.Add(clozeTextField);
                        clozeTextField.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) => PlayButtonSound(), TrickleDown.TrickleDown);
                        break;
                    case ClozeSubQuestion.ClozeType.MULTICHOICE:
                    case ClozeSubQuestion.ClozeType.MULTICHOICE_S:
                        clozeSubQuestion.Answers.Shuffle();
                        DropdownField dropdownField = new DropdownField();
                        dropdownField.style.minWidth = inputWidth;
                        List<string> answers = new List<string>();
                        foreach (var answer in clozeSubQuestion.Answers)
                        {
                            answers.Add(answer.Answertext);
                        }
                        dropdownField.choices.AddRange(answers);
                        dropdownField.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) => PlayButtonSound(), TrickleDown.TrickleDown);
                        m_SubQuestionVisualElements.Add(dropdownField);
                        currentTextRow.Add(dropdownField);
                        break;
                    case ClozeSubQuestion.ClozeType.MULTICHOICE_V:
                    case ClozeSubQuestion.ClozeType.MULTICHOICE_VS:
                    case ClozeSubQuestion.ClozeType.MULTICHOICE_H:
                    case ClozeSubQuestion.ClozeType.MULTICHOICE_HS:
                        clozeSubQuestion.Answers.Shuffle();
                        var answerList = clozeSubQuestion.Answers.Select(ans => ans.Answertext).ToList();
                        RadioButtonGroup radioButtonGroup = new RadioButtonGroup();
                        radioButtonGroup.choices = answerList;
                        m_SubQuestionVisualElements.Add(radioButtonGroup);
                        currentTextRow.Add(radioButtonGroup);
                        break;
                    case ClozeSubQuestion.ClozeType.MULTIRESPONSE:
                    case ClozeSubQuestion.ClozeType.MULTIRESPONSE_S:
                    case ClozeSubQuestion.ClozeType.MULTIRESPONSE_H:
                    case ClozeSubQuestion.ClozeType.MULTIRESPONSE_HS:
                        clozeSubQuestion.Answers.Shuffle();
                        ListView multiResponseListView = CreateListView(m_MultiplechoiceQuestionAnswerTemplate, clozeSubQuestion.Answers, false);
                        m_SubQuestionVisualElements.Add(multiResponseListView);
                        currentTextRow.Add(multiResponseListView);
                        break;
                    default:
                        break;
                }
            }
            currentIndex = 0;
            ExtractImages(questionText.Files, parent, ref currentTextRow, question.PostText, ref currentIndex);
        }
        private static ListView CreateListView(VisualTreeAsset answerTemplate, List<Answer> clozeAnswers, bool isSingleQuestion)
        {
            ListView listView = new ListView();
            listView.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            listView.name = "answers";
            List<MultipleChoiceQuestionAnswerController> m_AnswerControllers = new List<MultipleChoiceQuestionAnswerController>();
            foreach (var answer in clozeAnswers)
            {
                m_AnswerControllers.Add(new MultipleChoiceQuestionAnswerController(new MultipleChoiceAnswer { Answertext = answer.Answertext, Feedback = answer.Feedback, Fraction = answer.Fraction }));
            }
            listView.makeItem = () =>
            {
                var newAnswer = answerTemplate.Instantiate();
                return newAnswer;
            };
            listView.bindItem = (item, index) =>
            {
                m_AnswerControllers[index].SetAnswerText(item);
                m_AnswerControllers[index].FillAnswerText();
                item.RegisterCallback<MouseDownEvent>((MouseDownEvent evt) => OnMouseDownEvent(evt, index, isSingleQuestion, m_AnswerControllers));
                item.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) => PlayButtonSound(), TrickleDown.TrickleDown);
            };
            listView.itemsSource = m_AnswerControllers;
            return listView;
        }
        private static void OnMouseDownEvent(MouseDownEvent evt, int answerControllerIndex, bool isSingleQuestion, List<MultipleChoiceQuestionAnswerController> m_AnswerControllers)
        {
            if (isSingleQuestion)
            {
                m_AnswerControllers[answerControllerIndex].SetCheckmark(true);
            }
            else
            {
                m_AnswerControllers[answerControllerIndex].ToggleCheckmark();
            }

        }
        override public VisualElement ConstructInteractionElement()
        {
            return null;
        }




        protected override void Grade()
        {
            m_Result = new QuizResult();
            for (int i = 0; i < m_Question.SubQuestions.Count; i++)
            {
                var question = m_Question.SubQuestions[i];
                var visualElement = m_SubQuestionVisualElements[i];
                m_Result.MaxPoints += question.Grade;
                switch (question.Type)
                {
                    case ClozeSubQuestion.ClozeType.SHORTANSWER:
                    case ClozeSubQuestion.ClozeType.SHORTANSWER_C:
                        TextField shortAnswerTextField = (visualElement as TextField);
                        shortAnswerTextField.SetEnabled(false);
                        string shortAnswer = shortAnswerTextField.value;

                        if (question.Type == ClozeSubQuestion.ClozeType.SHORTANSWER_C)
                            shortAnswer = shortAnswer.ToUpper();
                        float shortMaxFraction = float.MinValue;
                        foreach (var possibleSolution in question.Answers)
                        {
                            if (shortMaxFraction >= possibleSolution.Fraction)
                                continue;
                            if (possibleSolution.Answertext.Length != shortAnswer.Length)
                                continue;
                            string shortSolution = possibleSolution.Answertext;
                            if (question.Type == ClozeSubQuestion.ClozeType.SHORTANSWER_C)
                                shortSolution = shortSolution.ToUpper();
                            bool correct = true;
                            for (int j = 0; j < possibleSolution.Answertext.Length; j++)
                            {
                                var shortSolutionChar = possibleSolution.Answertext[j];
                                if (shortSolutionChar == '*')
                                    continue;
                                var shortGuessChar = shortAnswer[j];
                                if (shortSolutionChar != shortGuessChar)
                                {
                                    correct = false;
                                    break;
                                }
                            }
                            if (correct)
                                shortMaxFraction = possibleSolution.Fraction;
                        }
                        if (shortMaxFraction != float.MinValue)
                        {
                            m_Result.Points += (int)(shortMaxFraction / 100 * question.Grade);
                        }
                        if (shortMaxFraction > 0)
                        {
                            shortAnswerTextField.AddToClassList("correct-answer");
                        }
                        else
                        {
                            shortAnswerTextField.AddToClassList("false-answer");
                        }
                        break;
                    case ClozeSubQuestion.ClozeType.NUMERICAL:
                        TextField numericalTextField = (visualElement as TextField);
                        numericalTextField.SetEnabled(false);
                        if (!float.TryParse(numericalTextField.value, NumberStyles.Number, CultureInfo.InvariantCulture, out float numericalAnswer))
                        {
                            numericalTextField.AddToClassList("false-answer");
                            break;
                        }
                        float numericalMaxFraction = float.MinValue;
                        foreach (var possibleSolution in question.Answers)
                        {
                            if (numericalMaxFraction >= possibleSolution.Fraction)
                                continue;
                            var solutionAndTolerance = possibleSolution.Answertext.Split(':', 2);
                            if (solutionAndTolerance.Length != 2)
                                continue;
                            if (!float.TryParse(solutionAndTolerance[0], NumberStyles.Number, CultureInfo.InvariantCulture, out float numericalSolution) ||
                               !float.TryParse(solutionAndTolerance[1], NumberStyles.Number, CultureInfo.InvariantCulture, out float numericalTolerance))
                            {
                                continue;
                            }
                            if (Math.Abs(numericalSolution - numericalAnswer) <= numericalSolution)
                            {
                                numericalMaxFraction = possibleSolution.Fraction;
                            }
                        }
                        if (numericalMaxFraction != float.MinValue)
                        {
                            m_Result.Points += (int)(numericalMaxFraction / 100 * question.Grade);
                        }
                        if (numericalMaxFraction > 0)
                        {
                            numericalTextField.AddToClassList("correct-answer");
                        }
                        else
                        {
                            numericalTextField.AddToClassList("false-answer");
                        }
                        break;
                    case ClozeSubQuestion.ClozeType.MULTICHOICE:
                    case ClozeSubQuestion.ClozeType.MULTICHOICE_S:
                        DropdownField mulichoiceDropdownField = (visualElement as DropdownField);
                        mulichoiceDropdownField.SetEnabled(false);
                        int multichoicePoints = -1;
                        if (int.TryParse(mulichoiceDropdownField.value, out int multichoiceStringChoice))
                        {
                            multichoicePoints = (int)(question.Answers[multichoiceStringChoice].Fraction / 100 * question.Grade);
                            m_Result.Points += multichoicePoints;
                        }
                        if (multichoicePoints > 0)
                        {
                            mulichoiceDropdownField.AddToClassList("correct-answer");
                        }
                        else
                        {
                            mulichoiceDropdownField.AddToClassList("false-answer");
                        }
                        break;
                    case ClozeSubQuestion.ClozeType.MULTICHOICE_V:
                    case ClozeSubQuestion.ClozeType.MULTICHOICE_VS:
                    case ClozeSubQuestion.ClozeType.MULTICHOICE_H:
                    case ClozeSubQuestion.ClozeType.MULTICHOICE_HS:
                        RadioButtonGroup mulichoiceRadioButtonGroup = visualElement as RadioButtonGroup;
                        mulichoiceRadioButtonGroup.SetEnabled(false);
                        int multichoiceChoice = mulichoiceRadioButtonGroup.value;
                        int multichoiceRadioButtonGroupPoints = 0;
                        if (multichoiceChoice >= 0 && multichoiceChoice < question.Answers.Count)
                        {
                            multichoiceRadioButtonGroupPoints = (int)(question.Answers[multichoiceChoice].Fraction / 100 * question.Grade);
                            m_Result.Points += multichoiceRadioButtonGroupPoints;
                        }

                        if (multichoiceRadioButtonGroupPoints > 0)
                        {
                            mulichoiceRadioButtonGroup.AddToClassList("correct-answer");
                        }
                        else
                        {
                            mulichoiceRadioButtonGroup.AddToClassList("false-answer");
                        }
                        break;
                    case ClozeSubQuestion.ClozeType.MULTIRESPONSE:
                    case ClozeSubQuestion.ClozeType.MULTIRESPONSE_S:
                    case ClozeSubQuestion.ClozeType.MULTIRESPONSE_H:
                    case ClozeSubQuestion.ClozeType.MULTIRESPONSE_HS:
                        // we already added 1 for the general case
                        m_Result.MaxPoints += question.Answers.Count - 1;
                        ListView listView = visualElement as ListView;
                        listView.SetEnabled(false);
                        listView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;
                        var answerControllers = listView.itemsSource as List<MultipleChoiceQuestionAnswerController>;
                        foreach (var controller in answerControllers)
                        {
                            if (controller.GradeAndDisable())
                                m_Result.Points++;
                        }
                        break;
                    default:
                        break;
                }
                if (question.Type != ClozeSubQuestion.ClozeType.MULTIRESPONSE ||
                    question.Type != ClozeSubQuestion.ClozeType.MULTIRESPONSE_S ||
                    question.Type != ClozeSubQuestion.ClozeType.MULTIRESPONSE_H ||
                    question.Type != ClozeSubQuestion.ClozeType.MULTIRESPONSE_HS)
                {
                    AddFeedback(question, visualElement);
                }
            }
        }
        private void AddFeedback(ClozeSubQuestion question, VisualElement visualElement)
        {
            string feedback = "";
            for (int i = 0; i < question.Answers.Count; i++)
            {
                var answer = question.Answers[i];
                if (question.Type == ClozeSubQuestion.ClozeType.NUMERICAL)
                {
                    var solutionAndTolerance = answer.Answertext.Split(':', 2);
                    if (solutionAndTolerance.Length != 2)
                        continue;
                    if (!float.TryParse(solutionAndTolerance[0], NumberStyles.Number, CultureInfo.InvariantCulture, out float numericalSolution) ||
                       !float.TryParse(solutionAndTolerance[1], NumberStyles.Number, CultureInfo.InvariantCulture, out float numericalTolerance))
                    {
                        continue;
                    }
                    float minValue = numericalSolution - numericalTolerance;
                    float maxValue = numericalSolution + numericalTolerance;
                    feedback += $"{minValue} bis {maxValue}: ";
                }
                else
                {
                    feedback += $"{answer.Answertext}: ";
                }

                if (!string.IsNullOrEmpty(answer.Feedback))
                {
                    feedback += answer.Feedback;
                }
                else
                {
                    feedback += "Kein Feedback vorhanden";
                }
                if (i < question.Answers.Count - 1)
                    feedback += "\n";
            }
            visualElement.parent.Add(new Label(feedback));
        }
    }
}