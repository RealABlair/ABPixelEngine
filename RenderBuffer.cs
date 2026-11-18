using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ABSoftware.ABPixelEngine
{
    public class RenderBuffer
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Pixel[] Pixels { get; private set; }
        public IntPtr PixelsAddress { get; private set; }
        public GCHandle PixelsHandle { get; private set; }

        public void Init(IntPtr pixelsAddress)
        {
            this.PixelsAddress = pixelsAddress;
        }

        public void SetPixelsHandle(GCHandle pixelsHandle)
        {
            this.PixelsHandle = pixelsHandle;
        }

        public RenderBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new Pixel[width * height];
        }

        public void Clear(Pixel color)
        {
            Pixels[0] = color;
            int copied = 1;
            while(copied < Pixels.Length)
            {
                int count = Math.Min(copied, Pixels.Length - copied);
                Array.Copy(Pixels, 0, Pixels, copied, count);
                copied *= 2;
            }
            /*for (int i = 0; i < Pixels.Length; i++)
                Pixels[i] = color;*/
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetPixel(int x, int y, Pixel pixel)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                Pixels[y * Width + x] = pixel;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Pixel GetPixel(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                return Pixels[y * Width + x];

            return Colors.Transparent;
        }

        public void Destroy()
        {
            if (PixelsHandle.IsAllocated)
                PixelsHandle.Free();
        }
        
        public static RenderBuffer Create(int width, int height)
        {
            RenderBuffer buffer = new RenderBuffer(width, height);

            buffer.PixelsHandle = GCHandle.Alloc(buffer.Pixels, GCHandleType.Pinned);

            buffer.PixelsAddress = buffer.PixelsHandle.AddrOfPinnedObject();

            return buffer;
        }
    }
}
