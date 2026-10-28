using System;

namespace ABSoftware.ABPixelEngine.Sprites
{
    public class Sprite
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Pixel[] Pixels { get; private set; }

        public Sprite(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Pixels = new Pixel[width * height];
        }

        public Sprite(string fileName, FileType fileType)
        {
            
        }

        public Sprite(int width, int height, Pixel[] pixels)
        {
            this.Width = width;
            this.Height = height;
            this.Pixels = pixels;
        }

        public Pixel GetPixel(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return Pixels[y * Width + x];

            return Colors.Transparent;
        }

        public Pixel SampleClamp(float x, float y)
        {
            float u = Mathf.Clamp01(x);
            float v = Mathf.Clamp01(y);

            int sx = (int)(u * (Width - 1));
            int sy = (int)(v * (Height - 1));

            return Pixels[sy * Width + sx];
        }

        public Pixel SampleRepeat(float x, float y)
        {
            float u = Mathf.RoundToNew01(x);
            float v = Mathf.RoundToNew01(y);

            int sx = (int)(u * Width);
            int sy = (int)(v * Height);

            if (sx >= Width) sx = Width - 1;
            if (sy >= Height) sy = Height - 1;

            return Pixels[sy * Width + sx];
        }

        public static explicit operator Bitmap(Sprite sprite)
        {
            Pixel[] sPix = sprite.Pixels;
            uint[] sPixRaw = new uint[sPix.Length];
            for (int i = 0; i < sPix.Length; i++)
                sPixRaw[i] = sPix[i];

            uint[] palette = Palettes.CreateAdaptivePalette(sPixRaw);
            byte[] pixels = Palettes.IndexImage(sPixRaw, palette);

            Bitmap b = new Bitmap(sprite.Width, sprite.Height, palette, pixels);

            return b;
        }
    }
}
