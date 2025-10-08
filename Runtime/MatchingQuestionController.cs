using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class MatchingQuestionController : AbstractQuestionController<MatchingQuestion>
    {
        VisualElement m_DocumentRoot;
        VisualTreeAsset m_MatchingQuestionAnswerTemplate;
        VisualElement m_InteractionElement;
        List<MatchingQuestionAnswerController> m_AnswerControllers = new List<MatchingQuestionAnswerController>();

        public MatchingQuestionController(MatchingQuestion question, VisualElement root = null, VisualTreeAsset matchingQuestionTemplate = null, VisualTreeAsset matchingQuestionAnswerTemplate = null, VisualElement documentRoot = null)
        {
            m_Root = root;
            m_DocumentRoot = documentRoot;
            m_Question = question;
            m_MatchingQuestionAnswerTemplate = matchingQuestionAnswerTemplate;
            m_QuestionTemplate = matchingQuestionTemplate;
        }
        override public void RemoveFromRoot()
        {
            if (m_QuestionBackground != null && m_Root != null)
            {
                m_Root.Remove(m_QuestionBackground);
                foreach (var controller in m_AnswerControllers)
                {
                    controller.CleanUp();
                }

            }
        }
        override public ScrollView ConstructQuestionText()
        {
            if (m_QuestionText != null)
                return m_QuestionText;
            if (m_Root != null)
                m_QuestionText = m_Root.Q<ScrollView>("question-text");
            if (m_QuestionText == null)
            {
                m_QuestionText = new ScrollView();
            }
            QuizUtility.GenerateQuestionText(m_Question.Questiontext, m_QuestionText);
            return m_QuestionText;
        }
        override public VisualElement ConstructInteractionElement()
        {
            if (m_InteractionElement != null)
                return m_InteractionElement;
            List<string> answerChoices = m_Question.SubQuestions.Select(subquestion => subquestion.Answer).ToList();
            if (m_Question.ShuffleAnswers)
                m_Question.SubQuestions.Shuffle();
            foreach (var subquestion in m_Question.SubQuestions.Where(x => x.Text != ""))
            {
                if (m_Question.ShuffleAnswers)
                    answerChoices.Shuffle();
                m_AnswerControllers.Add(new MatchingQuestionAnswerController(subquestion, answerChoices, m_DocumentRoot));
            }
            ListView answers = null;
            if (m_Root != null)
                answers = m_Root.Q<ListView>("answers");
            if (answers == null)
            {
                answers = new ListView();
                answers.style.flexGrow = 1;
            }
            answers.makeItem = () =>
            {
                var newAnswer = m_MatchingQuestionAnswerTemplate.Instantiate();
                return newAnswer;
            };
            answers.bindItem = (item, index) =>
            {
                m_AnswerControllers[index].SetAnswerText(item, m_Root == null);
                m_AnswerControllers[index].FillAnswer();
            };
            //if (m_Question.ShuffleAnswers) 
            m_AnswerControllers.Shuffle();
            answers.itemsSource = m_AnswerControllers;
            return m_InteractionElement = answers;
        }

        protected override void Grade()
        {
            m_Result = new QuizResult();
            m_Result.MaxPoints = m_AnswerControllers.Count;
            foreach (var answerController in m_AnswerControllers)
            {
                if (answerController.GradeAndDisable())
                    m_Result.Points++;
            }
        }

        protected override void DisableQuestion()
        {
            m_SubmitButton.style.display = DisplayStyle.Flex;
            m_SubmitButton.clicked += ShowSolution;
            m_SubmitButton.text = "Lösung anzeigen";
        }
        private void ShowSolution()
        {
            m_SubmitButton.clicked -= ShowSolution;
            m_SubmitButton.clicked += ShowStudentAnswer;
            m_SubmitButton.text = "Eigene Eingabe anzeigen";
            foreach (var answerController in m_AnswerControllers)
            {
                answerController.ShowSolution();
            }
        }
        private void ShowStudentAnswer()
        {
            m_SubmitButton.clicked -= ShowStudentAnswer;
            m_SubmitButton.clicked += ShowSolution;
            m_SubmitButton.text = "Lösung anzeigen";
            foreach (var answerController in m_AnswerControllers)
            {
                answerController.ShowStudentAnswer();
            }
        }
    }
}