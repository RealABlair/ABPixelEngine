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

        public Pixel(uint argb) : this()
        {
            /*this.R = 0;
            this.G = 0;
            this.B = 0;
            this.A = 0;*/
            Color = argb;
        }

        public Pixel(byte r, byte g, byte b, byte a = 255) : this()
        {
            this.Color = 0;
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public Pixel(float r, float g, float b, float a = 1.0f) : this()
        {
            this.Color = 0;
            this.R = (byte)(r * 255f);
            this.G = (byte)(g * 255f);
            this.B = (byte)(b * 255f);
            this.A = (byte)(a * 255f);
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

        public Pixel Lerp(Pixel to, float t)
        {
            t = Mathf.Clamp01(t);
            byte r = (byte)Mathf.Lerp(R, to.R, t);
            byte g = (byte)Mathf.Lerp(G, to.G, t);
            byte b = (byte)Mathf.Lerp(B, to.B, t);
            byte a = (byte)Mathf.Lerp(A, to.A, t);

            return new Pixel(r, g, b, a);
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
        public static Pixel operator *(Pixel left, Pixel right) 
        {
            float lr = left.R / 255f, lg = left.G / 255f, lb = left.B / 255f, la = left.A / 255f;
            float rr = right.R / 255f, rg = right.G / 255f, rb = right.B / 255f, ra = right.A / 255f;

            return new Pixel(lr * rr, lg * rg, lb * rb, la * ra);
        }

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
        public static Pixel Black => new Pixel(0xFF000000);
        public static Pixel White => new Pixel(0xFFFFFFFF);
        public static Pixel Red => new Pixel(0xFFFF0000);
        public static Pixel Blue => new Pixel(0xFF0000FF);
        public static Pixel Green => new Pixel(0xFF00FF00);
        public static Pixel Transparent => new Pixel(0);
        public static Pixel Gray => new Pixel(0xFF808080);
        public static Pixel LightGray => new Pixel(0xFFD3D3D3);
        public static Pixel DarkGray => new Pixel(0xFFA9A9A9);
        public static Pixel Yellow => new Pixel(0xFFFFFF00);
        public static Pixel Cyan => new Pixel(0xFF00FFFF);
        public static Pixel Magenta => new Pixel(0xFFFF00FF);
        public static Pixel Orange => new Pixel(0xFFFFA500);
        public static Pixel Purple => new Pixel(0xFF800080);
        public static Pixel Pink => new Pixel(0xFFFFC0CB);
        public static Pixel Lime => new Pixel(0xFF00FF00);
        public static Pixel Teal => new Pixel(0xFF008080);
        public static Pixel Navy => new Pixel(0xFF000080);
        public static Pixel Brown => new Pixel(0xFFA52A2A);
        public static Pixel Maroon => new Pixel(0xFF800000);
        public static Pixel Olive => new Pixel(0xFF808000);
        public static Pixel Gold => new Pixel(0xFFFFD700);
        public static Pixel Silver => new Pixel(0xFFC0C0C0);
        public static Pixel Crimson => new Pixel(0xFFDC143C);
        public static Pixel DarkRed => new Pixel(0xFF8B0000);
        public static Pixel Firebrick => new Pixel(0xFFB22222);
        public static Pixel IndianRed => new Pixel(0xFFCD5C5C);
        public static Pixel Salmon => new Pixel(0xFFFA8072);
        public static Pixel Coral => new Pixel(0xFFFF7F50);
        public static Pixel DeepPink => new Pixel(0xFFFF1493);
        public static Pixel HotPink => new Pixel(0xFFFF69B4);
        public static Pixel LightPink => new Pixel(0xFFFFB6C1);
        public static Pixel DarkOrange => new Pixel(0xFFFF8C00);
        public static Pixel Tomato => new Pixel(0xFFFF6347);
        public static Pixel CoralOrange => new Pixel(0xFFFF4500);
        public static Pixel Khaki => new Pixel(0xFFF0E68C);
        public static Pixel DarkKhaki => new Pixel(0xFFBDB76B);
        public static Pixel Moccasin => new Pixel(0xFFFFE4B5);
        public static Pixel PeachPuff => new Pixel(0xFFFFDAB9);
        public static Pixel DarkGreen => new Pixel(0xFF001000);
        public static Pixel ForestGreen => new Pixel(0xFF228B22);
        public static Pixel SeaGreen => new Pixel(0xFF2E8B57);
        public static Pixel OliveDrab => new Pixel(0xFF6B8E23);
        public static Pixel YellowGreen => new Pixel(0xFF9ACD32);
        public static Pixel GreenYellow => new Pixel(0xFFADFF2F);
        public static Pixel LawnGreen => new Pixel(0xFF7CFC00);
        public static Pixel Chartreuse => new Pixel(0xFF7FFF00);
        public static Pixel SpringGreen => new Pixel(0xFF00FF7F);
        public static Pixel SkyBlue => new Pixel(0xFF87CEEB);
        public static Pixel DeepSkyBlue => new Pixel(0xFF00BFFF);
        public static Pixel LightBlue => new Pixel(0xFFADD8E6);
        public static Pixel RoyalBlue => new Pixel(0xFF4169E1);
        public static Pixel MidnightBlue => new Pixel(0xFF191970);
        public static Pixel Cornflower => new Pixel(0xFF6495ED);
        public static Pixel SteelBlue => new Pixel(0xFF4682B4);
        public static Pixel CadetBlue => new Pixel(0xFF5F9EA0);
        public static Pixel Violet => new Pixel(0xFFEE82EE);
        public static Pixel Orchid => new Pixel(0xFFDA70D6);
        public static Pixel Plum => new Pixel(0xFFDDA0DD);
        public static Pixel Thistle => new Pixel(0xFFD8BFD8);
        public static Pixel Lavender => new Pixel(0xFFE6E6FA);
        public static Pixel Indigo => new Pixel(0xFF4B0082);
        public static Pixel DarkMagenta => new Pixel(0xFF8B008B);
        public static Pixel DarkViolet => new Pixel(0xFF9400D3);
        public static Pixel Chocolate => new Pixel(0xFFD2691E);
        public static Pixel SaddleBrown => new Pixel(0xFF8B4513);
        public static Pixel Sienna => new Pixel(0xFFA0522D);
        public static Pixel Peru => new Pixel(0xFFCD853F);
        public static Pixel SandyBrown => new Pixel(0xFFF4A460);
        public static Pixel BurlyWood => new Pixel(0xFFDEB887);
        public static Pixel Tan => new Pixel(0xFFD2B48C);
        public static Pixel Beige => new Pixel(0xFFF5F5DC);
        public static Pixel Wheat => new Pixel(0xFFF5DEB3);
        public static Pixel Ivory => new Pixel(0xFFFFFFF0);
        public static Pixel Linen => new Pixel(0xFFFAF0E6);
        public static Pixel OldLace => new Pixel(0xFFFDF5E6);
        public static Pixel AntiqueWhite => new Pixel(0xFFFAEBD7);
        public static Pixel MintCream => new Pixel(0xFFF5FFFA);
        public static Pixel Azure => new Pixel(0xFFF0FFFF);
        public static Pixel AliceBlue => new Pixel(0xFFF0F8FF);
        public static Pixel GhostWhite => new Pixel(0xFFF8F8FF);
        public static Pixel DimGray => new Pixel(0xFF696969);
        public static Pixel SlateGray => new Pixel(0xFF708090);
        public static Pixel DarkSlateGray => new Pixel(0xFF2F4F4F);
        public static Pixel Charcoal => new Pixel(0xFF36454F);
        public static Pixel Gunmetal => new Pixel(0xFF2A3439);
    }
}
