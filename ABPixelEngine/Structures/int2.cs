using System;

namespace ABSoftware.ABPixelEngine.Structures
{
    /// <summary>
    /// A structure containing two int fields
    /// </summary>
    public struct int2
    {
        public int x;
        public int y;

        public int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return $"x: {x}, y: {y}";
        }

        public static implicit operator int2((int, int) tuple) => new int2(tuple.Item1, tuple.Item2);

        public static int2 operator +(int2 left, int2 right) => new int2(left.x + right.x, left.y + right.y);
        public static int2 operator -(int2 left, int2 right) => new int2(left.x - right.x, left.y - right.y);
        public static int2 operator *(int2 left, int2 right) => new int2(left.x * right.x, left.y * right.y);
        public static int2 operator /(int2 left, int2 right) => new int2(left.x / right.x, left.y / right.y);
    }
}
