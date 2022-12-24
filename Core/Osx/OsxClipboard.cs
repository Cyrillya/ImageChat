using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using Terraria;

namespace ImageChat.Core.Osx;

internal static class OsxClipboard
{
    internal static bool TryGetClipboard(out Texture2D texture) {
        texture = null;
        try {
            var fileName = ImageChat.GenerateFileName();
            while (File.Exists(fileName)) fileName += '1';

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo("pbpaste", $"> {fileName}") {
                UseShellExecute = false
            };

            process.Start();
            // var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (!File.Exists(fileName)) return false;

            var stream = File.OpenRead(fileName);
            texture = Texture2D.FromStream(Main.instance.GraphicsDevice, stream);
            return true;
        }
        catch (Exception) {
            return false;
        }
    }

    internal static void SetClipboard(Texture2D texture) {
        try {
            var fileName = ImageChat.GenerateFileName();
            while (File.Exists(fileName)) fileName += '1';

            using var stream = File.Create(fileName);
            texture.SaveAsPng(stream, texture.Width, texture.Height);

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo("pbcopy", $"< {fileName}") {
                UseShellExecute = false,
            };

            process.Start();
            // process.StandardInput.Close();
            process.WaitForExit();

            File.Delete(fileName);
        }
        catch (Exception) { }
    }
}