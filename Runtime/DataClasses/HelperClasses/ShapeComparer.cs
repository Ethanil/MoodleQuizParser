using System;
using System.Collections.Generic;

namespace TUDarmstadt.SeriousGames.MoodleQuizParser
{
    public class ShapeComparer : IEqualityComparer<Shape>
    {
        public bool Equals(Shape x, Shape y)
        {
            if (ReferenceEquals(x, y))
                return true;
            if (x is null || y is null)
                return false;
            var shapeTypeX = x.GetType();
            var shapeTypeY = y.GetType();
            if (shapeTypeX != shapeTypeY)
                return false;
            if (shapeTypeX == typeof(Rectangle))
            {
                var rectangle1 = (Rectangle)x;
                var rectangle2 = (Rectangle)y;
                return rectangle1.TopLeft.X == rectangle2.TopLeft.X &&
                    rectangle1.TopLeft.Y == rectangle2.TopLeft.Y &&
                    rectangle1.Width == rectangle2.Width &&
                    rectangle1.Height == rectangle2.Height;
            }
            else if (shapeTypeX == typeof(Circle))
            {
                var circle1 = (Circle)x;
                var circle2 = (Circle)y;
                return circle1.Radius == circle2.Radius &&
                    circle1.Center.X == circle2.Center.X &&
                    circle1.Center.Y == circle2.Center.Y;
            }
            throw new NotImplementedException();
        }

        public int GetHashCode(Shape obj)
        {
            var shapeType = obj.GetType();
            if (shapeType == typeof(Rectangle))
            {
                var rect = (Rectangle)obj;
                return rect.Height ^ rect.Width ^ rect.TopLeft.X ^ rect.TopLeft.Y;
            }
            else if (shapeType == typeof(Circle))
            {
                var circle = (Circle)obj;
                return circle.Radius ^ circle.Center.X ^ circle.Center.Y;
            }
            else
            {
                return 0;
            }
        }
    }
}