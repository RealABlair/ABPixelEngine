using System;
using System.Runtime.InteropServices;

namespace ABSoftware.ABPixelEngine
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Pixel
    {
        [FieldOffset(0)]
        public readonly uint Color;

        [FieldOffset(2)]
        public readonly byte R;
        [FieldOffset(1)]
        public readonly byte G;
        [FieldOffset(0)]
        public readonly byte B;
        [FieldOffset(3)]
        public readonly byte A;

        public Pixel WithR(byte r) => new Pixel(r, G, B, A);
        public Pixel WithG(byte g) => new Pixel(R, g, B, A);
        public Pixel WithB(byte b) => new Pixel(R, G, b, A);
        public Pixel WithA(byte a) => new Pixel(R, G, B, a);

        public Pixel(uint argb)
        {
            this.R = 0;
            this.G = 0;
            this.B = 0;
            this.A = 0;
            Color = argb;
        }

        public Pixel(byte r, byte g, byte b, byte a = 255)
        {
            this.Color = 0;
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public Pixel Blend(Pixel background)
        {
            if (this.A == 255) return this;
            if (this.A == 0) return background;

            int src = this.A;

            int r = (this.R * src + background.R * (255 - src)) >> 8;
            int g = (this.G * src + background.G * (255 - src)) >> 8;
            int b = (this.B * src + background.B * (255 - src)) >> 8;

            return new Pixel((byte)r, (byte)g, (byte)b, 255);
        }

        public Pixel Invert() => new Pixel((byte)(255 - R), (byte)(255 - G), (byte)(255 - B), A);

        public Pixel Illuminate(float factor)
        {
            if (factor < 0f) factor = 0f;

            int r = (int)(R * factor);
            int g = (int)(G * factor);
            int b = (int)(B * factor);

            if (r > 255) r = 255;
            if (g > 255) g = 255;
            if (b > 255) b = 255;

            return new Pixel((byte)r, (byte)g, (byte)b, A);
        }

        public static Pixel Lerp(Pixel from, Pixel to, float t)
        {
            t = Mathf.Clamp01(t);
            byte r = (byte)Mathf.Lerp(from.R, to.R, t);
            byte g = (byte)Mathf.Lerp(from.G, to.G, t);
            byte b = (byte)Mathf.Lerp(from.B, to.B, t);
            byte a = (byte)Mathf.Lerp(from.A, to.A, t);

            return new Pixel(r, g, b, a);
        }

        public static bool operator ==(Pixel left, Pixel right) => left.Color == right.Color;
        public static bool operator !=(Pixel left, Pixel right) => left.Color != right.Color;

        public static implicit operator Pixel(uint color) => new Pixel(color);
        public static implicit operator uint(Pixel pixel) => pixel.Color;

        public override int GetHashCode() => Color.GetHashCode();
        public override bool Equals(object obj) => obj is Pixel p && p.Color == Color;
        public override string ToString()
        {
            return $"R: {R}, G: {G}, B: {B}, A: {A} (0x{Color:X8})";
        }
    }

    public static class Colors
    {
        public static Pixel Black       => new Pixel(0xFF000000);
        public static Pixel White       => new Pixel(0xFFFFFFFF);
        public static Pixel Red         => new Pixel(0xFFFF0000);
        public static Pixel Blue        => new Pixel(0xFF0000FF);
        public static Pixel Green       => new Pixel(0xFF00FF00);
        public static Pixel Transparent => new Pixel(0);
    }
}
