using System;
using System.IO;

namespace ABSoftware.ABPixelEngine.Sprites
{
    public class Bitmap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public uint[] Palette { get; private set; }
        public byte[] Pixels { get; private set; }

        public Bitmap(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }

        public Bitmap(string fileName, FileType fileType, PaletteType paletteType = PaletteType.Fast)
        {
            LoadFrom(fileName, fileType, paletteType);
        }

        public Bitmap(int width, int height, uint[] palette, byte[] pixels)
        {
            this.Width = width;
            this.Height = height;
            this.Palette = palette;
            this.Pixels = pixels;
        }

        public void LoadFrom(string fileName, FileType fileType, PaletteType paletteType)
        {
            switch(fileType)
            {
                case FileType.BMP:
                    {
                        BitmapData data = BitmapParser.LoadBitmap(fileName);

                        if(data.BitsPerPixel == 8)
                        {
                            Width = data.Width;
                            Height = data.Height;

                            Palette = data.Palette;
                            Pixels = data.IndexedPixels;
                        }
                        else
                        {
                            Width = data.Width;
                            Height = data.Height;

                            Palette = paletteType == PaletteType.Fast ? Palettes.FastPalette : Palettes.CreateAdaptivePalette(data.RgbaPixels);
                            Pixels = Palettes.IndexImage(data.RgbaPixels, Palette);
                        }
                    }
                    break;
            }
        }

        public Pixel GetPixel(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return Palette[Pixels[y * Width + x]];

            return Colors.Black;
        }

        public Pixel SampleClamp(float x, float y)
        {
            float u = Mathf.Clamp01(x);
            float v = Mathf.Clamp01(y);

            int sx = (int)(u * (Width - 1));
            int sy = (int)(v * (Height - 1));

            byte[] pixels = Pixels;
            uint[] palette = Palette;

            return palette[pixels[sy * Width + sx]];
        }

        public Pixel SampleRepeat(float x, float y)
        {
            float u = Mathf.RoundToNew01(x);
            float v = Mathf.RoundToNew01(y);

            int sx = (int)(u * Width);
            int sy = (int)(v * Height);

            if (sx >= Width) sx = Width - 1;
            if (sy >= Height) sy = Height - 1;

            byte[] pixels = Pixels;
            uint[] palette = Palette;

            return palette[pixels[sy * Width + sx]];
        }

        public static explicit operator Sprite(Bitmap bitmap)
        {
            Sprite s = new Sprite(bitmap.Width, bitmap.Height);

            Pixel[] sPix = s.Pixels;
            byte[] bPix = bitmap.Pixels;
            uint[] bPal = bitmap.Palette;
            
            for(int i = 0; i < bitmap.Pixels.Length; i++)
            {
                sPix[i] = bPal[bPix[i]];
            }

            return s;
        }
    }

    public enum FileType
    {
        RAW,
        BMP,
        ABPESPR
    }

    public enum PaletteType
    {
        Fast,
        Adaptive
    }
}
