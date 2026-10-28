using System;

namespace ABSoftware.ABPixelEngine.Structures
{
    /// <summary>
    /// A structure containing two int fields
    /// </summary>
    public struct float2
    {
        public float x;
        public float y;

        public float2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return $"x: {x}, y: {y}";
        }

        public static implicit operator float2((float, float) tuple) => new float2(tuple.Item1, tuple.Item2);

        public static float2 operator +(float2 left, float2 right) => new float2(left.x + right.x, left.y + right.y);
        public static float2 operator -(float2 left, float2 right) => new float2(left.x - right.x, left.y - right.y);
        public static float2 operator *(float2 left, float2 right) => new float2(left.x * right.x, left.y * right.y);
        public static float2 operator /(float2 left, float2 right) => new float2(left.x / right.x, left.y / right.y);
    }
}
