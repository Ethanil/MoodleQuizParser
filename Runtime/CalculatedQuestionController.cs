using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class CalculatedQuestionController : AbstractQuestionController<CalculatedQuestion>
    {
        VisualElement m_InteractionElement;
        Dictionary<string, double> m_Variables;

        public CalculatedQuestionController(CalculatedQuestion question, VisualElement root = null, VisualTreeAsset calculatedQuestionTemplate = null)
        {
            m_Root = root;
            m_Question = question;

            m_QuestionTemplate = calculatedQuestionTemplate;
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
            if (m_Root != null)
                m_InteractionElement = m_Root.Q<TextField>("answer");
            if (m_InteractionElement == null)
                m_InteractionElement = new TextField();
            m_InteractionElement.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) => ActionManager.OnButtonPressed?.Invoke(), TrickleDown.TrickleDown);
            return m_InteractionElement;
        }
        protected override void FillFeedback()
        {
            m_Question.GeneralFeedback.Text = QuizUtility.EvaluateEqualBracketsAndReplaceVariables(m_Question.GeneralFeedback.Text, m_Variables);
            base.FillFeedback();
        }
        protected override void Grade()
        {
            TextField answer = m_InteractionElement as TextField;
            m_Result = new QuizResult();
            m_Result.MaxPoints = (int)m_Question.Defaultgrade;
            double max_percentage = 0;
            double answerValue = 0;
            bool answerIsDouble = true;
            string answerFeedback = "";
            try
            {
                answerValue = Double.Parse(answer.text);
            }
            catch
            {
                answerIsDouble = false;
            }
            if (answerIsDouble)
            {
                foreach (var possibleSolution in m_Question.Answers)
                {
                    if (!string.IsNullOrEmpty(possibleSolution.Feedback))
                        answerFeedback += possibleSolution.Feedback + "\n";
                    if (possibleSolution.Fraction < max_percentage)
                        continue;
                    var res = QuizUtility.Evaluate(possibleSolution.Answertext, m_Variables);
                    if (Math.Abs(res - answerValue) <= possibleSolution.Tolerance)
                        max_percentage = possibleSolution.Fraction;
                }
            }
            else
            {
                foreach (var possibleSolution in m_Question.Answers)
                {
                    if (!string.IsNullOrEmpty(possibleSolution.Feedback))
                        answerFeedback += possibleSolution.Feedback + "\n";
                }

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
                answerFeedback = answerFeedback.Trim();
                Label feedback = answer.parent.Q<Label>("feedback");
                if (feedback == null)
                {
                    feedback = new Label();
                    answer.parent.Add(feedback);
                }
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