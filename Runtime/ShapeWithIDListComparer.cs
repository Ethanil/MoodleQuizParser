using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class ShapeWithIDListComparer : IEqualityComparer<ShapeWithIDList>
    {
        ShapeComparer m_Comparer = new ShapeComparer();
        public bool Equals(ShapeWithIDList x, ShapeWithIDList y)
        {
            return m_Comparer.Equals(x.Shape, y.Shape);
        }

        public int GetHashCode(ShapeWithIDList obj)
        {
            return m_Comparer.GetHashCode(obj.Shape);
        }
    }
}