using UnityEngine.UIElements;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public interface IQuestionController
    {
        public void GradeAndDisableQuestion();
        public QuizResult GetResult();
        public void AddToRoot();
        public void RemoveFromRoot();
        public ScrollView ConstructQuestionText();
        public VisualElement ConstructInteractionElement();
    }
}