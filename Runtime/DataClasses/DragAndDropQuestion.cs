using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class DragAndDropQuestion : BaseMultipleQuestion
    {
        /// <value>Property <c>File</c> represents the Image used in this Question.</value>
        public MoodleFile File;
        /// <value>Property <c>Drags</c> represents the List of Drag-Elements of this Question.</value>
        public List<DragElement> Drags = new List<DragElement>();
        /// <value>Property <c>Drops</c> represents the List of Drop-Elements of this Question.</value>
        public List<DropElement> Drops = new List<DropElement>();

        public override int CalculateMaxPoints()
        {
            int res = 0;
            foreach (var drag in Drags)
            {
                int numberOfDragElements = 1;
                if (drag.GetType() == typeof(DragElementWithMaxUses))
                {
                    numberOfDragElements = (drag as DragElementWithMaxUses).NumberOfDrags;
                }
                res += numberOfDragElements;
            }
            return res;
        }
    }
}