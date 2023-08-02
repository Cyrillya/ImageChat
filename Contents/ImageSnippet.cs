using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Terraria;
using Terraria.Localization;
using Terraria.UI.Chat;
using Color = Microsoft.Xna.Framework.Color;

namespace ImageChat.Contents;

public class ImageSnippet : TextSnippet
{
    public readonly Texture2D Texture;
    private readonly string _imagePath;

    public ImageSnippet(Stream imageStream, string imagePath) {
        Texture = Texture2D.FromStream(Main.graphics.GraphicsDevice, imageStream);
        _imagePath = imagePath;
        Scale = 1f;

        float availableWidth = 270;
        int width = Texture.Width;
        int height = Texture.Height;
        if (width > availableWidth || height > availableWidth) {
            if (width > height) {
                Scale = availableWidth / width;
            }
            else {
                Scale = availableWidth / height;
            }
        }
    }

    public override void OnHover() {
        if (!Main.drawingPlayerChat) return;

        string open = Language.GetTextValue("Mods.ImageChat.OpenImage");
        Main.instance.MouseText($"{open}");
        Main.LocalPlayer.mouseInterface = true;
    }

    public override void OnClick() {
        if (!Main.drawingPlayerChat) return;

        try {
            if (!File.Exists(_imagePath)) {
                using var stream = File.Create(_imagePath);
                Texture.SaveAsPng(stream, Texture.Width, Texture.Height);
            }

            var process = new Process();
            process.StartInfo.FileName = _imagePath;
            process.StartInfo.Arguments = "rundl132.exe C://WINDOWS//system32//shimgvw.dll";
            process.StartInfo.UseShellExecute = true;
            process.Start();
            process.Close();
        }
        catch (Exception e) {
            ImageChat.Instance.Logger.Error(e.Message);
            throw;
        }
    }

    public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch,
        Vector2 position = default, Color color = default, float scale = 1f) {
        if (!justCheckingString && color != Color.Black) {
            spriteBatch.Draw(Texture, position, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }

        size = Texture.Size() * scale; // 这里拿来作间隔的，GetStringLength不知道拿来干啥的反正绘制没用
        return true;
    }

    public override float GetStringLength(DynamicSpriteFont font) => Texture.Width * Scale;
}