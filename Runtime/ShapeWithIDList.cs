using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class ShapeWithIDList
    {
        public Shape Shape;
        public List<int> IDs = new List<int>();
        public List<int> FittingDragElementIDs = new List<int>();
        public bool IsSingleShot = false;
        public ShapeWithIDList(Shape shape, int id, int fittingDragElementID)
        {
            Shape = shape;
            IDs.Add(id);
            FittingDragElementIDs.Add(fittingDragElementID);
        }
        public void AddDrag(ShapeWithIDList shape)
        {
            IDs.Add(shape.IDs[0]);
            FittingDragElementIDs.Add(shape.FittingDragElementIDs[0]);
        }

    }
}