using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Diagnostics;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.UI.Chat;

namespace ImageChat.Contents;

public class ImageSnippet : TextSnippet
{
    public Texture2D Texture;
    public readonly string ImagePath;

    public ImageSnippet(Texture2D texture, string imagePath) {
        Texture = texture;
        ImagePath = imagePath;
        Scale = 1f;
        RecalculateScale();
    }

    public void RecalculateScale() {
        float screenHeightRate = Main.screenHeight / 800f;
        float availableWidth = 150 * screenHeightRate;
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

        string open = Language.GetTextValue("Mods.ImageChat.Common.OpenImage");
        Main.instance.MouseText($"{open}");
    }

    public override void OnClick() {
        if (!Main.drawingPlayerChat) return;

        try {
            if (!File.Exists(ImagePath)) {
                using var stream = File.Create(ImagePath);
                Texture.SaveAsPng(stream, Texture.Width, Texture.Height);
            }

            var process = new Process();
            process.StartInfo.FileName = ImagePath;
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

    private void DrawImageNoZoom(SpriteBatch spriteBatch, Rectangle collisionRectangle) {
        if (!collisionRectangle.Contains(Main.MouseScreen.ToPoint())) return;

        var drawPosition = Main.MouseScreen;
        drawPosition.Y -= 8f;
        var origin = new Vector2(0, Texture.Height);
        float scale = 1f / Main.UIScale;
        spriteBatch.Draw(Texture, drawPosition, null, Color.White, 0f, origin, scale, SpriteEffects.None, 0f);
    }

    public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch,
        Vector2 position = default, Color color = default, float scale = 1f) {
        size = Texture.Size() * scale;

        if (!justCheckingString && color != Color.Black) {
            spriteBatch.Draw(Texture, position, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            // if (scale * Main.UIScale < 0.5f) // 缩略图太小，显示原图
            //     DrawImageNoZoom(spriteBatch, new Rectangle((int)position.X, (int)position.Y, (int)size.X, (int)size.Y));
        }

        return true;
    }

    public override float GetStringLength(DynamicSpriteFont font) => Texture.Width * Scale;
}