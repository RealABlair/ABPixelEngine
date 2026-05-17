using System;
using System.Threading;

namespace ABSoftware.ABPixelEngine
{
    public class PixelEngineContext : Display
    {
        public bool IsActive { get; private set; }
        Thread mainThread;
        public EngineTime engineStartTime { get; private set; }
        public EngineTimeSpan deltaTime { get; private set; }
        public readonly float fixedDeltaTime;
        public RenderBuffer renderBuffer;
        float fixedUpdateAccumulator;
        float renderAccumulator;
        EngineTime lastUpdateTime;

        public bool IsFocused { get; private set; }
        public string Title { get { return WindowName; } set { Rename(value); } }

        public float TargetFramerate = 1f / 60f;

        public int RenderableWidth { get { return renderBuffer.Width; } }
        public int RenderableHeight { get { return renderBuffer.Height; } }

        public Input Input { get; private set; }

        public PixelEngineContext() : this(0.02f) { }

        public PixelEngineContext(float fixedUpdateDelta)
        {
            lastUpdateTime = EngineTime.Now;
            fixedDeltaTime = fixedUpdateDelta;
        }

        public void Initialize(int width, int height)
        {
            Initialize(this.GetType().Name, width, height);
        }

        public void Initialize(string name, int width, int height)
        {
            renderBuffer = SetupWindow(name, width, height, WndProc);
        }

        public void Initialize(int width, int height, int pixelWidth, int pixelHeight)
        {
            Initialize(this.GetType().Name, width, height, pixelWidth, pixelHeight);
        }

        public void Initialize(string name, int width, int height, int pixelWidth, int pixelHeight)
        {
            renderBuffer = SetupWindow(name, width, height, WndProc, pixelWidth, pixelHeight);
        }

        public void Start()
        {
            if (IsActive)
                return;
            IsActive = true;

            mainThread = new Thread(MainLoop);
            mainThread.Start();

            ProcessMessages();
        }

        public void Stop()
        {
            if (!IsActive)
                return;

            IsActive = false;
        }

        void MainLoop()
        {
            Input = new Input();

            CreateContext();
            InitOpenGL(renderBuffer);

            if (!OnStart())
            {
                Stop();
            }

            this.engineStartTime = EngineTime.Now;

            while(IsActive)
            {
                if(IsShuttingDown)
                {
                    IsActive = false;
                    break;
                }

                EngineTime now = EngineTime.Now;

                deltaTime = now - lastUpdateTime;
                float deltaSeconds = deltaTime.Seconds;
                lastUpdateTime = now;

                if (deltaSeconds > 0.25f) deltaSeconds = 0.25f;

                fixedUpdateAccumulator += deltaSeconds;

                Input.Update();

                while(fixedUpdateAccumulator >= fixedDeltaTime)
                {
                    OnFixedUpdate();
                    fixedUpdateAccumulator -= fixedDeltaTime;
                }

                if (!OnUpdate())
                    Stop();

                renderAccumulator += deltaSeconds;
                if (renderAccumulator >= TargetFramerate)
                {
                    UploadRenderBuffer(renderBuffer);
                    renderAccumulator %= TargetFramerate;
                }

                Thread.Yield();
            }

            DestroyOpenGL();
            CloseWindow();
        }

        public void SetTargetFramerate(float framerate)
        {
            this.TargetFramerate = 1f / framerate;
        }

        public void Resize(int width, int height, int pixelWidth, int pixelHeight)
        {
            ResizeWindow(width, height, pixelWidth, pixelHeight);
            UpdateBuffer(ref renderBuffer);
        }

        IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            switch ((Native.WM)msg)
            {
                case Native.WM.CLOSE:
                    Native.DestroyWindow(hWnd);
                    IsActive = false;
                    return IntPtr.Zero;
                case Native.WM.DESTROY:
                    Native.PostQuitMessage(0);
                    IsActive = false;
                    return IntPtr.Zero;
                case Native.WM.ACTIVATE:
                    {
                        int state = (int)(wParam.ToInt64() & 0xFFFF);

                        if (state == Native.WA_INACTIVE)
                            IsFocused = false;
                        else
                            IsFocused = true;
                    }
                    break;
                case Native.WM.KEYDOWN:
                case Native.WM.SYSKEYDOWN:
                    {
                        bool repeat = ((int)lParam & (1 << 30)) != 0;
                        if (!repeat)
                            Input.SetState((Key)wParam.ToInt32(), true);
                    }
                    break;
                case Native.WM.KEYUP:
                case Native.WM.SYSKEYUP:
                    {
                        Input.SetState((Key)wParam.ToInt32(), false);
                    }
                    break;
                case Native.WM.MOUSEMOVE:
                    {
                        int arg = lParam.ToInt32();
                        short low = (short)(arg & 0xFFFF);
                        short high = (short)(arg >> 16 & 0xFFFF);

                        Input.SetMouse(Mathf.Clamp(low, 0, Width - 1), Mathf.Clamp(high, 0, Height - 1), Mathf.Clamp(low / PixelWidth, 0, RenderableWidth - 1), Mathf.Clamp(high / PixelHeight, 0, RenderableHeight - 1));
                    }
                    break;
                case Native.WM.MOUSEWHEEL:
                    {
                        int arg = wParam.ToInt32();

                        Input.SetScroll(arg);
                    }
                    break;
            }

            return Native.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        public void Clear(Pixel color)
        {
            renderBuffer.Clear(color);
        }

        public void Draw(int x, int y, Pixel color)
        {
            renderBuffer.SetPixel(x, y, color);
        }

        public Pixel GetPixel(int x, int y)
        {
            return renderBuffer.GetPixel(x, y);
        }

        public void FillRectangle(int x, int y, int width, int height, Pixel color)
        {
            int x0 = Mathf.Max(0, x);
            int y0 = Mathf.Max(0, y);
            int x1 = Mathf.Min(RenderableWidth, x + width);
            int y1 = Mathf.Min(RenderableHeight, y + height);

            if (x0 >= x1 || y0 >= y1) return;

            for(int i = y0; i < y1; i++)
            {
                int r = i * RenderableWidth;

                for(int j = x0; j < x1; j++)
                {
                    renderBuffer.Pixels[r + j] = color;
                }
            }
        }

        public void DrawRectangle(int x, int y, int width, int height, Pixel color)
        {
            if (width <= 0 || height <= 0)
                return;

            int ex = x + width - 1;
            int ey = y + height - 1;

            for(int i = x; i <= ex; i++)
            {
                Draw(i, y, color);
                Draw(i, ey, color);
            }

            for (int i = y+1; i < ey; i++)
            {
                Draw(x, i, color);
                Draw(ex, i, color);
            }
        }

