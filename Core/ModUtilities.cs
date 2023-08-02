using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Color = System.Drawing.Color;
using Rectangle = System.Drawing.Rectangle;

namespace ImageChat.Core;

public static class ModUtilities
{
    public static MemoryStream SaveToStream(this Bitmap bitmap) {
        var s = new MemoryStream();
        bitmap.Save(s, ImageFormat.Png);
        return s;
    }

    public static void DrawBorder(Vector2 position, float width, float height, Microsoft.Xna.Framework.Color backgroundColor,
        Microsoft.Xna.Framework.Color borderColor) {
        Texture2D texture = TextureAssets.MagicPixel.Value;
        Vector2 scale = new(width, height);
        Main.spriteBatch.Draw(
            texture,
            position,
            new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1),
            backgroundColor,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(
            texture,
            position + Vector2.UnitX * -2f + Vector2.UnitY * -2f,
            new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1),
            borderColor, 0f, Vector2.Zero,
            new Vector2(2f, scale.Y + 4),
            SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + Vector2.UnitX * scale.X + Vector2.UnitY * -2f,
            new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1),
            borderColor, 0f, Vector2.Zero,
            new Vector2(2f, scale.Y + 4), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + Vector2.UnitY * -2f,
            new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1),
            borderColor, 0f, Vector2.Zero,
            new Vector2(scale.X, 2f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + Vector2.UnitY * scale.Y,
            new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1),
            borderColor, 0f, Vector2.Zero,
            new Vector2(scale.X, 2f), SpriteEffects.None, 0f);
    }
}