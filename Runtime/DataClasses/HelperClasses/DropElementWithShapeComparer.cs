using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class DropElementWithShapeComparer : IEqualityComparer<DropElementWithShape>
    {
        ShapeComparer shapeComparer = new ShapeComparer();
        public bool Equals(DropElementWithShape x, DropElementWithShape y)
        {
            return shapeComparer.Equals(x.Shape, y.Shape);
        }

        public int GetHashCode(DropElementWithShape obj)
        {
            return shapeComparer.GetHashCode(obj.Shape);
        }
    }
}