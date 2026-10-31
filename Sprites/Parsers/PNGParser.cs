using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace ABSoftware.ABPixelEngine.Sprites
{
    public class PNGParser
    {
        public static PNGData LoadBitmap(string fileName)
        {
            PNGData pngData = new PNGData();

            using (FileStream fileStream = File.OpenRead(fileName))
            using (BinaryReader reader = new BinaryReader(fileStream))
            {
                byte[] signature = reader.ReadBytes(8);
                if (signature.Length < 8 ||
                    signature[0] != 0x89 || signature[1] != 0x50 || signature[2] != 0x4E || signature[3] != 0x47 ||
                    signature[4] != 0x0D || signature[5] != 0x0A || signature[6] != 0x1A || signature[7] != 0x0A)
                {
                    throw new Exception("Invalid png file");
                }

                using (MemoryStream idatCompressedStream = new MemoryStream())
                {
                    bool hasMoreChunks = true;

                    while (hasMoreChunks && fileStream.Position < fileStream.Length)
                    {
                        int length = ReadBigEndianInt32(reader);

                        byte[] typeBytes = reader.ReadBytes(4);
                        string chunkType = Encoding.ASCII.GetString(typeBytes);

                        switch(chunkType)
                        {
                            case "IHDR":
                                {
                                    pngData.Width = ReadBigEndianInt32(reader);
                                    pngData.Height = ReadBigEndianInt32(reader);
                                    pngData.bitDepth = reader.ReadByte();
                                    pngData.colorType = reader.ReadByte();

                                    reader.BaseStream.Seek(3, SeekOrigin.Current);

                                    if(pngData.bitDepth != 8)
                                    {
                                        throw new Exception("Only 8 bit depth is supported");
                                    }

                                    if(pngData.colorType != 6 && pngData.colorType != 2)
                                    {
                                        throw new Exception("Only RGB or RGBA is supported");
                                    }
                                }
                                break;
                            case "IDAT":
                                {
                                    byte[] idatData = reader.ReadBytes(length);
                                    idatCompressedStream.Write(idatData, 0, idatData.Length);
                                }
                                break;
                            case "IEND":
                                {
                                    hasMoreChunks = false;
                                    reader.BaseStream.Seek(length, SeekOrigin.Current);
                                }
                                break;
                            default:
                                {
                                    reader.BaseStream.Seek(length, SeekOrigin.Current);
                                }
                                break;
                        }

                        reader.BaseStream.Seek(4, SeekOrigin.Current);
                    }

                    if (idatCompressedStream.Length == 0)
                        throw new Exception("No IDAT chunks were found in PNG file");

                    idatCompressedStream.Position = 0;

                    idatCompressedStream.ReadByte();
                    idatCompressedStream.ReadByte();

                    using (DeflateStream deflate = new DeflateStream(idatCompressedStream, CompressionMode.Decompress))
                    using (MemoryStream uncompressedStream = new MemoryStream())
                    {
                        deflate.CopyTo(uncompressedStream);
                        byte[] rawPngData = uncompressedStream.ToArray();

                        byte[] rgbaPixels = DeconstructFilters(rawPngData, pngData.Width, pngData.Height, pngData.colorType);
                        uint[] rgbaData = new uint[rgbaPixels.Length / 4];
                        for (int i = 0; i < rgbaData.Length; i++)
                        {
                            int byteIdx = i * 4;

                            uint r = rgbaPixels[byteIdx];
                            uint g = rgbaPixels[byteIdx + 1];
                            uint b = rgbaPixels[byteIdx + 2];
                            uint a = rgbaPixels[byteIdx + 3];
                            rgbaData[i] = (a << 24) |
                                            (r << 16) |
                                            (g << 8) |
                                            b;
                        }
                        pngData.rgbaData = rgbaData;
                    }
                }
            }

            return pngData;
        }

        static int ReadBigEndianInt32(BinaryReader reader)
        {
            byte[] bytes = reader.ReadBytes(4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        static byte[] DeconstructFilters(byte[] rawPngData, int width, int height, byte colorType)
        {
            int bytesPerPixelInFile = (colorType == 6) ? 4 : 3;
            int strideInFile = width * bytesPerPixelInFile;
            int uncompressedStride = strideInFile + 1;

            const int outputBytesPerPixel = 4;
            int outputStride = width * outputBytesPerPixel;
            byte[] outputPixels = new byte[width * height * outputBytesPerPixel];

            for (int y = 0; y < height; y++)
            {
                int srcLineStart = y * uncompressedStride;
                byte filterType = rawPngData[srcLineStart];
                int dataStart = srcLineStart + 1;

                int destLineStart = y * outputStride;

                for (int x = 0; x < strideInFile; x++)
                {
                    byte currentByte = rawPngData[dataStart + x];

                    int pixelIndex = x / bytesPerPixelInFile;
                    int channelIndex = x % bytesPerPixelInFile;

                    byte left = (pixelIndex > 0)
                        ? outputPixels[destLineStart + (pixelIndex - 1) * outputBytesPerPixel + channelIndex]
                        : (byte)0;

                    byte up = (y > 0)
                        ? outputPixels[(y - 1) * outputStride + pixelIndex * outputBytesPerPixel + channelIndex]
                        : (byte)0;

                    byte upLeft = (y > 0 && pixelIndex > 0)
                        ? outputPixels[(y - 1) * outputStride + (pixelIndex - 1) * outputBytesPerPixel + channelIndex]
                        : (byte)0;

                    byte reconstructedByte;
                    switch (filterType)
                    {
                        case 0:
                            reconstructedByte = currentByte;
                            break;
                        case 1:
                            reconstructedByte = (byte)((currentByte + left) & 0xFF);
                            break;
                        case 2:
                            reconstructedByte = (byte)((currentByte + up) & 0xFF);
                            break;
                        case 3:
                            reconstructedByte = (byte)((currentByte + (left + up) / 2) & 0xFF);
                            break;
                        case 4:
                            reconstructedByte = (byte)((currentByte + PaethPredictor(left, up, upLeft)) & 0xFF);
                            break;
                        default:
                            throw new Exception($"Wrong filter type: {filterType}");
                    }

                    int destIndex = destLineStart + pixelIndex * outputBytesPerPixel;
                    outputPixels[destIndex + channelIndex] = reconstructedByte;

                    if (colorType == 2 && channelIndex == 2)
                    {
                        outputPixels[destIndex + 3] = 0xFF;
                    }
                }
            }

            return outputPixels;
        }

        private static byte PaethPredictor(byte a, byte b, byte c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);

            if (pa <= pb && pa <= pc) return a;
            if (pb <= pc) return b;
            return c;
        }
    }

    public class PNGData
    {
        public int Width;
        public int Height;
        public byte bitDepth;
        public byte colorType;
        public uint[] rgbaData;
    }
}
