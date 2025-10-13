using System;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class ShortAnswerQuestionController : AbstractQuestionController<ShortQuestion>
    {
        VisualElement m_InteractionElement;
        bool m_ignoreCase;

        public ShortAnswerQuestionController(ShortQuestion question, VisualElement root = null, VisualTreeAsset shortAnswerQuestionTemplate = null, bool ignoreCase = true)
        {
            m_Root = root;
            m_Question = question;
            m_ignoreCase = ignoreCase;
            m_QuestionTemplate = shortAnswerQuestionTemplate;
        }

        override public ScrollView ConstructQuestionText()
        {
            if (m_QuestionText != null)
                return m_QuestionText;
            if (m_Root != null)
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
            if (m_Root != null)
                m_InteractionElement = m_Root.Q<TextField>("answer");
            if (m_InteractionElement == null)
                m_InteractionElement = new TextField();
            m_InteractionElement.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) => ActionManager.OnButtonPressed?.Invoke(), TrickleDown.TrickleDown);
            return m_InteractionElement;
        }
        protected override void Grade()
        {
            TextField answer = m_InteractionElement as TextField;
            m_Result = new QuizResult();
            m_Result.MaxPoints = (int)m_Question.Defaultgrade;
            double max_percentage = 0;
            string answerFeedback = "";
            foreach (var possibleSolution in m_Question.Answers)
            {
                if (!string.IsNullOrEmpty(possibleSolution.Feedback))
                    answerFeedback += possibleSolution.Feedback + "\n";
                if (possibleSolution.Fraction < max_percentage || possibleSolution.Answertext.Length != answer.text.Length)
                    continue;
                bool correct = true;
                for (int i = 0; i < possibleSolution.Answertext.Length; i++)
                {
                    var solution = possibleSolution.Answertext[i];
                    if (solution == '*')
                        continue;
                    var guess = answer.text[i];
                    if (m_ignoreCase)
                    {
                        solution = Char.ToLower(solution);
                        guess = Char.ToLower(guess);
                    }
                    if (solution != guess)
                    {
                        correct = false;
                        break;
                    }


                }
                if (correct)
                    max_percentage = possibleSolution.Fraction;
            }
            m_Result.Points = (int)(m_Result.MaxPoints * (max_percentage / 100));
            if (max_percentage > 0)
            {
                answer.parent.AddToClassList("correct-answer");
            }
            else
            {
                answer.parent.AddToClassList("false-answer");
            }
            if (!string.IsNullOrEmpty(answerFeedback))
            {
                Label feedback = answer.parent.Q<Label>("feedback");
                feedback.text = answerFeedback;
                feedback.style.display = DisplayStyle.Flex;
            }
        }
        protected override void DisableQuestion()
        {
            base.DisableQuestion();
            (m_InteractionElement as TextField).SetEnabled(false);
        }

    }
}