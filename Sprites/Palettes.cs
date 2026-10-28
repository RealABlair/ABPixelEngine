using System;

namespace ABSoftware.ABPixelEngine.Sprites
{
    public static class Palettes
    {
        public static readonly uint[] FastPalette = CreateFastPalette();

        static uint[] CreateFastPalette()
        {
            uint[] palette = new uint[256];
            int index = 0;

            for(int r = 0; r < 6; r++)
            {
                for (int g = 0; g < 6; g++)
                {
                    for (int b = 0; b < 6; b++)
                    {
                        byte red = (byte)(r * 51);
                        byte green = (byte)(g * 51);
                        byte blue = (byte)(b * 51);
                        palette[index++] = (uint)((255 << 24) | (red << 16) | (green << 8) | (blue));
                    }
                }
            }

            for(int i = 0; i < 40; i++)
            {
                byte gray = (byte)(i * 6.5641f);
                palette[index++] = (uint)((255 << 24) | (gray << 16) | (gray << 8) | (gray));
            }

            return palette;
        }

        public static uint[] CreateAdaptivePalette(uint[] rawRGBAPixels)
        {
            int totalPixels = rawRGBAPixels.Length;

            //r, g, b
            (byte, byte, byte)[] colors = new (byte, byte, byte)[totalPixels];

            for (int i = 0; i < totalPixels; i++)
            {
                uint c = rawRGBAPixels[i];
                colors[i] = ((byte)((c >> 16) & 0xFF), (byte)((c >> 8) & 0xFF), (byte)(c & 0xFF));
            }

            if (totalPixels <= 256)
            {
                uint[] smallPalette = new uint[256];
                for (int i = 0; i < 256; i++)
                {
                    if (i < totalPixels)
                        smallPalette[i] = (uint)((255 << 24) | (colors[i].Item1 << 16) | (colors[i].Item2 << 8) | colors[i].Item3);
                    else
                        smallPalette[i] = 0xFF000000;
                }
                return smallPalette;
            }

            //start, count
            (int, int)[] cubes = new (int, int)[256];
            cubes[0] = (0, totalPixels);
            int cubesCount = 1;

            while(cubesCount < 256)
            {
                int targetCubeIndex = 0;
                int maxCount = 0;

                for(int i = 0; i < cubesCount; i++)
                {
                    if(cubes[i].Item2 > maxCount)
                    {
                        maxCount = cubes[i].Item2;
                        targetCubeIndex = i;
                    }
                }

                (int, int) cubeToSplit = cubes[targetCubeIndex];

                int minR = 255, maxR = 0, minG = 255, maxG = 0, minB = 255, maxB = 0;
                int end = cubeToSplit.Item1 + cubeToSplit.Item2;

                for (int i = cubeToSplit.Item1; i < end; i++)
                {
                    (byte, byte, byte) c = colors[i];
                    if (c.Item1 < minR) minR = c.Item1; if (c.Item1 > maxR) maxR = c.Item1;
                    if (c.Item2 < minG) minG = c.Item2; if (c.Item2 > maxG) maxG = c.Item2;
                    if (c.Item3 < minB) minB = c.Item3; if (c.Item3 > maxB) maxB = c.Item3;
                }

                float rRange = (maxR - minR) * 0.299f;
                float gRange = (maxG - minG) * 0.587f;
                float bRange = (maxB - minB) * 0.114f;

                byte channel = 2;
                if (rRange >= gRange && rRange >= bRange) channel = 0;
                else if (gRange >= rRange && gRange >= bRange) channel = 1;

                QuickSort(colors, cubeToSplit.Item1, end - 1, channel);

                int median = cubeToSplit.Item2 / 2;

                cubes[targetCubeIndex] =  (cubeToSplit.Item1, median);
                cubes[cubesCount] = (cubeToSplit.Item1 + median, cubeToSplit.Item2 - median);
                cubesCount++;
            }

            uint[] palette = new uint[256];
            for (int i = 0; i < 256; i++)
            {
                long sumR = 0, sumG = 0, sumB = 0;
                (int, int) cube = cubes[i];
                int end = cube.Item1 + cube.Item2;

                for (int j = cube.Item1; j < end; j++)
                {
                    sumR += colors[j].Item1;
                    sumG += colors[j].Item2;
                    sumB += colors[j].Item3;
                }

                byte avgR = (byte)(sumR / cube.Item2);
                byte avgG = (byte)(sumG / cube.Item2);
                byte avgB = (byte)(sumB / cube.Item2);

                palette[i] = (uint)((255 << 24) | (avgR << 16) | (avgG << 8) | avgB);
            }

            return palette;
        }

        private static void QuickSort((byte, byte, byte)[] array, int left, int right, byte channel)
        {
            if (left >= right) return;

            int i = left;
            int j = right;
            (byte, byte, byte) pivot = array[(left + right) / 2];

            while (i <= j)
            {
                if (channel == 0)
                {
                    while (array[i].Item1 < pivot.Item1) i++;
                    while (array[j].Item1 > pivot.Item1) j--;
                }
                else if (channel == 1)
                {
                    while (array[i].Item2 < pivot.Item2) i++;
                    while (array[j].Item2 > pivot.Item2) j--;
                }
                else
                {
                    while (array[i].Item3 < pivot.Item3) i++;
                    while (array[j].Item3 > pivot.Item3) j--;
                }

                if (i <= j)
                {
                    (byte, byte, byte) temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                    i++;
                    j--;
                }
            }

            if (left < j) QuickSort(array, left, j, channel);
            if (i < right) QuickSort(array, i, right, channel);
        }

        public static byte[] IndexImage(uint[] rgbaPixels, uint[] palette)
        {
            byte[] indexedPixels = new byte[rgbaPixels.Length];
            for(int i = 0; i < rgbaPixels.Length; i++)
            {
                indexedPixels[i] = FindClosestPaletteIndex(rgbaPixels[i], palette);
            }

            return indexedPixels;
        }

        private static byte FindClosestPaletteIndex(uint pixelColor, uint[] palette)
        {
            byte a = (byte)((pixelColor >> 24) & 0xFF);
            byte r = (byte)((pixelColor >> 16) & 0xFF);
            byte g = (byte)((pixelColor >> 8) & 0xFF);
            byte b = (byte)(pixelColor & 0xFF);

            int minDistance = int.MaxValue;
            byte bestIndex = 0;

            for (int i = 0; i < palette.Length; i++)
            {
                uint paletteColor = palette[i];

                byte pr = (byte)((paletteColor >> 16) & 0xFF);
                byte pg = (byte)((paletteColor >> 8) & 0xFF);
                byte pb = (byte)(paletteColor & 0xFF);

                int dr = r - pr;
                int dg = g - pg;
                int db = b - pb;
                int distance = (dr * dr) + (dg * dg) + (db * db);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    bestIndex = (byte)i;
                }
            }

            return bestIndex;
        }
    }
}
