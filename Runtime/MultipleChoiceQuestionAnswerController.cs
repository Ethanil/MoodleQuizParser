using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class MultipleChoiceQuestionAnswerController
    {
        MultipleChoiceAnswer m_Answer;
        VisualElement m_VisualAnswer;
        Label m_AnswerText;
        Label m_FeedbackText;
        BaseBoolField m_Toggle;
        bool m_IsGraded = false;
        public MultipleChoiceQuestionAnswerController(MultipleChoiceAnswer answer)
        {
            m_Answer = answer;
        }
        public void SetAnswerText(VisualElement answer)
        {
            m_VisualAnswer = answer;
            m_AnswerText = answer.Q<Label>("answer-text");
            m_Toggle = answer.Q<BaseBoolField>("toggle");
            m_FeedbackText = answer.Q<Label>("feedback-text");
        }

        public bool GradeAndDisable()
        {
            m_IsGraded = true;
            bool correct = (m_Answer.Fraction > 0 && m_Toggle.value) || (m_Answer.Fraction == 0 && !m_Toggle.value);
            m_Toggle.SetEnabled(false);
            m_FeedbackText.style.display = DisplayStyle.Flex;
            m_FeedbackText.text = m_Answer.Feedback;
            if (correct)
                m_VisualAnswer.Q<VisualElement>("answer-background").AddToClassList("correct-answer");
            else
                m_VisualAnswer.Q<VisualElement>("answer-background").AddToClassList("false-answer");
            m_VisualAnswer.RemoveFromClassList("unity-collection-view__item");
            m_VisualAnswer.RemoveFromClassList("unity-list-view__item");
            return correct;
        }

        public void FillAnswerText()
        {
            m_AnswerText.text = m_Answer.Answertext;
        }

        public void ToggleCheckmark()
        {
            if (m_IsGraded)
                return;
            m_Toggle.value = !m_Toggle.value;
        }
        public void SetCheckmark(bool value)
        {
            if (m_IsGraded)
                return;
            m_Toggle.value = value;
        }
    }
}