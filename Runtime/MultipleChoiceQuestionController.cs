using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class MultipleChoiceQuestionController : AbstractQuestionController<MultipleChoiceQuestion>
    {
        VisualTreeAsset m_AnswerTemplate;
        VisualTreeAsset m_HelpDialogTemplate;
        VisualElement m_HelpBackground;
        VisualElement m_DocumentRoot;
        VisualElement m_InteractionElement;
        List<MultipleChoiceQuestionAnswerController> m_AnswerControllers = new List<MultipleChoiceQuestionAnswerController>();

        public MultipleChoiceQuestionController(MultipleChoiceQuestion question, VisualElement root = null, VisualTreeAsset multipleChoiceQuestionTemplate = null, VisualTreeAsset multipleChoiceAnswerTemplate = null, VisualTreeAsset singleChoiceAnswerTemplate = null, VisualTreeAsset helpDialogTemplate = null)
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
            m_QuestionText = m_Root.Q<ScrollView>("question-text");
            if (m_QuestionText == null)
                m_QuestionText = new ScrollView();
            QuizUtility.GenerateQuestionText(m_Question.Questiontext, m_QuestionText);

            return m_QuestionText;

        }
        override public VisualElement ConstructInteractionElement()
        {
            if (m_InteractionElement != null)
                return m_InteractionElement;
            ListView answers = null;
            if (m_Root != null)
                answers = m_Root.Q<ListView>("answers");
            if (answers == null)
                answers = new ListView();
            answers.Q<ScrollView>().mouseWheelScrollSize = 400;
            foreach (var answer in m_Question.Answers)
            {
                m_AnswerControllers.Add(new MultipleChoiceQuestionAnswerController(answer));
            }
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
                item.RegisterCallback<MouseDownEvent>((MouseDownEvent evt) => ActionManager.OnButtonPressed?.Invoke(), TrickleDown.TrickleDown);
            };
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

        override protected void Grade()
        {
            m_Result = new QuizResult();
            m_Result.MaxPoints = m_Question.Answers.Count;
            var answers = m_InteractionElement as ListView;
            foreach (var controller in answers.itemsSource)
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