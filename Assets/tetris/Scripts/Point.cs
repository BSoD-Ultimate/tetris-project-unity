using System;
using System.Collections.Generic;
using System.Text;

namespace TetrisEngine
{
    public class Point : ICloneable
    {
        public int x { get; set; }
        public int y { get; set; }

        public Point()
        {
            x = 0;
            y = 0;
        }
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public object Clone()
        {
            return new Point(this.x, this.y);
        }
        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.x + p2.x, p1.y + p2.y);
        }
        public static Point operator -(Point p)
        {
            return new Point(-p.x, -p.y);
        }
        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.x - p2.x, p1.y - p2.y);
        }
        public static Point operator *(Point point, int factor)
        {
            return new Point(point.x * factor, point.y * factor);
        }
        public static Point operator /(Point point, int factor)
        {
            return new Point(point.x / factor, point.y / factor);
        }
    }

}
