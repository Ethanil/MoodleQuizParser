using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class QuizController
    {
        QuestionControllerFactory m_QuestionFactory;
        List<IQuestionController> m_Questions = new List<IQuestionController>();
        List<IQuestionController> m_SubmittedQuestions = new List<IQuestionController>();
        Button m_SkipQuestionButton;
        Button m_SubmitQuestionButton;
        Button m_NextQuestionButton;
        Button m_CloseQuizButton;
        int m_CurrentQuestionIndex = -1;
        public VisualElement Root;
        VisualElement m_QuizContent;

        Label m_QuestionCountLabel;
        private bool m_IsUnskippable = false;

        public QuizController(VisualElement root)
        {
            this.Root = root;
            Initialize();
        }

        internal void RegisterButtonCallbacks()
        {
            m_SubmitQuestionButton = Root.Q<Button>("submit-question-button_");
            m_SkipQuestionButton = Root.Q<Button>("skip-question-button");
            m_NextQuestionButton = Root.Q<Button>("next-question-button");
            m_CloseQuizButton = Root.Q<Button>("close-quiz-button");
            m_SubmitQuestionButton.clicked += SubmitQuestion;
            m_SkipQuestionButton.clicked += NextQuestion;
            m_NextQuestionButton.clicked += NextQuestion;
            m_CloseQuizButton.clicked += CloseQuiz;
        }

        private void NextQuestion()
        {
            // Cleanup old Question
            if (m_CurrentQuestionIndex >= 0 && m_CurrentQuestionIndex < m_Questions.Count)
                m_Questions[m_CurrentQuestionIndex].RemoveFromRoot();
            m_CurrentQuestionIndex++;
            // All Questions done
            if (m_CurrentQuestionIndex >= m_Questions.Count)
            {
                m_SkipQuestionButton.style.display = DisplayStyle.None;
                m_SubmitQuestionButton.style.display = DisplayStyle.None;
                m_NextQuestionButton.style.display = DisplayStyle.None;
                m_CloseQuizButton.style.display = DisplayStyle.Flex;
                m_QuestionCountLabel.style.display = DisplayStyle.None;
                return;
            }
            // Add new Question
            if (m_CurrentQuestionIndex >= 0 && m_CurrentQuestionIndex < m_Questions.Count)
                m_Questions[m_CurrentQuestionIndex].AddToRoot();

            // Enable or Disble Skip Button
            if (m_IsUnskippable)
            {
                m_SkipQuestionButton.style.display = DisplayStyle.None;
                m_SkipQuestionButton.SetEnabled(false);
            }
            else
            {
                m_SkipQuestionButton.style.display = DisplayStyle.Flex;
                m_SkipQuestionButton.SetEnabled(true);
            }
            m_SubmitQuestionButton.style.display = DisplayStyle.Flex;
            m_NextQuestionButton.style.display = DisplayStyle.None;
            m_CloseQuizButton.style.display = DisplayStyle.None;

            m_QuestionCountLabel.style.display = DisplayStyle.Flex;
            m_QuestionCountLabel.text = $"{m_CurrentQuestionIndex}/{m_Questions.Count}";
        }

        private void SubmitQuestion()
        {
            m_Questions[m_CurrentQuestionIndex].GradeAndDisableQuestion();
            m_SubmittedQuestions.Add(m_Questions[m_CurrentQuestionIndex]);
            m_SkipQuestionButton.SetEnabled(false);
            m_SubmitQuestionButton.style.display = DisplayStyle.None;
            m_NextQuestionButton.style.display = DisplayStyle.Flex;
        }
        public void Initialize()
        {
            m_QuizContent = Root.Q<VisualElement>("quizcontent");
            m_QuestionCountLabel = Root.Q<Label>("question-count-label");
            m_QuestionFactory = new QuestionControllerFactory(m_QuizContent, Root);

            RegisterButtonCallbacks();
        }

        public void CloseQuiz()
        {
            List<QuizResult> results = new();
            foreach (var question in m_SubmittedQuestions)
            {
                results.Add(question.GetResult());
            }
            m_QuizContent.Clear();
            m_Questions.Clear();
            m_SubmittedQuestions.Clear();
            m_CurrentQuestionIndex = -1;
        }


        public void StartQuiz(List<Question> listOfQuestions, bool unskippable)
        {
            foreach (var question in listOfQuestions)
            {
                var controller = m_QuestionFactory.Create(question);
                if (controller != null)
                {
                    m_Questions.Add(controller);
                }
            }
            m_IsUnskippable = unskippable;
            NextQuestion();
        }

    }
    public class QuestionControllerFactory
    {
        VisualTreeAsset
            m_MultipleChoiceQuestionTemplate,
            m_MultipleChoiceAnswerTemplate,
            m_SingleChoiceAnswerTemplate,
            m_DragAndDropQuestionTemplate,
            m_DragAndDropMarkersQuestionTemplate,
            m_MatchingQuestionTemplate,
            m_MatchingQuestionAnswerTemplate,
            m_HelpDialogTemplate,
            m_CalculatedQuestionTemplate,
            m_OrderingQuestionTemplate,
            m_ClozeQuestionTemplate;
        private readonly VisualElement m_QuizContent;
        private readonly VisualElement m_Root;

        public QuestionControllerFactory(VisualElement quizContent, VisualElement root)
        {
            m_MultipleChoiceQuestionTemplate = Resources.Load<VisualTreeAsset>("UXML-Files/Quiz/MultipleChoiceQuestion");
            m_MultipleChoiceAnswerTemplate = Resources.Load<VisualTreeAsset>("UXML-Files/Quiz/MultipleChoiceAnswer");
            m_SingleChoiceAnswerTemplate = Resources.Load<VisualTreeAsset>("UXML-Files/Quiz/SingleChoiceAnswer");
            m_DragAndDropQuestionTemplate = Resources.Load<VisualTreeAsset>("UXML-Files/Quiz/DragAndDropQuestion");
            m_DragAndDropMarkersQuestionTemplate = Resources.Load<VisualTreeAsset>("UXML-Files/Quiz/DragAndDropMarkersQuestion");
            m_MatchingQuestionTemplate = Resources.Load<VisualTreeAsset>("UXML-Files/Quiz/MatchingQuestion");
            m_MatchingQuestionAnswerTemplate = Resources.Load<VisualTreeAsset>("UXML-Files/Quiz/MatchingAnswer");
            m_HelpDialogTemplate = Resources.Load<VisualTreeAsset>("UXML-Files/Quiz/HelpDialog");
            m_CalculatedQuestionTemplate = Resources.Load<VisualTreeAsset>("UXML-Files/Quiz/CalculatedQuestion");
            m_OrderingQuestionTemplate = Resources.Load<VisualTreeAsset>("UXML-Files/Quiz/OrderingQuestion");
            m_ClozeQuestionTemplate = Resources.Load<VisualTreeAsset>("UXML-Files/Quiz/ClozeQuestion");
            m_QuizContent = quizContent;
            m_Root = root;
        }

        public IQuestionController Create(Question question)
        {
            var questionType = question.GetType();
            if (questionType == typeof(MultipleChoiceQuestion))
            {
                return new MultipleChoiceQuestionController(question as MultipleChoiceQuestion, m_QuizContent, m_MultipleChoiceQuestionTemplate, m_MultipleChoiceAnswerTemplate, m_SingleChoiceAnswerTemplate, m_HelpDialogTemplate);
            }
            else if (questionType == typeof(DragAndDropQuestion))
            {
                return new DragAndDropQuestionController(question as DragAndDropQuestion, m_QuizContent, m_DragAndDropQuestionTemplate);
            }
            else if (questionType == typeof(TrueFalseQuestion))
            {
                var q = question as TrueFalseQuestion;
                MultipleChoiceQuestion tempQuestion = new MultipleChoiceQuestion();
                tempQuestion.Answers = q.Answers;
                foreach (var answer in tempQuestion.Answers)
                {
                    if (answer.Answertext == "true")
                        answer.Answertext = "Wahr";
                    else
                        answer.Answertext = "Falsch";
                }
                tempQuestion.Name = q.Name;
                tempQuestion.Questiontext = q.Questiontext;
                tempQuestion.GeneralFeedback = q.GeneralFeedback;
                tempQuestion.Defaultgrade = q.Defaultgrade;
                tempQuestion.Penalty = q.Penalty;
                tempQuestion.Hidden = q.Hidden;
                tempQuestion.IdNumber = q.IdNumber;
                tempQuestion.Tags = q.Tags;
                tempQuestion.Single = true;
                tempQuestion.QuestionID = q.QuestionID;
                return new MultipleChoiceQuestionController(tempQuestion, m_QuizContent, m_MultipleChoiceQuestionTemplate, m_MultipleChoiceAnswerTemplate, m_SingleChoiceAnswerTemplate, m_HelpDialogTemplate);
            }
            else if (questionType == typeof(MatchingQuestion))
            {
                return new MatchingQuestionController(question as MatchingQuestion, m_QuizContent, m_MatchingQuestionTemplate, m_MatchingQuestionAnswerTemplate, m_Root);

            }
            else if (questionType == typeof(DragAndDropMarkersQuestion))
            {
                return new DragAndDropMarkersQuestionController(question as DragAndDropMarkersQuestion, m_QuizContent, m_DragAndDropMarkersQuestionTemplate);
            }
            else if (questionType == typeof(CalculatedQuestion))
            {
                return new CalculatedMultiQuestionController(question as CalculatedQuestion, m_QuizContent, m_MultipleChoiceQuestionTemplate, m_MultipleChoiceAnswerTemplate, m_SingleChoiceAnswerTemplate, m_HelpDialogTemplate);
            }
            else if (questionType == typeof(CalculatedQuestionWithUnit))
            {
                return new CalculatedQuestionController(question as CalculatedQuestion, m_QuizContent, m_CalculatedQuestionTemplate);
            }
            else if (questionType == typeof(NumericQuestion))
            {
                return new CalculatedQuestionController(QuizUtility.ConvertToCalculatedQuestion(question as NumericQuestion), m_QuizContent, m_CalculatedQuestionTemplate);
            }
            else if (questionType == typeof(OrderingQuestion))
            {
                return new OrderingQuestionController(question as OrderingQuestion, m_QuizContent, m_OrderingQuestionTemplate);
            }
            else if (questionType == typeof(ShortQuestion))
            {
                return new ShortAnswerQuestionController(question as ShortQuestion, m_QuizContent, m_CalculatedQuestionTemplate);
            }
            else if (questionType == typeof(ClozeQuestion))
            {
                return new ClozeQuestionController(question as ClozeQuestion, m_QuizContent, m_ClozeQuestionTemplate, m_MultipleChoiceAnswerTemplate);
            }
            else
            {
                Debug.Log($"Found unsupported Questiontype {questionType}, skipping this question");
                return null;
            }
        }
    }
}