
using System;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    abstract public class AbstractQuestionController<T> : IQuestionController where T : Question
    {

        protected VisualElement m_Root;
        protected VisualElement m_QuestionBackground;
        protected Button m_SubmitButton;
        protected QuizResult m_Result;
        protected VisualTreeAsset m_QuestionTemplate;
        protected ScrollView m_QuestionText;
        protected Label m_FeedbackText;
        protected T m_Question;
        public bool GotAddedToRootAtLeastOnce = false;
        /// <summary>
        /// A hook for the user to provide their own data saving logic.
        /// Passes the Question ID (int) and the Result (QuizResult).
        /// </summary>
        public static Action<int, QuizResult> OnSaveResult;

        /// <summary>
        /// A hook for the user to connect to their own event or message system.
        /// Passes the Result (QuizResult).
        /// </summary>
        public static Action<QuizResult> OnQuizGraded;

        /// <summary>
        /// A hook for the user to connect to their Audiosystem.
        /// </summary>
        public static Action OnButtonPressed;
        public void AddToRoot()
        {
            GotAddedToRootAtLeastOnce = true;
            if (m_QuestionBackground == null)
            {
                m_QuestionTemplate.CloneTree(m_Root);
                m_QuestionBackground = m_Root.Q<VisualElement>("question-background");
                m_SubmitButton = m_Root.Q<Button>("submit-question-button");
                m_SubmitButton.style.display = DisplayStyle.None;
                var helpButton = m_Root.Q<Button>("help-button");
                if (helpButton != null)
                {
                    helpButton.clicked += ShowHelp;
                }
                m_FeedbackText = m_Root.Q<Label>("general-feedback-text");
                FillQuestionWithData();
            }
            else
            {
                m_Root.Add(m_QuestionBackground);
            }
        }
        public virtual void RemoveFromRoot()
        {
            if (m_QuestionBackground != null && m_Root != null)
            {
                m_Root.Remove(m_QuestionBackground);
            }
        }
        protected virtual void FillQuestionWithData()
        {
            ConstructQuestionText();
            ConstructInteractionElement();
        }
        protected virtual void ShowHelp() { }
        public abstract VisualElement ConstructInteractionElement();
        public abstract ScrollView ConstructQuestionText();
        public virtual QuizResult GetResult()
        {
            if (!GotAddedToRootAtLeastOnce)
            {
                return new QuizResult { NotEditedQuestion = true };
            }
            if (m_Result == null)
                GradeAndDisableQuestion();
            return m_Result;
        }

        /*OnGrade draus machen und Grade so machen
         * Grade(){
         *  OnGrade();
         *  Save();
         * }
         * 
         * */
        void GradeAndSave()
        {
            Grade();
            OnSaveResult?.Invoke(m_Question.QuestionID, m_Result);
            OnQuizGraded?.Invoke(m_Result);
        }

        public static void PlayButtonSound()
        {
            OnButtonPressed?.Invoke();
        }
        protected abstract void Grade();

        protected virtual void FillFeedback()
        {
            if (m_FeedbackText != null)
            {
                m_QuestionText?.Add(m_FeedbackText);
                m_FeedbackText.AddToClassList("general-feedback-text");
                m_FeedbackText.style.display = DisplayStyle.Flex;
                QuizUtility.GenerateFeedbackText(m_Question.GeneralFeedback, m_FeedbackText, m_Result);
            }
        }
        protected virtual void DisableQuestion()
        {
            if (m_Root != null)
            {
                m_SubmitButton.SetEnabled(false);
                var helpButton = m_Root.Q<Button>("help-button");
                if (helpButton != null)
                {
                    helpButton.SetEnabled(false);
                }
            }
        }
        public virtual void GradeAndDisableQuestion()
        {
            GradeAndSave();
            FillFeedback();
            DisableQuestion();
        }
    }
}