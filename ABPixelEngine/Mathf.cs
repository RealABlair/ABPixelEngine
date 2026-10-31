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
        public static int Floor(float x) => (int)Math.Floor(x);
        public static int Ceiling(float x) => (int)Math.Ceiling(x);
        public static int Round(float x) => (int)Math.Round(x);

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

        public static float RoundToNew(float min, float max, float value)
        {
            float range = max - min;

            float val = (value - min) % range;

            if (val < 0)
                val += range;

            return min + val;
        }

        public static float RoundToNew01(float value)
        {
            float val = value % 1f;

            if (val < 0)
                val += 1f;

            return val;
        }

        #region ints
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
        #endregion

        public static float Lerp(float start, float end, float t) => start + (end - start) * t;
        public static float InvLerp(float start, float end, float val) => (val - start) / (end - start);

        static readonly byte[] DeBruijnTable = new byte[] { 0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9 };
        public static int TrailingZeroCount(int value)
        {
            if (value == 0)
                return 32;

            return DeBruijnTable[(uint)((value & -value) * 0x077CB531U) >> 27];
        }
    }
}
