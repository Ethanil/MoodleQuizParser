using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class DragAndDropMarkersQuestion : BaseMultipleQuestion
    {
        /// <value>Property <c>DragBoxes</c> represents the DragBoxes which can be Dragged into the Text.</value>
        public List<DragBox> DragBoxes = new List<DragBox>();
        public override int CalculateMaxPoints()
        {
            return DragBoxes.Count;
        }
    }
}