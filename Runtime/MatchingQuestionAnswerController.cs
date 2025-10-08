using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class MatchingQuestionAnswerController
    {
        VisualElement m_Root;
        SubQuestion m_Subquestion;
        VisualElement m_VisualAnswer;
        Label m_AnswerText;
        List<string> m_AnswerChoices;
        ListSelectElement listSelectElement;
        bool m_isCorrect = false;
        string m_studentAnswer = string.Empty;
        Button m_selectButton;
        public MatchingQuestionAnswerController(SubQuestion subQuestion, List<string> answerChoices, VisualElement documentRoot)
        {
            m_Subquestion = subQuestion;
            m_AnswerChoices = answerChoices;
            m_Root = documentRoot;
        }
        public void SetAnswerText(VisualElement answer, bool hideAnswerText = false)
        {
            m_VisualAnswer = answer;
            m_AnswerText = answer.Q<Label>("answer-text");
            m_selectButton = answer.Q<Button>("open-list-select");
            listSelectElement = new ListSelectElement(m_Root, m_selectButton, $"Wähle mit Doppelklick eine Aussage passend zu {m_Subquestion.Text} aus.");
            if (hideAnswerText)
            {
                m_AnswerText.style.display = DisplayStyle.None;
            }
        }
        public bool GradeAndDisable()
        {
            m_studentAnswer = listSelectElement.Value;
            m_isCorrect = (m_Subquestion.Answer == m_studentAnswer);
            listSelectElement.SetEnabled(false);
            listSelectElement.RemoveFromContext();
            if (m_isCorrect)
                m_VisualAnswer.Q<VisualElement>("answer-background").AddToClassList("correct-answer");
            else
                m_VisualAnswer.Q<VisualElement>("answer-background").AddToClassList("false-answer");
            m_VisualAnswer.RemoveFromClassList("unity-collection-view__item");
            m_VisualAnswer.RemoveFromClassList("unity-list-view__item");
            return m_isCorrect;
        }
        public void FillAnswer()
        {
            m_AnswerText.text = m_Subquestion.Text;
            listSelectElement.AnswerChoices = m_AnswerChoices;
        }
        public void CleanUp()
        {
            listSelectElement.RemoveFromContext();
        }
        public void ShowSolution()
        {
            m_selectButton.text = m_Subquestion.Answer;
            if (m_isCorrect)
                m_VisualAnswer.Q<VisualElement>("answer-background").ToggleInClassList("correct-answer");
            else
                m_VisualAnswer.Q<VisualElement>("answer-background").ToggleInClassList("false-answer");
            m_VisualAnswer.Q<VisualElement>("answer-background").ToggleInClassList("solution-answer");
        }
        public void ShowStudentAnswer()
        {
            if (m_isCorrect)
                m_VisualAnswer.Q<VisualElement>("answer-background").ToggleInClassList("correct-answer");
            else
                m_VisualAnswer.Q<VisualElement>("answer-background").ToggleInClassList("false-answer");
            m_VisualAnswer.Q<VisualElement>("answer-background").ToggleInClassList("solution-answer");
            m_selectButton.text = m_studentAnswer;
        }
    }
}