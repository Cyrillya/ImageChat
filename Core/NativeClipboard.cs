using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using static System.Drawing.Image;

namespace ImageChat.Core;

internal partial class NativeMethods
{
    private const uint CF_BITMAP = 2U;

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();
    
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetClipboardData(uint uFormat, IntPtr data);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EmptyClipboard();

    public static void SetClipboardBitmap(IntPtr hBitmap) {
        OpenClipboard(IntPtr.Zero);
        EmptyClipboard();
        SetClipboardData(CF_BITMAP, hBitmap); // 这里又用到指针
        CloseClipboard();
    }

    public static bool ClipboardTryGetBitmap(out Bitmap bitmap)
    {
        bitmap = null;

        if (!IsClipboardFormatAvailable(CF_BITMAP))
        {
            return false;
        }

        try
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                return false;
            }

            IntPtr handle = GetClipboardData(CF_BITMAP);

            if (handle == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                bitmap = FromHbitmap(handle);

                return true;
            }
            catch
            {
                return false;
            }
        }
        finally
        {
            CloseClipboard();
        }
    }
}