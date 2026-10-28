using ABSoftware.ABPixelEngine.Sprites;
using ABSoftware.ABPixelEngine.TextRendering;
using System;

namespace ABSoftware.ABPixelEngine.Renderers
{
    public unsafe class DirectRenderer : IRenderer
    {
        Pixel* pixelsBuffer;
        int bufferWidth;
        int bufferHeight;

        public void PassPixelFilter(IPixelFilter pixelFilter)
        {
            return;
        }

        public void Draw(int x, int y, Pixel color)
        {
            pixelsBuffer[y * bufferWidth + x] = color;
        }

        public void DrawUnsafe(int index, Pixel color)
        {
            pixelsBuffer[index] = color;
        }

        public void DrawCircle(int x, int y, int radius, Pixel color)
        {
            if (x + radius < 0 || x - radius >= bufferWidth || y + radius < 0 || y - radius >= bufferHeight)
                return;

            if (radius <= 0)
            {
                if (x >= 0 && x < bufferWidth && y >= 0 && y < bufferHeight)
                    pixelsBuffer[y * bufferWidth + x] = color;
                return;
            }

            int xx = 0;
            int yy = radius;
            int d = 3 - 2 * radius;

            while (xx <= yy)
            {
                int cx_p_x = x + xx; int cx_m_x = x - xx;
                int cy_p_y = y + yy; int cy_m_y = y - yy;
                int cx_p_y = x + yy; int cx_m_y = x - yy;
                int cy_p_x = y + xx; int cy_m_x = y - xx;

                if (cy_p_y >= 0 && cy_p_y < bufferHeight)
                {
                    if (cx_p_x >= 0 && cx_p_x < bufferWidth) pixelsBuffer[cy_p_y * bufferWidth + cx_p_x] = color;
                    if (cx_m_x >= 0 && cx_m_x < bufferWidth) pixelsBuffer[cy_p_y * bufferWidth + cx_m_x] = color;
                }
                if (cy_m_y >= 0 && cy_m_y < bufferHeight)
                {
                    if (cx_p_x >= 0 && cx_p_x < bufferWidth) pixelsBuffer[cy_m_y * bufferWidth + cx_p_x] = color;
                    if (cx_m_x >= 0 && cx_m_x < bufferWidth) pixelsBuffer[cy_m_y * bufferWidth + cx_m_x] = color;
                }
                if (cy_p_x >= 0 && cy_p_x < bufferHeight)
                {
                    if (cx_p_y >= 0 && cx_p_y < bufferWidth) pixelsBuffer[cy_p_x * bufferWidth + cx_p_y] = color;
                    if (cx_m_y >= 0 && cx_m_y < bufferWidth) pixelsBuffer[cy_p_x * bufferWidth + cx_m_y] = color;
                }
                if (cy_m_x >= 0 && cy_m_x < bufferHeight)
                {
                    if (cx_p_y >= 0 && cx_p_y < bufferWidth) pixelsBuffer[cy_m_x * bufferWidth + cx_p_y] = color;
                    if (cx_m_y >= 0 && cx_m_y < bufferWidth) pixelsBuffer[cy_m_x * bufferWidth + cx_m_y] = color;
                }

                if (d < 0)
                {
                    d += 4 * xx + 6;
                }
                else
                {
                    d += 4 * (xx - yy) + 10;
                    yy--;
                }
                xx++;
            }
        }

        public void DrawEllipse(int xc, int yc, int rx, int ry, Pixel color)
        {
            if (xc + rx < 0 || xc - rx >= bufferWidth || yc + ry < 0 || yc - ry >= bufferHeight)
                return;

            if (rx <= 0 || ry <= 0) return;

            int x = 0;
            int y = ry;

            long rx2 = (long)rx * rx;
            long ry2 = (long)ry * ry;
            long fx2 = 2 * ry2;
            long fy2 = 2 * rx2;

            long p = ry2 - rx2 * ry + (rx2 >> 2);
            long dx = 0;
            long dy = fy2 * y;

            while (dx < dy)
            {
                int py = yc + y;
                int my = yc - y;
                int px = xc + x;
                int mx = xc - x;

                if (py >= 0 && py < bufferHeight)
                {
                    int row = py * bufferWidth;
                    if (px >= 0 && px < bufferWidth) pixelsBuffer[row + px] = color;
                    if (mx >= 0 && mx < bufferWidth) pixelsBuffer[row + mx] = color;
                }
                if (my >= 0 && my < bufferHeight)
                {
                    int row = my * bufferWidth;
                    if (px >= 0 && px < bufferWidth) pixelsBuffer[row + px] = color;
                    if (mx >= 0 && mx < bufferWidth) pixelsBuffer[row + mx] = color;
                }

                x++;
                dx += fx2;
                if (p < 0)
                {
                    p += ry2 + dx;
                }
                else
                {
                    y--;
                    dy -= fy2;
                    p += ry2 + dx - dy;
                }
            }

            p = (long)(ry2 * (x + 0.5f) * (x + 0.5f) + rx2 * (y - 1) * (y - 1) - rx2 * ry2);

            while (y >= 0)
            {
                int py = yc + y;
                int my = yc - y;
                int px = xc + x;
                int mx = xc - x;

                if (py >= 0 && py < bufferHeight)
                {
                    int row = py * bufferWidth;
                    if (px >= 0 && px < bufferWidth) pixelsBuffer[row + px] = color;
                    if (mx >= 0 && mx < bufferWidth) pixelsBuffer[row + mx] = color;
                }
                if (my >= 0 && my < bufferHeight)
                {
                    int row = my * bufferWidth;
                    if (px >= 0 && px < bufferWidth) pixelsBuffer[row + px] = color;
                    if (mx >= 0 && mx < bufferWidth) pixelsBuffer[row + mx] = color;
                }

                y--;
                dy -= fy2;
                if (p > 0)
                {
                    p += rx2 - dy;
                }
                else
                {
                    x++;
                    dx += fx2;
                    p += rx2 + dx - dy;
                }
            }
        }

