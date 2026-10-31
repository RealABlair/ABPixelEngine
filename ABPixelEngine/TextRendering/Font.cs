using System;
using ABSoftware.ABPixelEngine.Sprites;
using ABSoftware.ABPixelEngine.Structures;

namespace ABSoftware.ABPixelEngine.TextRendering
{
    public class Font
    {
        public int CharacterWidth { get; private set; }
        public int CharacterHeight { get; private set; }
        public int Rows { get; private set; }
        public int Columns{ get; private set; }
        public int AtlasWidth { get; private set; }
        public int AtlasHeight { get; private set; }

        public byte[] FontMask { get; private set; }

        public int HorizontalSpacing { get; set; }
        public int VerticalSpacing { get; set; }

        public Font(int characterWidth, int characterHeight, int rows, int columns)
        {
            this.CharacterWidth = characterWidth;
            this.CharacterHeight = characterHeight;
            this.Rows = rows;
            this.Columns = columns;

            this.AtlasWidth = characterWidth * rows;
            this.AtlasHeight = characterHeight * columns;

            this.FontMask = new byte[AtlasWidth * AtlasHeight];
        }

        public Font(int characterWidth, int characterHeight, Bitmap bitmap, Pixel transparencyKey)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;

            this.CharacterWidth = characterWidth;
            this.CharacterHeight = characterHeight;
            this.Rows = width / characterWidth;
            this.Columns = height / characterHeight;

            this.AtlasWidth = characterWidth * this.Rows;
            this.AtlasHeight = characterHeight * this.Columns;

            this.FontMask = new byte[bitmap.Width * bitmap.Height];

            for(int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    FontMask[y * width + x] = bitmap.GetPixel(x, y) == transparencyKey ? (byte)0 : (byte)255;
                }
            }
        }

        public byte GetMaskAt(int x, int y)
        {
            return FontMask[y * AtlasWidth + x];
        }

        public void SetMaskAt(int x, int y, byte mask)
        {
            FontMask[y * AtlasWidth + x] = mask;
        }

        public int GetStringWidth(string text)
        {
            return text.Length * CharacterWidth + (text.Length - 1) * HorizontalSpacing;
        }

        public int GetStringHeight(string text)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++) if (text[i] == '\n') count++;

            return CharacterHeight * (count + 1) + VerticalSpacing * count;
        }

        public int2 GetStringSize(string text)
        {
            int count = 0;
            for (int i = 0; i < text.Length; i++) if (text[i] == '\n') count++;

            return (text.Length * CharacterWidth + (text.Length - 1) * HorizontalSpacing, CharacterHeight * (count + 1) + VerticalSpacing * count);
        }
    }
}
