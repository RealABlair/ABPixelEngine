using System;

namespace ABSoftware.ABPixelEngine
{
    public class RenderBuffer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Pixel[] Pixels { get; private set; }
        public IntPtr PixelsAddress { get; private set; }

        public void Init(IntPtr pixelsAddress)
        {
            this.PixelsAddress = pixelsAddress;
        }

        public RenderBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new Pixel[width * height];
        }

        public void Clear(Pixel color)
        {
            for (int i = 0; i < Pixels.Length; i++)
                Pixels[i] = color;
        }

        public void SetPixel(int x, int y, Pixel pixel)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                Pixels[y * Width + x] = pixel.Color;
        }

        public Pixel GetPixel(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return Pixels[y * Width + x];

            return Colors.Transparent;
        }
    }
}
