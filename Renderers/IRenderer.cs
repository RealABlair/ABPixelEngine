using System;
using ABSoftware.ABPixelEngine.Sprites;
using ABSoftware.ABPixelEngine.TextRendering;

namespace ABSoftware.ABPixelEngine.Renderers
{
    public interface IRenderer
    {
        void Initialize(RenderBuffer renderBuffer);
        void PassPixelFilter(IPixelFilter pixelFilter);

        void Draw(int x, int y, Pixel color);
        void DrawUnsafe(int index, Pixel color);
        void FillRectangle(int x, int y, int width, int height, Pixel color);
        void DrawRectangle(int x, int y, int width, int height, Pixel color);
        void DrawLine(int x0, int y0, int x1, int y1, Pixel color);
        void DrawLine(int x0, int y0, int x1, int y1, int thickness, Pixel color);
        void DrawCircle(int x, int y, int radius, Pixel color);
        void FillCircle(int x, int y, int radius, Pixel color);
        void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Pixel color);
        void FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Pixel color);
        void FillTexturedTriangle(int x0, int y0, float u0, float v0, int x1, int y1, float u1, float v1, int x2, int y2, float u2, float v2, Bitmap texture);
        void FillTexturedTriangle(int x0, int y0, float u0, float v0, int x1, int y1, float u1, float v1, int x2, int y2, float u2, float v2, Bitmap texture, Pixel color);
        void DrawEllipse(int xc, int yc, int rx, int ry, Pixel color);
        void FillEllipse(int xc, int yc, int rx, int ry, Pixel color);
        void DrawHorizontalLine(int xStart, int xEnd, int y, Pixel color);
        void DrawPolygon(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3, bool useTriangles, Pixel color);
        void FillPolygon(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3, Pixel color);
        void DrawString(int x, int y, string text, Font font, Pixel color);
        void DrawString(int x, int y, string text, Font font, Pixel color, int scaleX, int scaleY);
        void DrawStringBox(int x, int y, int width, int height, string text, Font font, Pixel color);
        void DrawStringBox(int x, int y, int width, int height, string text, Font font, Pixel color, int scaleX, int scaleY);
    }
}