        public void DrawLine(int x0, int y0, int x1, int y1, Pixel color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while(true)
            {
                Draw(x0, y0, color);

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if(e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                else
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public void DrawLine(int x0, int y0, int x1, int y1, int thickness, Pixel color)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int radius = (int)(thickness / 2f);

            while (true)
            {
                FillCirlce(x0, y0, radius, color);

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                else
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        public void FillCirlce(int x, int y, int radius, Pixel color)
        {
            if(radius <= 0)
            {
                Draw(x, y, color);
                return;
            }

            for(int yy = -radius; yy <= radius; yy++)
            {
                for (int xx = -radius; xx <= radius; xx++)
                {
                    if (xx * xx + yy * yy <= radius * radius)
                        Draw(x + xx, y + yy, color);
                }
            }
        }

        public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Pixel color)
        {
            DrawLine(x0, y0, x1, y1, color);
            DrawLine(x1, y1, x2, y2, color);
            DrawLine(x2, y2, x0, y0, color);
        }

        public void FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Pixel color)
        {
            if (y0 > y1) { (x0, x1) = (x1, x0); (y0, y1) = (y1, y0); }
            if (y0 > y2) { (x0, x2) = (x2, x0); (y0, y2) = (y2, y0); }
            if (y1 > y2) { (x1, x2) = (x2, x1); (y1, y2) = (y2, y1); }

            int totalHeight = y2 - y0;
            if (totalHeight == 0) return;

            for (int y = y0; y <= y2; y++)
            {
                if (y < 0) continue;
                if (y >= RenderableHeight) break;

                bool isSecondHalf = y > y1 || y0 == y1;
                int segmentHeight = isSecondHalf ? (y2 - y1) : (y1 - y0);

                if (segmentHeight == 0) continue;

                float alpha = (float)(y - y0) / totalHeight;
                float beta = (float)(y - (isSecondHalf ? y1 : y0)) / segmentHeight;

                int xa = x0 + (int)((x2 - x0) * alpha);

                int xb = isSecondHalf
                    ? x1 + (int)((x2 - x1) * beta)
                    : x0 + (int)((x1 - x0) * beta);

                if (xa > xb) (xa, xb) = (xb, xa);

                int xStart = Mathf.Max(0, xa);
                int xEnd = Mathf.Min(RenderableWidth, xb + 1);

                if (xStart >= xEnd) continue;

                int rowOffset = y * RenderableWidth;
                for (int x = xStart; x < xEnd; x++)
                {
                    renderBuffer.Pixels[rowOffset + x] = color;
                }
            }
        }

        public void DrawEllipse(int xc, int yc, int rx, int ry, Pixel color)
        {
            if (rx <= 0 || ry <= 0) return;

            int x = 0;
            int y = ry;

            long rx2 = (long)rx * rx;
            long ry2 = (long)ry * ry;
            long fx2 = 2 * ry2;
            long fy2 = 2 * rx2;

            long p = (long)(ry2 - rx2 * ry + 0.25 * rx2);
            long dx = 0;
            long dy = fy2 * y;

            while (dx < dy)
            {
                Draw(xc + x, yc + y, color);
                Draw(xc - x, yc + y, color);
                Draw(xc + x, yc - y, color);
                Draw(xc - x, yc - y, color);

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

            p = (long)(ry2 * (x + 0.5) * (x + 0.5) + rx2 * (y - 1) * (y - 1) - rx2 * ry2);

            while (y >= 0)
            {
                Draw(xc + x, yc + y, color);
                Draw(xc - x, yc + y, color);
                Draw(xc + x, yc - y, color);
                Draw(xc - x, yc - y, color);

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

        public void FillEllipse(int xc, int yc, int rx, int ry, Pixel color)
        {
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

            p = (long)(ry2 * (x + 0.5) * (x + 0.5) + rx2 * (y - 1) * (y - 1) - rx2 * ry2);

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

        private void DrawHorizontalLine(int xStart, int xEnd, int y, Pixel color)
        {
            if (y < 0 || y >= RenderableHeight) return;

            int x0 = Mathf.Max(0, xStart);
            int x1 = Mathf.Min(RenderableWidth - 1, xEnd);

            if (x0 > x1) return;

            int rowOffset = y * RenderableWidth;
            int maxIndex = renderBuffer.Pixels.Length;

            for (int x = x0; x <= x1; x++)
            {
                int index = rowOffset + x;
                if (index >= 0 && index < maxIndex)
                {
                    renderBuffer.Pixels[index] = color;
                }
            }
        }

        /// <returns>Returns a flag if the engine should keep itself active or not</returns>
        public virtual bool OnStart() { return true; }
        public virtual void OnFixedUpdate() { }
        /// <returns>Returns a flag if the engine should keep itself active or not</returns>
        public virtual bool OnUpdate() { return true; }
        public virtual void OnStop() { }
    }
}
