using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ImageChat.Core;

internal partial class NativeMethods
{
    #region Native Methods

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    public static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    public static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc,
        int nXSrc, int nYSrc, CopyPixelOperation dwRop);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll")]
    public static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll")]
    public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("dwmapi.dll")]
    public static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out RECT pvAttribute, int cbAttribute);
    
    #endregion

    #region Expanding Methods

    public static Bitmap CaptureRectangleNative(Rectangle rect) {
        var handle = GetDesktopWindow(); // 获取桌面窗口
        var foregroundhandle = GetForegroundWindow(); // 获取前台窗口
        _ = DwmGetWindowAttribute(foregroundhandle, DWMWINDOWATTRIBUTE.ExtendedFrameBounds, out var bounds, Marshal.SizeOf(typeof(RECT)));
        rect = Rectangle.Intersect(bounds, rect); // 限制区域在前台窗口范围内

        if (rect.Width == 0 || rect.Height == 0) {
            return null;
        }

        IntPtr hdcSrc = GetWindowDC(handle);
        IntPtr hdcDest = CreateCompatibleDC(hdcSrc);
        IntPtr hBitmap = CreateCompatibleBitmap(hdcSrc, rect.Width, rect.Height);
        IntPtr hOld = SelectObject(hdcDest, hBitmap);
        BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.X, rect.Y,
            CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);

        if (ImageChat.Config.ScreenshotClipboard) {
            SetClipboardBitmap(hBitmap);
        }

        SelectObject(hdcDest, hOld);
        DeleteDC(hdcDest);
        ReleaseDC(handle, hdcSrc);
        Bitmap bmp = Image.FromHbitmap(hBitmap);
        DeleteObject(hBitmap);

        return bmp;
    }

    public static Point GetCursorPosition() {
        if (GetCursorPos(out var point)) {
            return (Point) point;
        }

        return Point.Empty;
    }
    
    #endregion
}