        public void DrawHorizontalLine(int xStart, int xEnd, int y, Pixel color)
        {
            if (y < 0 || y >= bufferHeight) return;

            if (xStart > xEnd)
            {
                int temp = xStart;
                xStart = xEnd;
                xEnd = temp;
            }

            int x0 = xStart < 0 ? 0 : xStart;
            int x1 = xEnd >= bufferWidth ? bufferWidth - 1 : xEnd;

            if (x0 > x1) return;

            Pixel* ptr = pixelsBuffer + (y * bufferWidth + x0);

            for (int x = x0; x <= x1; x++)
            {
                *ptr = color;
                ptr++;
            }
        }

        public void DrawLine(int x0, int y0, int x1, int y1, Pixel color)
        {
            int minX = Math.Min(x0, x1);
            int maxX = Math.Max(x0, x1);
            int minY = Math.Min(y0, y1);
            int maxY = Math.Max(y0, y1);
            if (maxX < 0 || minX >= bufferWidth || maxY < 0 || minY >= bufferHeight) return;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int ptrStepX = sx;
            int ptrStepY = sy * bufferWidth;

            Pixel* ptr = pixelsBuffer + (y0 * bufferWidth) + x0;

            while (true)
            {
                if(x0 >= 0 && x0 < bufferWidth && y0 >= 0 && y0 < bufferHeight)
                {
                    *ptr = color;
                }

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                    ptr += ptrStepX;
                }
                else
                {
                    err += dx;
                    y0 += sy;
                    ptr += ptrStepY;
                }
            }
        }

