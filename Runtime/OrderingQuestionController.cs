using System.Collections.Generic;
using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{ 
    public class OrderingQuestionController : AbstractQuestionController<OrderingQuestion>
    {
        VisualElement m_InteractionElement;
        private class AnswerWithLabel
        {
            public Label Label;
            public OrderingAnswer Answer;
        }
        ListView m_Answers;
        ListView m_Solution;
        public OrderingQuestionController(OrderingQuestion question, VisualElement root = null, VisualTreeAsset orderingQuestionTemplate = null)
        {
            m_Root = root;
            m_Question = question;
            m_QuestionTemplate = orderingQuestionTemplate;
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
                m_Answers = m_Root.Q<ListView>("answers");
            if (m_Answers == null)
                m_Answers = new ListView();
            m_Answers.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) => PlayButtonSound(), TrickleDown.TrickleDown);
            m_Answers.RegisterCallback<PointerUpEvent>((PointerUpEvent evt) => PlayButtonSound(), TrickleDown.TrickleDown);
            m_Answers.makeItem = () =>
            {
                var label = new Label();
                //label.RegisterCallback<PointerDownEvent>((PointerDownEvent evt) => AudioManager.Instance.PlaySound(SoundID.ButtonSoundWithRandomPitch), TrickleDown.TrickleDown);
                return label;
            };
            List<AnswerWithLabel> answerList = new List<AnswerWithLabel>();
            m_Answers.bindItem = (item, index) =>
            {
                (item as Label).text = answerList[index].Answer.Answertext;
                answerList[index].Label = item as Label;
            };
            foreach (var answer in m_Question.Answers)
            {
                AnswerWithLabel a = new AnswerWithLabel();
                a.Answer = answer;
                answerList.Add(a);
            }
            answerList.Shuffle();
            m_Answers.itemsSource = answerList;

            m_Solution = new ListView();
            m_Solution.name = "answers";
            m_Solution.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
            m_Solution.selectionType = SelectionType.None;
            m_Solution.showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly;
            m_Solution.makeItem = () =>
            {
                return new Label();
            };
            List<AnswerWithLabel> solutionList = new List<AnswerWithLabel>();
            m_Solution.bindItem = (item, index) =>
            {
                (item as Label).text = m_Question.Answers[index].Answertext;
                item.AddToClassList("solution-answer");
            };
            foreach (var answer in m_Question.Answers)
            {
                AnswerWithLabel a = new AnswerWithLabel();
                a.Answer = answer;
                solutionList.Add(a);
            }
            m_Solution.itemsSource = m_Question.Answers;
            return m_InteractionElement = m_Answers;
        }


        protected override void Grade()
        {
            m_Result = new QuizResult();
            m_Result.MaxPoints = m_Question.Answers.Count;
            for (int i = 0; i < m_Answers.itemsSource.Count; i++)
            {
                AnswerWithLabel answer = m_Answers.itemsSource[i] as AnswerWithLabel;
                if (answer.Answer.Fraction == i + 1)
                {
                    m_Result.Points++;
                    answer.Label.AddToClassList("correct-answer");
                }
                else
                {
                    answer.Label.AddToClassList("false-answer");
                }
            }
        }
        protected override void DisableQuestion()
        {
            m_Answers.reorderable = false;
            m_SubmitButton.style.display = DisplayStyle.Flex;
            m_SubmitButton.clicked += ShowSolution;
            m_SubmitButton.text = "Lösung anzeigen";
        }

        private void ShowSolution()
        {
            if (m_SubmitButton != null)
            {
                m_SubmitButton.clicked -= ShowSolution;
                m_SubmitButton.text = "Eigene Eingabe anzeigen";
                m_SubmitButton.clicked += ShowPlayerSolution;
            }
            m_Answers.parent.Add(m_Solution);
            m_Answers.parent.Remove(m_Answers);

        }

        private void ShowPlayerSolution()
        {
            if (m_SubmitButton != null)
            {
                m_SubmitButton.clicked -= ShowPlayerSolution;
                m_SubmitButton.text = "Lösung anzeigen";
                m_SubmitButton.clicked += ShowSolution;
            }
            m_Solution.parent.Add(m_Answers);
            m_Solution.parent.Remove(m_Solution);

        }

    }
}