using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class CalculatedMultiQuestionController : AbstractQuestionController<CalculatedQuestion>
    {
        VisualTreeAsset m_AnswerTemplate;
        VisualTreeAsset m_HelpDialogTemplate;
        VisualElement m_HelpBackground;
        VisualElement m_DocumentRoot;
        VisualElement m_InteractionElement;
        Dictionary<string, double> m_Variables;
        List<MultipleChoiceQuestionAnswerController> m_AnswerControllers = new List<MultipleChoiceQuestionAnswerController>();

        public CalculatedMultiQuestionController(CalculatedQuestion question, VisualElement root = null, VisualTreeAsset multipleChoiceQuestionTemplate = null, VisualTreeAsset multipleChoiceAnswerTemplate = null, VisualTreeAsset singleChoiceAnswerTemplate = null, VisualTreeAsset helpDialogTemplate = null)
        {
            m_Root = root;
            m_Question = question;
            if (m_Question.Single)
            {
                m_AnswerTemplate = singleChoiceAnswerTemplate;
            }
            else
            {
                m_AnswerTemplate = multipleChoiceAnswerTemplate;
            }

            m_QuestionTemplate = multipleChoiceQuestionTemplate;
            m_HelpDialogTemplate = helpDialogTemplate;
        }

        override public ScrollView ConstructQuestionText()
        {
            if (m_QuestionText != null)
                return m_QuestionText;
            if (m_Variables == null)
                m_Variables = QuizUtility.GenerateRandomVariables(m_Question.DatasetDefinitions);
            if (m_Root != null)
                m_QuestionText = m_Root.Q<ScrollView>("question-text");
            if (m_QuestionText == null)
                m_QuestionText = new ScrollView();
            foreach (var variablePair in m_Variables)
            {
                m_Question.Questiontext.Text = m_Question.Questiontext.Text.Replace("{" + variablePair.Key + "}", variablePair.Value.ToString());
            }
            QuizUtility.GenerateQuestionText(m_Question.Questiontext, m_QuestionText);
            return m_QuestionText;

        }
        override public VisualElement ConstructInteractionElement()
        {
            if (m_InteractionElement != null)
                return m_InteractionElement;
            if (m_Variables == null)
                m_Variables = QuizUtility.GenerateRandomVariables(m_Question.DatasetDefinitions);
            foreach (var answer in m_Question.Answers)
            {
                MultipleChoiceAnswer a = new MultipleChoiceAnswer();
                a.Answertext = QuizUtility.EvaluateEqualBracketsAndReplaceVariables(answer.Answertext, m_Variables);
                a.Feedback = answer.Feedback;
                a.Fraction = answer.Fraction;
                m_AnswerControllers.Add(new MultipleChoiceQuestionAnswerController(a));
            }
            ListView answers = null;
            if (m_Root != null)
                answers = m_Root.Q<ListView>("answers");
            if (answers == null)
                answers = new ListView();
            answers.Q<ScrollView>().mouseWheelScrollSize = 400;
            answers.makeItem = () =>
            {
                var newAnswer = m_AnswerTemplate.Instantiate();
                return newAnswer;
            };
            answers.bindItem = (item, index) =>
            {
                m_AnswerControllers[index].SetAnswerText(item);
                m_AnswerControllers[index].FillAnswerText();
                item.RegisterCallback<MouseDownEvent>((MouseDownEvent evt) => OnMouseDownEvent(evt, index));
                item.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) => ActionManager.OnButtonPressed?.Invoke(), TrickleDown.TrickleDown);
            };
            if (m_Question.ShuffleAnswers)
                m_AnswerControllers.Shuffle();
            answers.itemsSource = m_AnswerControllers;
            return m_InteractionElement = answers;
        }

        override protected void ShowHelp()
        {
            if (m_DocumentRoot == null)
                m_DocumentRoot = m_Root.panel.visualTree;
            if (m_HelpBackground == null)
            {
                m_HelpDialogTemplate.CloneTree(m_DocumentRoot);
                m_HelpBackground = m_DocumentRoot.Q<VisualElement>("help-background");
                m_HelpBackground.AddManipulator(new Clickable(evt => m_DocumentRoot.Remove(m_HelpBackground)));
                var helpText = m_HelpBackground.Q<Label>("help-text");
                var correctAnswers = m_Question.Answers.Where(ans => ans.Fraction > 0).ToList().Count;
                helpText.text = String.Format("Genau {0} Antwort{1} korrekt.", correctAnswers, correctAnswers == 1 ? " ist" : "en sind");
            }
            else
            {
                m_DocumentRoot.Add(m_HelpBackground);
            }


        }

        protected override void FillFeedback()
        {
            m_Question.GeneralFeedback.Text = QuizUtility.EvaluateEqualBracketsAndReplaceVariables(m_Question.GeneralFeedback.Text, m_Variables);
            base.FillFeedback();
        }
        protected override void Grade()
        {
            m_Result = new QuizResult();
            m_Result.MaxPoints = m_Question.Answers.Count;
            foreach (var controller in (m_InteractionElement as ListView).itemsSource)
            {
                if ((controller as MultipleChoiceQuestionAnswerController).GradeAndDisable())
                    m_Result.Points++;
            }
        }
        protected override void DisableQuestion()
        {
            base.DisableQuestion();
            (m_InteractionElement as ListView).showAlternatingRowBackgrounds = AlternatingRowBackground.None;
        }


        private void OnMouseDownEvent(MouseDownEvent evt, int answerControllerIndex)
        {
            if (m_Question.Single)
            {
                m_AnswerControllers[answerControllerIndex].SetCheckmark(true);
            }
            else
            {
                m_AnswerControllers[answerControllerIndex].ToggleCheckmark();
            }

        }
    }
}