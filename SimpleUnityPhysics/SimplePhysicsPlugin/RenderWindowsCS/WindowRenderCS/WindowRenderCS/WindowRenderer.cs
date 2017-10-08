using System;
using System.Runtime.InteropServices;

namespace WindowRenderCS
{
    public class WindowRenderer
    {

        [DllImport("RenderWindowsPlugin")]
        public static extern bool InitDeskDupl(IntPtr dummyTexture, int outputNum, int screenWidth, int screenHeight);

        [DllImport("RenderWindowsPlugin")]
        public static extern void CleanupDeskDupl();

        [DllImport("RenderWindowsPlugin")]
        public static extern void GetDesktopFrame(IntPtr dummyTexture, out int width, out int height, byte[] data, int lenData, int timeoutInMillis);

        [DllImport("RenderWindowsPlugin")]
        public static extern void SetDebugFunction(IntPtr fp);

    }
}