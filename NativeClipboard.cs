﻿using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using static System.Drawing.Image;

namespace ImageChat;

class NativeClipboard
{
    private const uint CF_BITMAP = 2U;

    [DllImport("User32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool CloseClipboard();

    [DllImport("user32.dll")]
    private static extern bool SetClipboardData(uint uFormat, IntPtr data);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("Kernel32.dll", SetLastError = true)]
    private static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("Kernel32.dll", SetLastError = true)]
    private static extern int GlobalSize(IntPtr hMem);

    public static void SetText(string text) {
        OpenClipboard(IntPtr.Zero);

        var ptr = Marshal.StringToHGlobalUni(text);

        SetClipboardData(13, ptr);
        CloseClipboard();

        //Marshal.FreeHGlobal(ptr);
    }

    public static bool TryGetBitmap(out Bitmap bitmap) {
        bitmap = null;

        if (!IsClipboardFormatAvailable(CF_BITMAP)) {
            return false;
        }

        try {
            if (!OpenClipboard(IntPtr.Zero)) {
                return false;
            }

            IntPtr handle = GetClipboardData(CF_BITMAP);

            if (handle == IntPtr.Zero) {
                return false;
            }

            IntPtr pointer = IntPtr.Zero;

            try {
                bitmap = FromHbitmap(handle);

                return true;
            }
            catch {
                return false;
            }
        }
        finally {
            CloseClipboard();
        }
    }
}