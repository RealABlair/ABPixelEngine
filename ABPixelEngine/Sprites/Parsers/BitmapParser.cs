using System;
using System.IO;

namespace ABSoftware.ABPixelEngine.Sprites
{
    public class BitmapParser
    {
        public static BitmapData LoadBitmap(string fileName)
        {
            using(BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                ushort type = reader.ReadUInt16();
                if (type != 0x4D42)
                    throw new Exception("Invalid bmp file");

                reader.ReadUInt32();
                reader.ReadUInt16();
                reader.ReadUInt16();

                uint dataOffset = reader.ReadUInt32();

                uint size = reader.ReadUInt32();
                int width = reader.ReadInt32();
                int height = reader.ReadInt32();
                ushort planes = reader.ReadUInt16();
                ushort bitsPerPixel = reader.ReadUInt16();
                uint compression = reader.ReadUInt32();
                uint imageSize = reader.ReadUInt32();
                reader.ReadUInt32();
                reader.ReadUInt32();
                uint colorsUsed = reader.ReadUInt32();
                uint colorsImportant = reader.ReadUInt32();

                bool isFlipped = height > 0;
                height = Math.Abs(height);

                BitmapData data = new BitmapData()
                {
                    Width = width,
                    Height = height,
                    BitsPerPixel = bitsPerPixel
                };

                if (bitsPerPixel == 8)
                {
                    data.Palette = new uint[256];
                    for (int i = 0; i < 256; i++)
                    {
                        byte b = reader.ReadByte();
                        byte g = reader.ReadByte();
                        byte r = reader.ReadByte();
                        byte a = reader.ReadByte();

                        data.Palette[i] = (uint)((255 << 24) | (r << 16) | (g << 8) | b);
                    }

                    data.IndexedPixels = new byte[width * height];
                    reader.BaseStream.Position = dataOffset;

                    int padding = (4 - (width % 4)) % 4;

                    for (int y = 0; y < height; y++)
                    {
                        int targetY = isFlipped ? (height - 1 - y) : y;
                        int rowOffset = targetY * width;

                        for (int x = 0; x < width; x++)
                        {
                            data.IndexedPixels[rowOffset + x] = reader.ReadByte();
                        }
                        reader.BaseStream.Position += padding;
                    }
                }
                else if (bitsPerPixel == 24 || bitsPerPixel == 32)
                {
                    data.RgbaPixels = new uint[width * height];
                    reader.BaseStream.Position = dataOffset;

                    int bytesPerPixel = bitsPerPixel / 8;
                    int padding = (4 - ((width * bytesPerPixel) % 4)) % 4;

                    for (int y = 0; y < height; y++)
                    {
                        int targetY = isFlipped ? (height - 1 - y) : y;
                        int rowOffset = targetY * width;

                        for (int x = 0; x < width; x++)
                        {
                            byte b = reader.ReadByte();
                            byte g = reader.ReadByte();
                            byte r = reader.ReadByte();
                            byte a = (bytesPerPixel == 32) ? reader.ReadByte() : (byte)255;

                            data.RgbaPixels[rowOffset + x] = (uint)((a << 24) | (r << 16) | (g << 8) | b);
                        }
                        reader.BaseStream.Position += padding;
                    }
                }
                else
                    throw new Exception($"{bitsPerPixel} bit mode is not supported");

                return data;
            }
        }
    }

    public class BitmapData
    {
        public int Width;
        public int Height;
        public int BitsPerPixel;
        public uint[] Palette;
        public byte[] IndexedPixels;
        public uint[] RgbaPixels;
    }
}