        public void DrawLine(int x0, int y0, int x1, int y1, int thickness, Pixel color)
        {
            if(thickness <= 1)
            {
                DrawLine(x0, y0, x1, y1, color);
            }

            int offset = thickness / 2;
            int minX = Math.Min(x0, x1) - offset;
            int maxX = Math.Max(x0, x1) + offset;
            int minY = Math.Min(y0, y1) - offset;
            int maxY = Math.Max(y0, y1) + offset;
            if (maxX < 0 || minX >= bufferWidth || maxY < 0 || minY >= bufferHeight) return;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int ptrStepX = sx;
            int ptrStepY = sy * bufferWidth;

            Pixel* ptr = pixelsBuffer + (y0 * bufferWidth) + x0;

            bool tAlongY = dx > dy;

            int thickStep = tAlongY ? bufferWidth : 1;

            int startThick = -offset;
            int endThick = offset + (thickness % 2 == 0 ? 0 : 1);

            while (true)
            {
                for(int t = startThick; t < endThick; t++)
                {
                    int curX = tAlongY ? x0 : x0 + t;
                    int curY = tAlongY ? y0 + t : y0;

                    if (curX >= 0 && curX < bufferWidth && curY >= 0 && curY < bufferHeight)
                    {
                        *(ptr + (t * thickStep)) = color;
                    }
                }

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                    ptr += ptrStepX;
                }
                else
                {
                    err += dx;
                    y0 += sy;
                    ptr += ptrStepY;
                }
            }
        }

        public void DrawPolygon(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3, bool useTriangles, Pixel color)
        {
            if (useTriangles)
            {
                DrawLine(x0, y0, x1, y1, color);
                DrawLine(x1, y1, x2, y2, color);
                DrawLine(x2, y2, x3, y3, color);
                DrawLine(x3, y3, x0, y0, color);

                DrawLine(x2, y2, x0, y0, color);
            }
            else
            {
                DrawLine(x0, y0, x1, y1, color);
                DrawLine(x1, y1, x2, y2, color);
                DrawLine(x2, y2, x3, y3, color);
                DrawLine(x3, y3, x0, y0, color);
            }
        }

        public void DrawRectangle(int x, int y, int width, int height, Pixel color)
        {
            if (width <= 0 || height <= 0) return;

            int ex = x + width - 1;
            int ey = y + height - 1;

            int xStart = Math.Max(0, x);
            int xEnd = Math.Min(bufferWidth - 1, ex);
            int rowWidth = xEnd - xStart + 1;

            if (rowWidth > 0)
            {
                if (y >= 0 && y < bufferHeight)
                {
                    Pixel* topRowPtr = pixelsBuffer + (y * bufferWidth) + xStart;
                    for (int j = 0; j < rowWidth; j++) topRowPtr[j] = color;
                }

                if (ey >= 0 && ey < bufferHeight)
                {
                    Pixel* bottomRowPtr = pixelsBuffer + (ey * bufferWidth) + xStart;
                    for (int j = 0; j < rowWidth; j++) bottomRowPtr[j] = color;
                }
            }

            int yStart = Math.Max(0, y + 1);
            int yEnd = Math.Min(bufferHeight - 1, ey - 1);

            if (yStart <= yEnd)
            {
                if (x >= 0 && x < bufferWidth)
                {
                    Pixel* leftPtr = pixelsBuffer + (yStart * bufferWidth) + x;

                    for (int i = yStart; i <= yEnd; i++)
                    {
                        *leftPtr = color;
                        leftPtr += bufferWidth;
                    }
                }

                if (ex >= 0 && ex < bufferWidth)
                {
                    Pixel* rightPtr = pixelsBuffer + (yStart * bufferWidth) + ex;

                    for (int i = yStart; i <= yEnd; i++)
                    {
                        *rightPtr = color;
                        rightPtr += bufferWidth;
                    }
                }
            }
        }

        public void DrawString(int x, int y, string text, Font font, Pixel color)
        {
            if (string.IsNullOrEmpty(text)) return;

            int cX = x;
            int cY = y;

            int charWidth = font.CharacterWidth;
            int charHeight = font.CharacterHeight;
            int fontRows = font.Rows;
            int atlasWidth = charWidth * fontRows;

            fixed (byte* maskPtr = font.FontMask)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];

                    if (ch == '\n')
                    {
                        cY += charHeight + font.VerticalSpacing;
                        cX = x;
                        continue;
                    }

                    int sox = (ch % fontRows) * charWidth;
                    int soy = (ch / fontRows) * charHeight;

                    if (cX + charWidth < 0 || cX >= bufferWidth || cY + charHeight < 0 || cY >= bufferHeight)
                    {
                        cX += charWidth + font.HorizontalSpacing;
                        continue;
                    }

                    int yStart = Math.Max(0, cY);
                    int yEnd = Math.Min(bufferHeight - 1, cY + charHeight - 1);
                    int xStart = Math.Max(0, cX);
                    int xEnd = Math.Min(bufferWidth - 1, cX + charWidth - 1);

                    for (int sy = yStart; sy <= yEnd; sy++)
                    {
                        int fontY = soy + (sy - cY);
                        byte* fontRowPtr = maskPtr + (fontY * atlasWidth) + sox;

                        Pixel* screenRowPtr = pixelsBuffer + (sy * bufferWidth);

                        for (int sx = xStart; sx <= xEnd; sx++)
                        {
                            int fontX = sx - cX;

                            if (fontRowPtr[fontX] > 0)
                            {
                                screenRowPtr[sx] = color;
                            }
                        }
                    }

                    cX += charWidth + font.HorizontalSpacing;
                }
            }
        }

        public void DrawString(int x, int y, string text, Font font, Pixel color, int scaleX, int scaleY)
        {
            if (string.IsNullOrEmpty(text)) return;

            int cX = x;
            int cY = y;

            int charWidth = font.CharacterWidth;
            int charHeight = font.CharacterHeight;
            int fontRows = font.Rows;
            int atlasWidth = charWidth * fontRows;

            if (scaleX < 1) scaleX = 1;
            if (scaleY < 1) scaleY = 1;

            fixed (byte* maskPtr = font.FontMask)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];

                    if (ch > 255) continue;

                    if (ch == '\n')
                    {
                        cY += (charHeight + font.VerticalSpacing) * scaleY;
                        cX = x;
                        continue;
                    }

                    int sox = (ch % fontRows) * charWidth;
                    int soy = (ch / fontRows) * charHeight;

                    int scaledWidth = charWidth * scaleX;
                    int scaledHeight = charHeight * scaleY;
                    if (cX + scaledWidth < 0 || cX >= bufferWidth || cY + scaledHeight < 0 || cY >= bufferHeight)
                    {
                        cX += (charWidth + font.HorizontalSpacing) * scaleX;
                        continue;
                    }

                    for (int sy = 0; sy < charHeight; sy++)
                    {
                        int fontY = soy + sy;
                        byte* fontRowPtr = maskPtr + (fontY * atlasWidth) + sox;

                        int screenYBase = cY + (sy * scaleY);

                        for (int sx = 0; sx < charWidth; sx++)
                        {
                            if (fontRowPtr[sx] > 0)
                            {
                                int screenXBase = cX + (sx * scaleX);

                                for (int ay = 0; ay < scaleY; ay++)
                                {
                                    int targetY = screenYBase + ay;

                                    if (targetY < 0 || targetY >= bufferHeight) continue;

                                    int xStart = Math.Max(0, screenXBase);
                                    int xEnd = Math.Min(bufferWidth - 1, screenXBase + scaleX - 1);
                                    int lineLen = xEnd - xStart + 1;

                                    if (lineLen > 0)
                                    {
                                        Pixel* ptr = pixelsBuffer + (targetY * bufferWidth) + xStart;
                                        for (int k = 0; k < lineLen; k++)
                                        {
                                            *ptr++ = color;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    cX += (charWidth + font.HorizontalSpacing) * scaleX;
                }
            }
        }

        public void DrawStringBox(int x, int y, int width, int height, string text, Font font, Pixel color)
        {
            if (string.IsNullOrEmpty(text)) return;

            int cX = x;
            int cY = y;

            int charWidth = font.CharacterWidth;
            int charHeight = font.CharacterHeight;
            int fontRows = font.Rows;
            int atlasWidth = charWidth * fontRows;

            fixed (byte* maskPtr = font.FontMask)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];
                    if (ch > 255) continue;

                    if (cX + charWidth > (x + width))
                    {
                        cY += charHeight + font.VerticalSpacing;
                        cX = x;
                    }

                    if (cY + charHeight > (y + height))
                        break;

                    if (ch == '\n')
                    {
                        cY += charHeight + font.VerticalSpacing;
                        cX = x;
                        continue;
                    }

                    int sox = (ch % fontRows) * charWidth;
                    int soy = (ch / fontRows) * charHeight;

                    if (cX + charWidth < 0 || cX >= bufferWidth || cY + charHeight < 0 || cY >= bufferHeight)
                    {
                        cX += charWidth + font.HorizontalSpacing;
                        continue;
                    }

                    int yStart = Math.Max(0, cY);
                    int yEnd = Math.Min(bufferHeight - 1, cY + charHeight - 1);
                    int xStart = Math.Max(0, cX);
                    int xEnd = Math.Min(bufferWidth - 1, cX + charWidth - 1);

                    for (int sy = yStart; sy <= yEnd; sy++)
                    {
                        int fontY = soy + (sy - cY);
                        byte* fontRowPtr = maskPtr + (fontY * atlasWidth) + sox;
                        Pixel* screenRowPtr = pixelsBuffer + (sy * bufferWidth);

                        for (int sx = xStart; sx <= xEnd; sx++)
                        {
                            int fontX = sx - cX;
                            if (fontRowPtr[fontX] > 0)
                            {
                                screenRowPtr[sx] = color;
                            }
                        }
                    }

                    cX += charWidth + font.HorizontalSpacing;
                }
            }

        }

        public void DrawStringBox(int x, int y, int width, int height, string text, Font font, Pixel color, int scaleX, int scaleY)
        {
            if (string.IsNullOrEmpty(text)) return;

            int cX = x;
            int cY = y;

            int charWidth = font.CharacterWidth;
            int charHeight = font.CharacterHeight;
            int fontRows = font.Rows;
            int atlasWidth = charWidth * fontRows;

            if (scaleX < 1) scaleX = 1;
            if (scaleY < 1) scaleY = 1;

            fixed (byte* maskPtr = font.FontMask)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    char ch = text[i];
                    if (ch > 255) continue;

                    int scaledWidth = charWidth * scaleX;
                    int scaledHeight = charHeight * scaleY;

                    if (cX + scaledWidth > (x + width))
                    {
                        cY += (charHeight + font.VerticalSpacing) * scaleY;
                        cX = x;
                    }

                    if (cY + scaledHeight > (y + height))
                        break;

                    if (ch == '\n')
                    {
                        cY += (charHeight + font.VerticalSpacing) * scaleY;
                        cX = x;
                        continue;
                    }

                    int sox = (ch % fontRows) * charWidth;
                    int soy = (ch / fontRows) * charHeight;

                    if (cX + scaledWidth < 0 || cX >= bufferWidth || cY + scaledHeight < 0 || cY >= bufferHeight)
                    {
                        cX += (charWidth + font.HorizontalSpacing) * scaleX;
                        continue;
                    }

                    for (int sy = 0; sy < charHeight; sy++)
                    {
                        int fontY = soy + sy;
                        byte* fontRowPtr = maskPtr + (fontY * atlasWidth) + sox;
                        int screenYBase = cY + (sy * scaleY);

                        for (int sx = 0; sx < charWidth; sx++)
                        {
                            if (fontRowPtr[sx] > 0)
                            {
                                int screenXBase = cX + (sx * scaleX);

                                for (int ay = 0; ay < scaleY; ay++)
                                {
                                    int targetY = screenYBase + ay;
                                    if (targetY < 0 || targetY >= bufferHeight) continue;

                                    int xStart = Math.Max(0, screenXBase);
                                    int xEnd = Math.Min(bufferWidth - 1, screenXBase + scaleX - 1);
                                    int lineLen = xEnd - xStart + 1;

                                    if (lineLen > 0)
                                    {
                                        Pixel* ptr = pixelsBuffer + (targetY * bufferWidth) + xStart;
                                        for (int k = 0; k < lineLen; k++)
                                        {
                                            *ptr++ = color;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    cX += (charWidth + font.HorizontalSpacing) * scaleX;
                }
            }
        }

        public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Pixel color)
        {
            DrawLine(x0, y0, x1, y1, color);
            DrawLine(x1, y1, x2, y2, color);
            DrawLine(x2, y2, x0, y0, color);
        }

        public void FillCircle(int x, int y, int radius, Pixel color)
        {
            if (x + radius < 0 || x - radius >= bufferWidth || y + radius < 0 || y - radius >= bufferHeight)
                return;

            if (radius <= 0)
            {
                if (x >= 0 && x < bufferWidth && y >= 0 && y < bufferHeight)
                    pixelsBuffer[y * bufferWidth + x] = color;
                return;
            }

            int xx = 0;
            int yy = radius;
            int d = 3 - 2 * radius;

            while (xx <= yy)
            {
                DrawHorizontalLine(x - xx, x + xx, y + yy, color);
                DrawHorizontalLine(x - xx, x + xx, y - yy, color);
                DrawHorizontalLine(x - yy, x + yy, y + xx, color);
                DrawHorizontalLine(x - yy, x + yy, y - xx, color);

                if (d < 0)
                {
                    d += 4 * xx + 6;
                }
                else
                {
                    d += 4 * (xx - yy) + 10;
                    yy--;
                }
                xx++;
            }
        }

        public void FillEllipse(int xc, int yc, int rx, int ry, Pixel color)
        {
            if (xc + rx < 0 || xc - rx >= bufferWidth || yc + ry < 0 || yc - ry >= bufferHeight)
                return;

            if (rx <= 0 || ry <= 0) return;

            int x = 0;
            int y = ry;

            long rx2 = (long)rx * rx;
            long ry2 = (long)ry * ry;
            long fx2 = 2 * ry2;
            long fy2 = 2 * rx2;

            long p = ry2 - (rx2 * ry) + (rx2 / 4);
            long dx = 0;
            long dy = fy2 * y;

            while (dx < dy)
            {
                DrawHorizontalLine(xc - x, xc + x, yc + y, color);
                DrawHorizontalLine(xc - x, xc + x, yc - y, color);

                x++;
                dx += fx2;
                if (p < 0)
                {
                    p += ry2 + dx;
                }
                else
                {
                    y--;
                    dy -= fy2;
                    p += ry2 + dx - dy;
                }
            }

            p = ry2 * x * x + ry2 * x + (ry2 / 4) + rx2 * (y - 1) * (y - 1) - rx2 * ry2;

            while (y >= 0)
            {
                DrawHorizontalLine(xc - x, xc + x, yc + y, color);
                DrawHorizontalLine(xc - x, xc + x, yc - y, color);

                y--;
                dy -= fy2;
                if (p > 0)
                {
                    p += rx2 - dy;
                }
                else
                {
                    x++;
                    dx += fx2;
                    p += rx2 + dx - dy;
                }
            }
        }

        public void FillPolygon(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3, Pixel color)
        {
            FillTriangle(x0, y0, x1, y1, x2, y2, color);
            FillTriangle(x2, y2, x3, y3, x0, y0, color);
        }

        public void FillRectangle(int x, int y, int width, int height, Pixel color)
        {
            int x0 = Mathf.Max(0, x);
            int y0 = Mathf.Max(0, y);
            int x1 = Mathf.Min(bufferWidth, x + width);
            int y1 = Mathf.Min(bufferHeight, y + height);

            if (x0 >= x1 || y0 >= y1) return;

            int rowWidth = x1 - x0;

            for (int i = y0; i < y1; i++)
            {
                Pixel* rowPtr = pixelsBuffer + (i * bufferWidth) + x0;

                for(int j = 0; j < rowWidth; j++)
                {
                    rowPtr[j] = color;
                }
            }
        }

        public void FillTexturedTriangle(int x0, int y0, float u0, float v0, int x1, int y1, float u1, float v1, int x2, int y2, float u2, float v2, Bitmap texture)
        {
            fixed (byte* texPixelsPtr = texture.Pixels)
            fixed (uint* texPalettePtr = texture.Palette)
            {
                int texWidth = texture.Width;
                int texHeight = texture.Height;
                int texWidthMinus1 = texWidth - 1;
                int texHeightMinus1 = texHeight - 1;

                if (y0 > y1) { (x0, x1) = (x1, x0); (y0, y1) = (y1, y0); (u0, u1) = (u1, u0); (v0, v1) = (v1, v0); }
                if (y0 > y2) { (x0, x2) = (x2, x0); (y0, y2) = (y2, y0); (u0, u2) = (u2, u0); (v0, v2) = (v2, v0); }
                if (y1 > y2) { (x1, x2) = (x2, x1); (y1, y2) = (y2, y1); (u1, u2) = (u2, u1); (v1, v2) = (v2, v1); }

                int totalHeight = y2 - y0;
                if (totalHeight == 0) return;

                float invSlopeA = (float)(x2 - x0) / totalHeight;
                float duSlopeA = (u2 - u0) / totalHeight;
                float dvSlopeA = (v2 - v0) / totalHeight;

                float curXa = x0, curUa = u0, curVa = v0;
                float curXb = x0, curUb = u0, curVb = v0;

                int scanStart = Math.Max(0, y0);
                int scanEnd = Math.Min(bufferHeight - 1, y2);

                int height1 = y1 - y0;
                if (height1 > 0)
                {
                    float invSlopeB = (float)(x1 - x0) / height1;
                    float duSlopeB = (u1 - u0) / height1;
                    float dvSlopeB = (v1 - v0) / height1;

                    if (y0 < 0)
                    {
                        int skip = -y0;
                        curXa += invSlopeA * skip; curUa += duSlopeA * skip; curVa += dvSlopeA * skip;
                        curXb += invSlopeB * skip; curUb += duSlopeB * skip; curVb += dvSlopeB * skip;
                    }

                    int limit = Math.Min(scanEnd, y1);
                    for (int y = scanStart; y <= limit; y++)
                    {
                        RasterizeTexturedScanline(y, (int)curXa, (int)curXb, curUa, curVa, curUb, curVb, texPixelsPtr, texPalettePtr, texWidth, texHeightMinus1, texWidthMinus1);
                        curXa += invSlopeA; curUa += duSlopeA; curVa += dvSlopeA;
                        curXb += invSlopeB; curUb += duSlopeB; curVb += dvSlopeB;
                    }
                }

                int height2 = y2 - y1;
                if (height2 > 0)
                {
                    float invSlopeB = (float)(x2 - x1) / height2;
                    float duSlopeB = (u2 - u1) / height2;
                    float dvSlopeB = (v2 - v1) / height2;

                    curXb = x1; curUb = u1; curVb = v1;
                    if (y1 < 0)
                    {
                        curXa = x0 + invSlopeA * (-y0); curUa = u0 + duSlopeA * (-y0); curVa = v0 + dvSlopeA * (-y0);
                        curXb += invSlopeB * (-y1); curUb += duSlopeB * (-y1); curVb += dvSlopeB * (-y1);
                    }

                    int startY = Math.Max(scanStart, y1 + 1);
                    for (int y = startY; y <= scanEnd; y++)
                    {
                        RasterizeTexturedScanline(y, (int)curXa, (int)curXb, curUa, curVa, curUb, curVb, texPixelsPtr, texPalettePtr, texWidth, texHeightMinus1, texWidthMinus1);
                        curXa += invSlopeA; curUa += duSlopeA; curVa += dvSlopeA;
                        curXb += invSlopeB; curUb += duSlopeB; curVb += dvSlopeB;
                    }
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private unsafe void RasterizeTexturedScanline(int y, int xa, int xb, float ua, float va, float ub, float vb, byte* texPixels, uint* texPalette, int texWidth, int texHeightMinus1, int texWidthMinus1)
        {
            if (xa > xb)
            {
                (xa, xb) = (xb, xa);
                (ua, ub) = (ub, ua);
                (va, vb) = (vb, va);
            }

            int xStart = xa < 0 ? 0 : xa;
            int xEnd = xb >= bufferWidth ? bufferWidth - 1 : xb;
            int length = xEnd - xStart + 1;
            if (length <= 0) return;

            float width = xb - xa;
            float divWidth = width > 0 ? 1.0f / width : 0;

            float du = (ub - ua) * divWidth;
            float dv = (vb - va) * divWidth;

            if (xa < 0)
            {
                ua += du * (-xa);
                va += dv * (-xa);
            }

            Pixel* ptr = pixelsBuffer + (y * bufferWidth) + xStart;

            for (int i = 0; i < length; i++)
            {
                float uClamp = ua < 0f ? 0f : (ua > 1f ? 1f : ua);
                float vClamp = va < 0f ? 0f : (va > 1f ? 1f : va);

                int sx = (int)(uClamp * texWidthMinus1);
                int sy = (int)(vClamp * texHeightMinus1);

                int texIndex = sy * texWidth + sx;
                uint rawColor = texPalette[texPixels[texIndex]];

                *ptr = *(Pixel*)&rawColor;

                ptr++;
                ua += du;
                va += dv;
            }
        }

        public void FillTexturedTriangle(int x0, int y0, float u0, float v0, int x1, int y1, float u1, float v1, int x2, int y2, float u2, float v2, Bitmap texture, Pixel color)
        {
            fixed (byte* texPixelsPtr = texture.Pixels)
            fixed (uint* texPalettePtr = texture.Palette)
            {
                int texWidth = texture.Width;
                int texHeight = texture.Height;
                int texWidthMinus1 = texWidth - 1;
                int texHeightMinus1 = texHeight - 1;

                if (y0 > y1) { (x0, x1) = (x1, x0); (y0, y1) = (y1, y0); (u0, u1) = (u1, u0); (v0, v1) = (v1, v0); }
                if (y0 > y2) { (x0, x2) = (x2, x0); (y0, y2) = (y2, y0); (u0, u2) = (u2, u0); (v0, v2) = (v2, v0); }
                if (y1 > y2) { (x1, x2) = (x2, x1); (y1, y2) = (y2, y1); (u1, u2) = (u2, u1); (v1, v2) = (v2, v1); }

                int totalHeight = y2 - y0;
                if (totalHeight == 0) return;

                float invSlopeA = (float)(x2 - x0) / totalHeight;
                float duSlopeA = (u2 - u0) / totalHeight;
                float dvSlopeA = (v2 - v0) / totalHeight;

                float curXa = x0, curUa = u0, curVa = v0;
                float curXb = x0, curUb = u0, curVb = v0;

                int scanStart = Math.Max(0, y0);
                int scanEnd = Math.Min(bufferHeight - 1, y2);

                int height1 = y1 - y0;
                if (height1 > 0)
                {
                    float invSlopeB = (float)(x1 - x0) / height1;
                    float duSlopeB = (u1 - u0) / height1;
                    float dvSlopeB = (v1 - v0) / height1;

                    if (y0 < 0)
                    {
                        int skip = -y0;
                        curXa += invSlopeA * skip; curUa += duSlopeA * skip; curVa += dvSlopeA * skip;
                        curXb += invSlopeB * skip; curUb += duSlopeB * skip; curVb += dvSlopeB * skip;
                    }

                    int limit = Math.Min(scanEnd, y1);
                    for (int y = scanStart; y <= limit; y++)
                    {
                        RasterizeTexturedTintScanline(y, (int)curXa, (int)curXb, curUa, curVa, curUb, curVb, texPixelsPtr, texPalettePtr, texWidth, texHeightMinus1, texWidthMinus1, color);
                        curXa += invSlopeA; curUa += duSlopeA; curVa += dvSlopeA;
                        curXb += invSlopeB; curUb += duSlopeB; curVb += dvSlopeB;
                    }
                }

                int height2 = y2 - y1;
                if (height2 > 0)
                {
                    float invSlopeB = (float)(x2 - x1) / height2;
                    float duSlopeB = (u2 - u1) / height2;
                    float dvSlopeB = (v2 - v1) / height2;

                    curXb = x1; curUb = u1; curVb = v1;
                    if (y1 < 0)
                    {
                        curXa = x0 + invSlopeA * (-y0); curUa = u0 + duSlopeA * (-y0); curVa = v0 + duSlopeA * (-y0);
                        curXb += invSlopeB * (-y1); curUb += duSlopeB * (-y1); curVb += dvSlopeB * (-y1);
                    }

                    int startY = Math.Max(scanStart, y1 + 1);
                    for (int y = startY; y <= scanEnd; y++)
                    {
                        RasterizeTexturedTintScanline(y, (int)curXa, (int)curXb, curUa, curVa, curUb, curVb, texPixelsPtr, texPalettePtr, texWidth, texHeightMinus1, texWidthMinus1, color);
                        curXa += invSlopeA; curUa += duSlopeA; curVa += dvSlopeA;
                        curXb += invSlopeB; curUb += duSlopeB; curVb += dvSlopeB;
                    }
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private unsafe void RasterizeTexturedTintScanline(int y, int xa, int xb, float ua, float va, float ub, float vb, byte* texPixels, uint* texPalette, int texWidth, int texHeightMinus1, int texWidthMinus1, Pixel tintColor)
        {
            if (xa > xb)
            {
                (xa, xb) = (xb, xa);
                (ua, ub) = (ub, ua);
                (va, vb) = (vb, va);
            }

            int xStart = xa < 0 ? 0 : xa;
            int xEnd = xb >= bufferWidth ? bufferWidth - 1 : xb;
            int length = xEnd - xStart + 1;
            if (length <= 0) return;

            float width = xb - xa;
            float divWidth = width > 0 ? 1.0f / width : 0;

            float du = (ub - ua) * divWidth;
            float dv = (vb - va) * divWidth;

            if (xa < 0)
            {
                ua += du * (-xa);
                va += dv * (-xa);
            }

            byte tintAlpha = tintColor.A;
            int tintR = tintColor.R;
            int tintG = tintColor.G;
            int tintB = tintColor.B;

            Pixel* ptr = pixelsBuffer + (y * bufferWidth) + xStart;

            for (int i = 0; i < length; i++)
            {
                float uClamp = ua < 0f ? 0f : (ua > 1f ? 1f : ua);
                float vClamp = va < 0f ? 0f : (va > 1f ? 1f : va);

                int sx = (int)(uClamp * texWidthMinus1);
                int sy = (int)(vClamp * texHeightMinus1);

                int texIndex = sy * texWidth + sx;
                uint rawColor = texPalette[texPixels[texIndex]];

                int texR = (int)((rawColor >> 16) & 0xFF);
                int texG = (int)((rawColor >> 8) & 0xFF);
                int texB = (int)(rawColor & 0xFF);

                int r = ((tintR - texR) * tintAlpha >> 8) + texR;
                int g = ((tintG - texG) * tintAlpha >> 8) + texG;
                int b = ((tintB - texB) * tintAlpha >> 8) + texB;

                uint finalColor = (255U << 24) | ((uint)r << 16) | ((uint)g << 8) | (uint)b;

                *ptr = *(Pixel*)&finalColor;

                ptr++;
                ua += du;
                va += dv;
            }
        }

        public void FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Pixel color)
        {
            if (y0 > y1) { (x0, x1) = (x1, x0); (y0, y1) = (y1, y0); }
            if (y0 > y2) { (x0, x2) = (x2, x0); (y0, y2) = (y2, y0); }
            if (y1 > y2) { (x1, x2) = (x2, x1); (y1, y2) = (y2, y1); }

            int totalHeight = y2 - y0;
            if (totalHeight == 0) return;

            float invSlopeA = (float)(x2 - x0) / totalHeight;

            float curXa = x0;
            float curXb = x0;

            int scanStart = Math.Max(0, y0);
            int scanEnd = Math.Min(bufferHeight - 1, y2);

            int height1 = y1 - y0;
            if (height1 > 0)
            {
                float invSlopeB = (float)(x1 - x0) / height1;

                if (y0 < 0)
                {
                    curXa += invSlopeA * (-y0);
                    curXb += invSlopeB * (-y0);
                }

                int limit = Math.Min(scanEnd, y1);
                for (int y = scanStart; y <= limit; y++)
                {
                    RasterizeScanline(y, (int)curXa, (int)curXb, color);
                    curXa += invSlopeA;
                    curXb += invSlopeB;
                }
            }

            int height2 = y2 - y1;
            if (height2 > 0)
            {
                float invSlopeB = (float)(x2 - x1) / height2;

                curXb = x1;
                if (y1 < 0)
                {
                    curXa = x0 + invSlopeA * (-y0);
                    curXb += invSlopeB * (-y1);
                }

                int startY = Math.Max(scanStart, y1 + 1);
                for (int y = startY; y <= scanEnd; y++)
                {
                    RasterizeScanline(y, (int)curXa, (int)curXb, color);
                    curXa += invSlopeA;
                    curXb += invSlopeB;
                }
            }
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        private void RasterizeScanline(int y, int xa, int xb, Pixel color)
        {
            if (xa > xb) (xa, xb) = (xb, xa);

            int xStart = xa < 0 ? 0 : xa;
            int xEnd = xb >= bufferWidth ? bufferWidth - 1 : xb;

            int length = xEnd - xStart + 1;
            if (length <= 0) return;

            Pixel* ptr = pixelsBuffer + (y * bufferWidth) + xStart;
            for (int i = 0; i < length; i++)
            {
                *ptr++ = color;
            }
        }

        public void Initialize(RenderBuffer renderBuffer)
        {
            this.pixelsBuffer = (Pixel*)renderBuffer.PixelsAddress.ToPointer();
            this.bufferWidth = renderBuffer.Width;
            this.bufferHeight = renderBuffer.Height;
        }
    }
}
