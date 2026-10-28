using ABSoftware.ABPixelEngine.Renderers;
using ABSoftware.ABPixelEngine.Sprites;
using ABSoftware.ABPixelEngine.Structures;
using ABSoftware.ABPixelEngine.TextRendering;
using System;
using System.Runtime.CompilerServices;
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

        public PixelMode DrawMode { get; private set; }
        private ABDictionary<PixelMode, IRenderer> renderers;
        private IRenderer currentRenderer;

        public PixelEngineContext() : this(0.02f) { }

        public PixelEngineContext(float fixedUpdateDelta)
        {
            lastUpdateTime = EngineTime.Now;
            fixedDeltaTime = fixedUpdateDelta;

            DrawMode = PixelMode.Direct;

            renderers = new ABDictionary<PixelMode, IRenderer>()
            {
                { PixelMode.Direct, new DirectRenderer() },
                { PixelMode.Alpha, new AlphaRenderer() },
                { PixelMode.Filter, new FilterRenderer() },
                { PixelMode.AlphaFilter, new AlphaFilterRenderer() }
            };

            currentRenderer = renderers[DrawMode];
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
            pixelFilter = defaultPixelFilter;
            Input = new Input();

            CreateContext();
            InitOpenGL(renderBuffer);

            foreach (var renderer in renderers)
            {
                renderer.Value.Initialize(renderBuffer);
            }

            if (!OnStart())
            {
                Stop();
            }

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
            foreach (var renderer in renderers)
            {
                renderer.Value.Initialize(renderBuffer);
            }
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

                        Input.SetMouse(low, high, low / PixelWidth, high / PixelWidth);
                    }
                    break;
                case Native.WM.MOUSEWHEEL:
                    {
                        int arg = wParam.ToInt32();

                        Input.SetScroll(arg);
                    }
                    break;
                case Native.WM.SYSCOMMAND: //Disable ALT
                    {
                        if((wParam.ToInt32() & 0xFFF0) == Native.SC_KEYMENU)
                            return IntPtr.Zero;
                    }
                    break;
                case Native.WM.KILLFOCUS:
                    {
                        Input.KillStates();
                    }
                    break;
                case Native.WM.LBUTTONDOWN: Input.SetState(Key.LButton, true); break;
                case Native.WM.LBUTTONUP: Input.SetState(Key.LButton, false); break;
                case Native.WM.RBUTTONDOWN: Input.SetState(Key.RButton, true); break;
                case Native.WM.RBUTTONUP: Input.SetState(Key.RButton, false); break;
                case Native.WM.MBUTTONDOWN: Input.SetState(Key.MButton, true); break;
                case Native.WM.MBUTTONUP: Input.SetState(Key.MButton, false); break;
            }

            return Native.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        public void Clear(Pixel color)
        {
            renderBuffer.Clear(color);
            currentRenderer = renderers[DrawMode];
        }

        public void Draw(int x, int y, Pixel color)
        {
            currentRenderer.Draw(x, y, color);
        }

        public void DrawUnsafe(int index, Pixel color)
        {
            currentRenderer.DrawUnsafe(index, color);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateRenderState(PixelMode state)
        {
            this.DrawMode = state;
            this.currentRenderer = this.renderers[state];
        }

        public Pixel GetPixel(int x, int y)
        {
            return renderBuffer.GetPixel(x, y);
        }

        public void FillRectangle(int x, int y, int width, int height, Pixel color)
        {
            currentRenderer.FillRectangle(x, y, width, height, color);
        }

        public void DrawRectangle(int x, int y, int width, int height, Pixel color)
        {
            currentRenderer.DrawRectangle(x, y, width, height, color);
        }

        public void DrawLine(int x0, int y0, int x1, int y1, Pixel color)
        {
            currentRenderer.DrawLine(x0, y0, x1, y1, color);
        }

        public void DrawLine(int x0, int y0, int x1, int y1, int thickness, Pixel color)
        {
            currentRenderer.DrawLine(x0, y0, x1, y1, thickness, color);
        }

        public void DrawCircle(int x, int y, int radius, Pixel color)
        {
            currentRenderer.DrawCircle(x, y, radius, color);
        }

        public void FillCircle(int x, int y, int radius, Pixel color)
        {
            currentRenderer.FillCircle(x, y, radius, color);
        }

        public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Pixel color)
        {
            currentRenderer.DrawTriangle(x0, y0, x1, y1, x2, y2, color);
        }

        public void FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, Pixel color)
        {
            currentRenderer.FillTriangle(x0, y0, x1, y1, x2, y2, color);
        }

        public void FillTexturedTriangle(int x0, int y0, float u0, float v0, int x1, int y1, float u1, float v1, int x2, int y2, float u2, float v2, Bitmap texture)
        {
            currentRenderer.FillTexturedTriangle(x0, y0, u0, v0, x1, y1, u1, v1, x2, y2, u2, v2, texture);
        }

        public void FillTexturedTriangle(int x0, int y0, float u0, float v0, int x1, int y1, float u1, float v1, int x2, int y2, float u2, float v2, Bitmap texture, Pixel color)
        {
            currentRenderer.FillTexturedTriangle(x0, y0, u0, v0, x1, y1, u1, v1, x2, y2, u2, v2, texture, color);
        }

        public void DrawEllipse(int xc, int yc, int rx, int ry, Pixel color)
        {
            currentRenderer.DrawEllipse(xc, yc, rx, ry, color);
        }

        public void FillEllipse(int xc, int yc, int rx, int ry, Pixel color)
        {
            currentRenderer.FillEllipse(xc, yc, rx, ry, color);
        }

        public void DrawPolygon(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3, bool useTriangles, Pixel color)
        {
            currentRenderer.DrawPolygon(x0, y0, x1, y1, x2, y2, x3, y3, useTriangles, color);
        }

        public void FillPolygon(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3, Pixel color)
        {
            currentRenderer.FillPolygon(x0, y0, x1, y1, x2, y2, x3, y3, color);
        }

        public void DrawString(int x, int y, string text, Font font, Pixel color)
        {
            currentRenderer.DrawString(x, y, text, font, color);
        }

        public void DrawString(int x, int y, string text, Font font, Pixel color, int scaleX, int scaleY)
        {
            currentRenderer.DrawString(x, y, text, font, color, scaleX, scaleY);
        }

        public void DrawStringBox(int x, int y, int width, int height, string text, Font font, Pixel color)
        {
            currentRenderer.DrawStringBox(x, y, width, height, text, font, color);
        }

        public void DrawStringBox(int x, int y, int width, int height, string text, Font font, Pixel color, int scaleX, int scaleY)
        {
            currentRenderer.DrawStringBox(x, y, width, height, text, font, color, scaleX, scaleY);
        }

        #region int2 extensions
        public void Draw(int2 position, Pixel color) => Draw(position.x, position.y, color);
        public void FillRectangle(int2 position, int2 size, Pixel color) => FillRectangle(position.x, position.y, size.x, size.y, color);
        public void DrawRectangle(int2 position, int2 size, Pixel color) => DrawRectangle(position.x, position.y, size.x, size.y, color);
        public void DrawLine(int2 position1, int2 position2, Pixel color) => DrawLine(position1.x, position1.y, position2.x, position2.y, color);
        public void DrawLine(int2 position1, int2 position2, int thickness, Pixel color) => DrawLine(position1.x, position1.y, position2.x, position2.y, thickness, color);
        public void DrawCircle(int2 position, int radius, Pixel color) => DrawCircle(position.x, position.y, radius, color);
        public void FillCircle(int2 position, int radius, Pixel color) => FillCircle(position.x, position.y, radius, color);
        public void DrawTriangle(int2 position1, int2 position2, int2 position3, Pixel color) => DrawTriangle(position1.x, position1.y, position2.x, position2.y, position3.x, position3.y, color);
        public void FillTriangle(int2 position1, int2 position2, int2 position3, Pixel color) => FillTriangle(position1.x, position1.y, position2.x, position2.y, position3.x, position3.y, color);
        public void DrawEllipse(int2 position, int2 size, Pixel color) => DrawEllipse(position.x, position.y, size.x, size.y, color);
        public void FillEllipse(int2 position, int2 size, Pixel color) => FillEllipse(position.x, position.y, size.x, size.y, color);
        public void DrawPolygon(int2 position1, int2 position2, int2 position3, int2 position4, bool useTriangles, Pixel color) => DrawPolygon(position1.x, position1.y, position2.x, position2.y, position3.x, position3.y, position4.x, position4.y, useTriangles, color);
        public void FillPolygon(int2 position1, int2 position2, int2 position3, int2 position4, Pixel color) => FillPolygon(position1.x, position1.y, position2.x, position2.y, position3.x, position3.y, position4.x, position4.y, color);
        #endregion

        #region Filtering
        DefaultFilter defaultPixelFilter = new DefaultFilter();
        IPixelFilter pixelFilter;

        public void EnablePixelFilter(IPixelFilter filter)
        {
            pixelFilter = filter;
            foreach(var renderer in renderers)
            {
                renderer.Value.PassPixelFilter(pixelFilter);
            }
        }

        public void DisablePixelFilter()
        {
            pixelFilter = defaultPixelFilter;
            foreach (var renderer in renderers)
            {
                renderer.Value.PassPixelFilter(pixelFilter);
            }
        }

        struct DefaultFilter : IPixelFilter
        {
            public Pixel Filter(int x, int y, Pixel newPixel, Pixel prevPixel)
            {
                return newPixel;
            }
        }
        #endregion

        /// <returns>Returns a flag if the engine should keep itself active or not</returns>
        public virtual bool OnStart() { return true; }
        public virtual void OnFixedUpdate() { }
        /// <returns>Returns a flag if the engine should keep itself active or not</returns>
        public virtual bool OnUpdate() { return true; }
        public virtual void OnStop() { }

        public enum PixelMode
        {
            Direct = 1,
            Alpha = 2,
            Filter = 4,
            AlphaFilter = Alpha | Filter
        }
    }
}
