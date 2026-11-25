using System;
using System.Runtime.CompilerServices;
using ABSoftware.ABPixelEngine.Renderers;

namespace ABSoftware.ABPixelEngine.Shaders
{
    public unsafe class Lighting2DShader : IFragmentShader
    {
        public Pixel AmbientColor = new Pixel(255, 255, 255, 255);
        public ArrayList<PointLight2D> StaticLights { get; } = new ArrayList<PointLight2D>();
        public ArrayList<DynamicPointLight2D> DynamicLights { get; } = new ArrayList<DynamicPointLight2D>();

        private short[] staticLightMap;
        private short[] finalLightMap;
        private int cachedWidth;
        private int cachedHeight;
        private int lmWidth;
        private int lmHeight;
        private bool isStaticDirty = true;

        private readonly bool useHalfRes;
        public bool UseHalfRes => useHalfRes;

        public Lighting2DShader(bool useHalfRes = true)
        {
            this.useHalfRes = useHalfRes;
        }

        public void RebuildStaticLighting()
        {
            this.isStaticDirty = true;
        }

        public void Apply(RenderBuffer buffer)
        {
            int width = buffer.Width;
            int height = buffer.Height;
            int pixelCount = width * height;
            int mapLength = lmWidth * lmHeight * 3;

            if (finalLightMap == null || cachedWidth != width || cachedHeight != height)
            {
                cachedWidth = width;
                cachedHeight = height;

                if (useHalfRes)
                {
                    lmWidth = (width + 1) >> 1;
                    lmHeight = (height + 1) >> 1;
                }
                else
                {
                    lmWidth = width;
                    lmHeight = height;
                }

                mapLength = lmWidth * lmHeight * 3;
                staticLightMap = new short[mapLength];
                finalLightMap = new short[mapLength];
                isStaticDirty = true;
            }

            fixed (short* staticPtr = staticLightMap)
            fixed (short* finalPtr = finalLightMap)
            {
                if (isStaticDirty)
                {
                    short ambR = AmbientColor.R;
                    short ambG = AmbientColor.G;
                    short ambB = AmbientColor.B;

                    for (int i = 0; i < mapLength; i += 3)
                    {
                        staticPtr[i] = ambR;
                        staticPtr[i + 1] = ambG;
                        staticPtr[i + 2] = ambB;
                    }

                    for (int i = 0; i < StaticLights.Size; i++)
                    {
                        PointLight2D light = StaticLights[i];

                        if (useHalfRes)
                            AccumulateLight(staticPtr, lmWidth, lmHeight, light.X >> 1, light.Y >> 1, light.Radius >> 1, light.R, light.G, light.B, light.Intensity);
                        else
                            AccumulateLight(staticPtr, lmWidth, lmHeight, light.X, light.Y, light.Radius, light.R, light.G, light.B, light.Intensity);
                    }

                    isStaticDirty = false;
                }

                Buffer.BlockCopy(staticLightMap, 0, finalLightMap, 0, mapLength * sizeof(short));

                for (int i = 0; i < DynamicLights.Size; i++)
                {
                    DynamicPointLight2D light = DynamicLights[i];
                    if (light == null || !light.IsActive) continue;

                    if (useHalfRes)
                        AccumulateLight(finalPtr, lmWidth, lmHeight, light.X >> 1, light.Y >> 1, light.Radius >> 1, light.R, light.G, light.B, light.Intensity);
                    else
                        AccumulateLight(finalPtr, lmWidth, lmHeight, light.X, light.Y, light.Radius, light.R, light.G, light.B, light.Intensity);
                }

                fixed (Pixel* pixels = buffer.Pixels)
                {
                    uint* dst = (uint*)pixels;

                    if (useHalfRes)
                        ApplyHalfRes(dst, finalPtr, width, height, lmWidth);
                    else
                        ApplyFullRes(dst, finalPtr, width * height);
                }

                /*fixed (Pixel* pixels = buffer.Pixels)
                {
                    uint* dst = (uint*)pixels;
                    short* lm = finalPtr;
                    
                    for (int i = 0; i < pixelCount; i++)
                    {
                        //int lmIdx = i * 3;
                        uint px = dst[i];

                        int bVal = ((int)(px & 0xFF) * lm[2]) >> 8;
                        int gVal = ((int)((px >> 8) & 0xFF) * lm[1]) >> 8;
                        int rVal = ((int)((px >> 16) & 0xFF) * lm[0]) >> 8;

                        rVal = (rVal | ((255 - rVal) >> 31)) & 0xFF;
                        gVal = (gVal | ((255 - gVal) >> 31)) & 0xFF;
                        bVal = (bVal | ((255 - bVal) >> 31)) & 0xFF;

                        dst[i] = (px & 0xFF000000 | (uint)rVal << 16 | (uint)gVal << 8 | (uint)bVal);
                        lm += 3;
                    }
                }*/
            }
        }

