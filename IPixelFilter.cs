using System;

namespace ABSoftware.ABPixelEngine
{
    public interface IPixelFilter
    {
        Pixel Filter(int x, int y, Pixel newPixel, Pixel prevPixel);
    }
}
