using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class StreamlinedDrops
    {
        HashSet<ShapeWithIDList> m_Shapes = new HashSet<ShapeWithIDList>(new ShapeWithIDListComparer());
        public StreamlinedDrops(List<DropElement> drops)
        {
            bool combined_dropZones = false;
            foreach (var drop in drops)
            {
                Shape shape;
                if (drop.GetType() == typeof(DropElementWithShape))
                {
                    shape = ((DropElementWithShape)drop).Shape;
                }
                else
                {
                    var d = (DropElementWithoutShape)drop;
                    var rect = new Rectangle();
                    rect.TopLeft = new Point(d.XLeft, d.YTop);
                    rect.Width = 200;
                    rect.Height = 80;
                    shape = rect;
                }
                ShapeWithIDList shapeWithIDList = new ShapeWithIDList(shape, drop.Choice, drop.Number);
                if (!m_Shapes.Add(shapeWithIDList))
                {
                    ShapeWithIDList r;
                    m_Shapes.TryGetValue(shapeWithIDList, out r);
                    r.AddDrag(shapeWithIDList);
                    combined_dropZones = true;
                }
            }
            if (!combined_dropZones)
            {
                foreach (var s in m_Shapes)
                {
                    s.IsSingleShot = true;
                }
            }
        }
        public HashSet<ShapeWithIDList> GetShapes() { return m_Shapes; }
    }
}