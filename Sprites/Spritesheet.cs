using System;

namespace ABSoftware.ABPixelEngine.Sprites
{
    public class Spritesheet
    {
        public Bitmap Sheet { get; private set; }
        public Frame[] frames;

        public int FramesX { get; private set; }
        public int FramesY { get; private set; }

        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }

        public Spritesheet(Bitmap bitmap, int framesX, int framesY, int frameWidth, int frameHeight)
        {
            this.Sheet = bitmap;
            this.frames = new Frame[framesX * framesY];

            this.FramesX = framesX;
            this.FramesY = framesY;
            this.FrameWidth = frameWidth;
            this.FrameHeight = frameHeight;

            for (int y = 0; y < framesY; y++)
            {
                for (int x = 0; x < framesX; x++)
                {
                    int index = y * framesX + x;

                    frames[index] = new Frame(this, x * frameWidth, y * frameHeight);
                }
            }
        }

        public Frame GetFrame(int x, int y)
        {
            return frames[y * FramesX + x];
        }

        public Frame GetFrame(int index)
        {
            return frames[index];
        }

        public Bitmap[] GenerateBitmapsFromFrames()
        {
            Bitmap[] bitmaps = new Bitmap[frames.Length];

            for(int i = 0; i < bitmaps.Length; i++)
            {
                Frame frame = frames[i];
                byte[] pixels = new byte[FrameWidth * FrameHeight];

                for (int y = 0; y < FrameHeight; y++)
                {
                    int sourceIndex = (frame.Y + y) * Sheet.Width + frame.X;

                    int targetIndex = y * FrameWidth;

                    Array.Copy(Sheet.Pixels, sourceIndex, pixels, targetIndex, FrameWidth);
                }

                bitmaps[i] = new Bitmap(FrameWidth, FrameHeight, Sheet.Palette, pixels);
            }

            return bitmaps;
        }
    }

    public class Frame
    {
        private Spritesheet spritesheet;

        public int X { get; private set; }
        public int Y { get; private set; }

        public int Width { get { return spritesheet.FrameWidth; } }
        public int Height { get { return spritesheet.FrameHeight; } }

        public Frame(Spritesheet spritesheet, int x, int y)
        {
            this.spritesheet = spritesheet;

            this.X = x;
            this.Y = y;
        }

        public Pixel GetPixel(int x, int y)
        {
            if (x >= 0 && x < spritesheet.FrameWidth && y >= 0 && y < spritesheet.FrameHeight)
                return spritesheet.Sheet.Palette[spritesheet.Sheet.Pixels[(this.Y + y) * spritesheet.Sheet.Width + (this.X + x)]];

            return Colors.Black;
        }

        public Pixel SampleClamp(float x, float y)
        {
            float u = Mathf.Clamp01(x);
            float v = Mathf.Clamp01(y);

            int sx = (int)(u * (spritesheet.FrameWidth - 1));
            int sy = (int)(v * (spritesheet.FrameHeight - 1));

            byte[] pixels = spritesheet.Sheet.Pixels;
            uint[] palette = spritesheet.Sheet.Palette;

            return palette[pixels[(this.Y + sy) * spritesheet.Sheet.Width + (this.X + sx)]];
        }

        public Pixel SampleRepeat(float x, float y)
        {
            float u = Mathf.RoundToNew01(x);
            float v = Mathf.RoundToNew01(y);

            int sx = (int)(u * spritesheet.FrameWidth);
            int sy = (int)(v * spritesheet.FrameHeight);

            if (sx >= spritesheet.FrameWidth) sx = spritesheet.FrameWidth - 1;
            if (sy >= spritesheet.FrameHeight) sy = spritesheet.FrameHeight - 1;

            byte[] pixels = spritesheet.Sheet.Pixels;
            uint[] palette = spritesheet.Sheet.Palette;

            return palette[pixels[(this.Y + sy) * spritesheet.Sheet.Width + (this.X + sx)]];
        }
    }
}
