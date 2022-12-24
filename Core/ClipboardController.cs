﻿using ImageChat.Core.Osx;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.OS;

namespace ImageChat.Core;

internal static class ClipboardController
{
    internal static bool TryGetTexture2D(out Texture2D texture) {
        texture = null;

        if (Platform.IsWindows) {
            bool succeed = WindowsMethods.ClipboardTryGetBitmap(out var bitmap);
            if (!succeed) return false;

            texture = ImageChat.Bitmap2Tex2D(bitmap);
            return true;
        }

        if (Platform.IsOSX) {
            return OsxClipboard.TryGetClipboard(out texture);
        }

        return false;
    }
}