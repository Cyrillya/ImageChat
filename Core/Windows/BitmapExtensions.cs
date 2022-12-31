using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using ImageChat.Contents;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace ImageChat.Core.Windows;

[SuppressMessage("Interoperability", "CA1416:验证平台兼容性")]
public static class BitmapExtensions
{
    public static Texture2D Bitmap2Tex2D(this Bitmap bm) {
        using var s = new MemoryStream();
        bm.Save(s, System.Drawing.Imaging.ImageFormat.Png);
        return Texture2D.FromStream(Main.instance.GraphicsDevice, s);
    }

    /// <summary>
    /// 修改 Bitmap 尺寸 避免内存溢出
    /// </summary>
    /// <param name="bmp">Bitmap原图</param>
    /// <returns></returns>
    public static Bitmap ResizeImage(this Bitmap bmp) {
        if (ImageChat.ServerConfig.Resize is ServerConfiguration.ResizeLevel.Full)
            return bmp;

        try {
            var (newW, newH) = bmp.GetSize();
            var b = new Bitmap(newW, newH);
            var g = Graphics.FromImage(b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(bmp, new Rectangle(0, 0, newW, newH), new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                GraphicsUnit.Pixel);
            g.Dispose();
            return b;
        }
        catch {
            return bmp;
        }
    }

    private static (int w, int h) GetSize(this Image bmp) {
        if (bmp.Width < 32 || bmp.Height < 32) return (bmp.Width, bmp.Height);

        return ImageChat.ServerConfig.Resize switch {
            ServerConfiguration.ResizeLevel.ThreeQuarters => ((int) (bmp.Width * 0.75), (int) (bmp.Height * 0.75)),
            ServerConfiguration.ResizeLevel.Half => (bmp.Width / 2, bmp.Height / 2),
            ServerConfiguration.ResizeLevel.Quarter => (bmp.Width / 4, bmp.Height / 4),
            _ => (bmp.Width, bmp.Height)
        };
    }
}