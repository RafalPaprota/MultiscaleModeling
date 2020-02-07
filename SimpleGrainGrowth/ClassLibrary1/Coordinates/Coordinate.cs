using System.Collections.Generic;
using System.Drawing;

namespace Grains.Library.Unites
{
    public static class Unites
    {
        public static Point[] MooreCoordinates =>
           new Point[]
           {
                new Point(-1, -1),
                new Point(0, -1),
                new Point(1, -1),
                new Point(-1, 0),
                new Point(1, 0),
                new Point(-1, 1),
                new Point(0, 1),
                new Point(1, 1),
           };

        public static Point[] VonNeumannCoordinates =>
          new Point[]
          {
                new Point(0, -1),
                new Point(-1, 0),
                new Point(1, 0),
                new Point(0, 1)
          };

        public static Point[] InvertedVonNeumannCoordinates =>
         new Point[]
         {
                new Point(-1, -1),
                new Point(1, -1),
                new Point(-1, 1),
                new Point(1, 1),
         };

        public static Point[] WidenMooreCoordinates(int size)
        {
            var points = new List<Point>();

            for (int i = -1 - size; i < 1 + size; i++)
            {
                points.Add(new Point(i, -1 - size));
                points.Add(new Point(i, 1 + size));
            }

            for (int j = -1 - size; j < 1 + size; j++)
            {
                points.Add(new Point(-1 - size, j));
                points.Add(new Point(1 + size, j));
            }

            return points.ToArray();
        }
    }
}
