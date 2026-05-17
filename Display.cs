using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using static ABSoftware.ABPixelEngine.Native;

namespace ABSoftware.ABPixelEngine
{
    public class Display
    {
        WndProcDelegate wndProcInstance;

        public IntPtr hWnd { get; private set; }
        public IntPtr hDC { get; private set; }
        public IntPtr glContext { get; private set; }
        int textureId;
        GCHandle bufferHandle;

        public string WindowName { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int PixelWidth = 1;
        public int PixelHeight = 1;

        public bool IsShuttingDown { get; private set; }

        public RenderBuffer SetupWindow(string name, int width, int height, WndProcDelegate wndProc, int pixelWidth = 1, int pixelHeight = 1)
        {
            this.WindowName = name;

            IsShuttingDown = false;

            this.PixelWidth = pixelWidth;
            this.PixelHeight = pixelHeight;
            this.Width = width;
            this.Height = height;

            IntPtr handle = GetModuleHandle(null);

            wndProcInstance = wndProc;

            WNDCLASSEX wndClass = default(WNDCLASSEX);

            wndClass.cbSize = Marshal.SizeOf<WNDCLASSEX>();
            wndClass.style = (int)(ClassStyles.HorizontalRedraw | ClassStyles.VerticalRedraw);
            wndClass.lpfnWndProc = wndProcInstance;
            wndClass.cbClsExtra = 0;
            wndClass.cbWndExtra = 0;
            wndClass.hInstance = handle;
            wndClass.hIcon = LoadIcon(IntPtr.Zero, IDI_APPLICATION);
            wndClass.hCursor = GetCursor(IDC.IDC_ARROW);
            wndClass.hbrBackground = IntPtr.Zero;
            wndClass.lpszMenuName = null;
            wndClass.lpszClassName = "PixelEngineWindowInstance";

            RECT rect = new RECT();
            rect.Left = 0; 
            rect.Top = 0;
            rect.Right = width;
            rect.Bottom = height;

            WindowStyles style = WindowStyles.WS_OVERLAPPED | WindowStyles.WS_CAPTION | WindowStyles.WS_SYSMENU | WindowStyles.WS_MINIMIZEBOX | WindowStyles.WS_VISIBLE;

            AdjustWindowRect(ref rect, (uint)style, false);

            short registerResult = RegisterClassEx(ref wndClass);
            if(registerResult == 0)
            {
                throw new Exception("Window registration error");
            }

            this.hWnd = CreateWindowEx(WindowStylesEx.WS_EX_APPWINDOW, "PixelEngineWindowInstance", name, style, CW_USEDEFAULT, CW_USEDEFAULT, rect.Right - rect.Left, rect.Bottom - rect.Top, IntPtr.Zero, IntPtr.Zero, handle, IntPtr.Zero);
            if(hWnd == IntPtr.Zero)
            {
                int code = Marshal.GetLastWin32Error();

                throw new Exception("Window creation error. Code: " + code);
            }

            return new RenderBuffer(Width / PixelWidth, Height / PixelHeight);
        }

        public void CreateContext()
        {
            hDC = GetDC(hWnd);

            PIXELFORMATDESCRIPTOR pfd = new PIXELFORMATDESCRIPTOR();
            pfd.nSize = (ushort)Marshal.SizeOf(pfd);
            pfd.nVersion = 1;
            pfd.dwFlags = PFD_DRAW_TO_WINDOW | PFD_SUPPORT_OPENGL | PFD_DOUBLEBUFFER;
            pfd.iPixelType = PFD_TYPE_RGBA;
            pfd.cColorBits = 32;
            pfd.cDepthBits = 24;

            int pixelFormat = ChoosePixelFormat(hDC, ref pfd);
            SetPixelFormat(hDC, pixelFormat, ref pfd);

            glContext = wglCreateContext(hDC);

            if(!wglMakeCurrent(hDC, glContext))
            {
                throw new Exception("OpenGL context creation error");
            }
        }

        public void InitOpenGL(RenderBuffer renderBuffer)
        {
            bufferHandle = GCHandle.Alloc(renderBuffer.Pixels, GCHandleType.Pinned);
            renderBuffer.Init(bufferHandle.AddrOfPinnedObject());

            glEnable(GL_TEXTURE_2D);

            glViewport(0, 0, Width, Height);

            glGenTextures(1, out textureId);

            glBindTexture(GL_TEXTURE_2D, textureId);

            glPixelStorei(GL_UNPACK_ALIGNMENT, 1);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, (int)GL_NEAREST);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, (int)GL_NEAREST);

            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, (int)GL_CLAMP_TO_EDGE);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, (int)GL_CLAMP_TO_EDGE);

            glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGBA, renderBuffer.Width, renderBuffer.Height, 0, GL_BGRA, GL_UNSIGNED_BYTE, IntPtr.Zero);

            glBindTexture(GL_TEXTURE_2D, 0);
        }

        public void UploadRenderBuffer(RenderBuffer renderBuffer)
        {
            glClear(GL_COLOR_BUFFER_BIT);

            glDisable(GL_LIGHTING);
            glDisable(GL_DEPTH_TEST);

            glBindTexture(GL_TEXTURE_2D, textureId);

            glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, renderBuffer.Width, renderBuffer.Height, GL_BGRA, GL_UNSIGNED_BYTE, renderBuffer.PixelsAddress);

            glBegin(GL_QUADS);
            glTexCoord2f(0f, 1f); glVertex2f(-1f, -1f);
            glTexCoord2f(1f, 1f); glVertex2f(1f, -1f);
            glTexCoord2f(1f, 0f); glVertex2f(1f, 1f);
            glTexCoord2f(0f, 0f); glVertex2f(-1f, 1f);
            glEnd();

            glBindTexture(GL_TEXTURE_2D, 0);

            SwapBuffers(hDC);
        }

        protected void ResizeWindow(int width, int height, int pixelWidth, int pixelHeight)
        {
            this.Width = width;
            this.Height = height;
            this.PixelWidth = pixelWidth;
            this.PixelHeight = pixelHeight;

            uint style = GetWindowLong(hWnd, GWL_STYLE);

            RECT rect = new RECT();
            rect.Left = 0;
            rect.Top = 0;
            rect.Right = width;
            rect.Bottom = height;

            AdjustWindowRect(ref rect, style, false);

            SetWindowPos(hWnd, IntPtr.Zero, 0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top, SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER);
        }

        protected void UpdateBuffer(ref RenderBuffer renderBuffer)
        {
            if (bufferHandle.IsAllocated)
                bufferHandle.Free();

            renderBuffer = new RenderBuffer(Width / PixelWidth, Height / PixelHeight);

            bufferHandle = GCHandle.Alloc(renderBuffer.Pixels, GCHandleType.Pinned);

            renderBuffer.Init(bufferHandle.AddrOfPinnedObject());

            glViewport(0, 0, Width, Height);

            glBindTexture(GL_TEXTURE_2D, textureId);

            glTexImage2D(GL_TEXTURE_2D, 0, (int)GL_RGBA, renderBuffer.Width, renderBuffer.Height, 0, GL_BGRA, GL_UNSIGNED_BYTE, IntPtr.Zero);

            glBindTexture(GL_TEXTURE_2D, 0);
        }

        public void DestroyOpenGL()
        {
            wglMakeCurrent(IntPtr.Zero, IntPtr.Zero);

            if (glContext != IntPtr.Zero)
            {
                wglDeleteContext(glContext);
            }

            if (glContext != IntPtr.Zero)
            {
                ReleaseDC(hWnd, hDC);
            }

            if (bufferHandle.IsAllocated)
                bufferHandle.Free();
        }

        public void CloseWindow()
        {
            PostMessage(hWnd, (uint)WM.CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        public void Rename(string newName)
        {
            SetWindowText(hWnd, newName);
        }

        protected void ProcessMessages()
        {
            MSG msg;
            while(GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                TranslateMessage(ref msg);
                DispatchMessage(ref msg);
            }
        }
    }
}