        static void ApplyHalfRes(uint* dst, short* finalPtr, int width, int height, int lmWidth)
        {
            int fullRows = height >> 1;
            int fullCols = width >> 1;
            int lmStride = lmWidth * 3;

            for (int ly = 0; ly < fullRows; ly++)
            {
                int sy = ly << 1;
                uint* row0 = dst + sy * width;
                uint* row1 = row0 + width;
                short* lmRow = finalPtr + ly * lmStride;

                for (int lx = 0; lx < fullCols; lx++)
                {
                    int lR = lmRow[0];
                    int lG = lmRow[1];
                    int lB = lmRow[2];
                    lmRow += 3;

                    int sx = lx << 1;

                    if (((lR | lG | lB) & ~0xFF) == 0)
                    {
                        uint uR = (uint)lR;
                        uint uG = (uint)lG;
                        uint uB = (uint)lB;

                        uint px00 = row0[sx];
                        uint b00 = ((px00 & 0xFF) * uB) >> 8;
                        uint g00 = (((px00 >> 8) & 0xFF) * uG) & 0x0000FF00;
                        uint r00 = (((px00 >> 16) & 0xFF) * uR) << 8;
                        row0[sx] = (px00 & 0xFF000000) | (r00 & 0x00FF0000) | g00 | b00;

                        uint px01 = row0[sx + 1];
                        uint b01 = ((px01 & 0xFF) * uB) >> 8;
                        uint g01 = (((px01 >> 8) & 0xFF) * uG) & 0x0000FF00;
                        uint r01 = (((px01 >> 16) & 0xFF) * uR) << 8;
                        row0[sx + 1] = (px01 & 0xFF000000) | (r01 & 0x00FF0000) | g01 | b01;

                        uint px10 = row1[sx];
                        uint b10 = ((px10 & 0xFF) * uB) >> 8;
                        uint g10 = (((px10 >> 8) & 0xFF) * uG) & 0x0000FF00;
                        uint r10 = (((px10 >> 16) & 0xFF) * uR) << 8;
                        row1[sx] = (px10 & 0xFF000000) | (r10 & 0x00FF0000) | g10 | b10;

                        uint px11 = row1[sx + 1];
                        uint b11 = ((px11 & 0xFF) * uB) >> 8;
                        uint g11 = (((px11 >> 8) & 0xFF) * uG) & 0x0000FF00;
                        uint r11 = (((px11 >> 16) & 0xFF) * uR) << 8;
                        row1[sx + 1] = (px11 & 0xFF000000) | (r11 & 0x00FF0000) | g11 | b11;
                    }
                    else
                    {
                        uint px00 = row0[sx];
                        int b00 = ((int)(px00 & 0xFF) * lB) >> 8;
                        int g00 = ((int)((px00 >> 8) & 0xFF) * lG) >> 8;
                        int r00 = ((int)((px00 >> 16) & 0xFF) * lR) >> 8;
                        r00 = (r00 | ((255 - r00) >> 31)) & 0xFF;
                        g00 = (g00 | ((255 - g00) >> 31)) & 0xFF;
                        b00 = (b00 | ((255 - b00) >> 31)) & 0xFF;
                        row0[sx] = (px00 & 0xFF000000) | ((uint)r00 << 16) | ((uint)g00 << 8) | (uint)b00;

                        uint px01 = row0[sx + 1];
                        int b01 = ((int)(px01 & 0xFF) * lB) >> 8;
                        int g01 = ((int)((px01 >> 8) & 0xFF) * lG) >> 8;
                        int r01 = ((int)((px01 >> 16) & 0xFF) * lR) >> 8;
                        r01 = (r01 | ((255 - r01) >> 31)) & 0xFF;
                        g01 = (g01 | ((255 - g01) >> 31)) & 0xFF;
                        b01 = (b01 | ((255 - b01) >> 31)) & 0xFF;
                        row0[sx + 1] = (px01 & 0xFF000000) | ((uint)r01 << 16) | ((uint)g01 << 8) | (uint)b01;

                        uint px10 = row1[sx];
                        int b10 = ((int)(px10 & 0xFF) * lB) >> 8;
                        int g10 = ((int)((px10 >> 8) & 0xFF) * lG) >> 8;
                        int r10 = ((int)((px10 >> 16) & 0xFF) * lR) >> 8;
                        r10 = (r10 | ((255 - r10) >> 31)) & 0xFF;
                        g10 = (g10 | ((255 - g10) >> 31)) & 0xFF;
                        b10 = (b10 | ((255 - b10) >> 31)) & 0xFF;
                        row1[sx] = (px10 & 0xFF000000) | ((uint)r10 << 16) | ((uint)g10 << 8) | (uint)b10;

                        uint px11 = row1[sx + 1];
                        int b11 = ((int)(px11 & 0xFF) * lB) >> 8;
                        int g11 = ((int)((px11 >> 8) & 0xFF) * lG) >> 8;
                        int r11 = ((int)((px11 >> 16) & 0xFF) * lR) >> 8;
                        r11 = (r11 | ((255 - r11) >> 31)) & 0xFF;
                        g11 = (g11 | ((255 - g11) >> 31)) & 0xFF;
                        b11 = (b11 | ((255 - b11) >> 31)) & 0xFF;
                        row1[sx + 1] = (px11 & 0xFF000000) | ((uint)r11 << 16) | ((uint)g11 << 8) | (uint)b11;
                    }
                }
            }

            if ((height & 1) != 0)
            {
                int lastY = height - 1;
                uint* lastRow = dst + lastY * width;
                short* lmRow = finalPtr + fullRows * lmStride;

                for (int lx = 0; lx < fullCols; lx++)
                {
                    int lR = lmRow[0];
                    int lG = lmRow[1];
                    int lB = lmRow[2];
                    lmRow += 3;

                    int sx = lx << 1;

                    uint px = lastRow[sx];
                    uint px1 = lastRow[sx + 1];

                    int bVal = ((int)(px & 0xFF) * lB) >> 8;
                    int gVal = ((int)((px >> 8) & 0xFF) * lG) >> 8;
                    int rVal = ((int)((px >> 16) & 0xFF) * lR) >> 8;
                    rVal = (rVal | ((255 - rVal) >> 31)) & 0xFF;
                    gVal = (gVal | ((255 - gVal) >> 31)) & 0xFF;
                    bVal = (bVal | ((255 - bVal) >> 31)) & 0xFF;

                    int bVal1 = ((int)(px1 & 0xFF) * lB) >> 8;
                    int gVal1 = ((int)((px1 >> 8) & 0xFF) * lG) >> 8;
                    int rVal1 = ((int)((px1 >> 16) & 0xFF) * lR) >> 8;
                    rVal1 = (rVal1 | ((255 - rVal1) >> 31)) & 0xFF;
                    gVal1 = (gVal1 | ((255 - gVal1) >> 31)) & 0xFF;
                    bVal1 = (bVal1 | ((255 - bVal1) >> 31)) & 0xFF;

                    lastRow[sx] = (px & 0xFF000000) | ((uint)rVal << 16) | ((uint)gVal << 8) | (uint)bVal;
                    lastRow[sx + 1] = (px1 & 0xFF000000) | ((uint)rVal1 << 16) | ((uint)gVal1 << 8) | (uint)bVal1;
                }

                if ((width & 1) != 0)
                {
                    int lastX = width - 1;
                    uint px = lastRow[lastX];

                    int lR = lmRow[0];
                    int lG = lmRow[1];
                    int lB = lmRow[2];

                    int bVal = ((int)(px & 0xFF) * lB) >> 8;
                    int gVal = ((int)((px >> 8) & 0xFF) * lG) >> 8;
                    int rVal = ((int)((px >> 16) & 0xFF) * lR) >> 8;
                    rVal = (rVal | ((255 - rVal) >> 31)) & 0xFF;
                    gVal = (gVal | ((255 - gVal) >> 31)) & 0xFF;
                    bVal = (bVal | ((255 - bVal) >> 31)) & 0xFF;

                    lastRow[lastX] = (px & 0xFF000000) | ((uint)rVal << 16) | ((uint)gVal << 8) | (uint)bVal;
                }
            }
        }

