using System;

namespace ABSoftware.ABPixelEngine
{
    public struct Mathf
    {
        public const float PI = 3.141592653589_79323846f;
        public const float PI2 = PI * 2f;
        public const float PIH = PI / 2f;
        public const float PISQ = PI * PI;
        public const float Rad2Deg = 180f / PI; 
        public const float Deg2Rad = PI / 180f;

        public static float Sinf(float x) => (float)Math.Sin(x);
        public static float Cosf(float x) => (float)Math.Cos(x);
        public static float Asinf(float x) => (float)Math.Asin(x);
        public static float Acosf(float x) => (float)Math.Acos(x);
        public static float Tanf(float x) => (float)Math.Tan(x);
        public static float Atanf(float x) => (float)Math.Atan(x);
        public static float Atan2f(float y, float x) => (float)Math.Atan2(y, x);

        public static float Abs(float x)
        {
            if (x < 0)
                return -x;

            return x;
        }

        public static float Max(float a, float b)
        {
            if (a > b)
                return a;

            return b;
        }

        public static float Min(float a, float b)
        {
            if (a < b)
                return a;

            return b;
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value > max) return max;
            if (value < min) return min;

            return value;
        }

        public static float Clamp01(float value)
        {
            if (value > 1f) return 1f;
            if (value < 0f) return 0f;

            return value;
        }

        public static int Abs(int x)
        {
            if (x < 0)
                return -x;

            return x;
        }

        public static int Max(int a, int b)
        {
            if (a > b)
                return a;

            return b;
        }

        public static int Min(int a, int b)
        {
            if (a < b)
                return a;

            return b;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value > max) return max;
            if (value < min) return min;

            return value;
        }

        public static float Lerp(float start, float end, float t) => start + (end - start) * t;
    }
}