        static void ApplyFullRes(uint* dst, short* lm, int pixelCount)
        {
            for (int i = 0; i < pixelCount; i++)
            {
                int lR = lm[0];
                int lG = lm[1];
                int lB = lm[2];
                lm += 3;

                uint px = dst[i];

                if (((lR | lG | lB) & ~0xFF) == 0)
                {
                    uint bVal = ((px & 0xFF) * (uint)lB) >> 8;
                    uint gVal = (((px >> 8) & 0xFF) * (uint)lG) & 0x0000FF00;
                    uint rVal = (((px >> 16) & 0xFF) * (uint)lR) << 8;

                    dst[i] = (px & 0xFF000000) | (rVal & 0x00FF0000) | gVal | bVal;
                }
                else
                {
                    int bVal = ((int)(px & 0xFF) * lB) >> 8;
                    int gVal = ((int)((px >> 8) & 0xFF) * lG) >> 8;
                    int rVal = ((int)((px >> 16) & 0xFF) * lR) >> 8;

                    rVal = (rVal | ((255 - rVal) >> 31)) & 0xFF;
                    gVal = (gVal | ((255 - gVal) >> 31)) & 0xFF;
                    bVal = (bVal | ((255 - bVal) >> 31)) & 0xFF;

                    dst[i] = (px & 0xFF000000) | ((uint)rVal << 16) | ((uint)gVal << 8) | (uint)bVal;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint BlendPixel(uint px, int lR, int lG, int lB)
        {
            int bVal = ((int)(px & 0xFF) * lB) >> 8;
            int gVal = ((int)((px >> 8) & 0xFF) * lG) >> 8;
            int rVal = ((int)((px >> 16) & 0xFF) * lR) >> 8;

            rVal = (rVal | ((255 - rVal) >> 31)) & 0xFF;
            gVal = (gVal | ((255 - gVal) >> 31)) & 0xFF;
            bVal = (bVal | ((255 - bVal) >> 31)) & 0xFF;

            return (px & 0xFF000000) | ((uint)rVal << 16) | ((uint)gVal << 8) | (uint)bVal;
        }

        static void AccumulateLight(short* lmPtr, int width, int height, int lx, int ly, int r, byte lr, byte lg, byte lb, float intensity)
        {
            if (r <= 0 || intensity <= 0f) return;

            int minX = Math.Max(0, lx - r);
            int maxX = Math.Min(width - 1, lx + r);
            int minY = Math.Max(0, ly - r);
            int maxY = Math.Min(height - 1, ly + r);

            if (minX > maxX || minY > maxY) return;

            int rSq = r * r;
            int intensityFP = (int)(intensity * 256f);
            int invRSq = (1 << 24) / rSq;

            for (int y = minY; y <= maxY; y++)
            {
                int dy = y - ly;
                int dySq = dy * dy;
                int rowOffset = y * width * 3;

                for (int x = minX; x <= maxX; x++)
                {
                    int dx = x - lx;
                    int distSq = dx * dx + dySq;

                    if (distSq < rSq)
                    {
                        //int normDist = 256 - (distSq << 8) / rSq;
                        int normDist = 256 - ((distSq * invRSq) >> 16);
                        int atten = (normDist * normDist) >> 8;
                        atten = (atten * intensityFP) >> 8;

                        int idx = rowOffset + x * 3;
                        lmPtr[idx] += (short)((lr * atten) >> 8);
                        lmPtr[idx + 1] += (short)((lg * atten) >> 8);
                        lmPtr[idx + 2] += (short)((lb * atten) >> 8);
                    }
                }
            }
        }
    }

    public struct PointLight2D
    {
        public int X;
        public int Y;
        public int Radius;
        public byte R;
        public byte G;
        public byte B;
        public float Intensity;
    }

    public class DynamicPointLight2D
    {
        public int X;
        public int Y;
        public int Radius;
        public byte R;
        public byte G;
        public byte B;
        public float Intensity;
        public bool IsActive = true;
    }
}
