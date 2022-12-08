using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.UI.Chat;

namespace ImageChat.Contents;

public class ImageSnippet : TextSnippet
{
    public Texture2D Texture;
    public float Scale;

    public ImageSnippet(Texture2D texture) {
        Texture = texture;
        Scale = 1f;

        float availableWidth = 100;
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

    public override bool UniqueDraw(bool justCheckingString, out Vector2 size, SpriteBatch spriteBatch,
        Vector2 position = default, Color color = default, float scale = 1f) {
        position.Y -= 21f;
        position.X += 4f;

        if (!justCheckingString && color != Color.Black) {
            spriteBatch.Draw(Texture, position, null, Color.White, 0f, Vector2.Zero, scale * Scale, SpriteEffects.None, 0f);
        }

        size = Texture.Size() * scale * Scale; // 这里拿来作间隔的，GetStringLength不知道拿来干啥的反正绘制没用
        return true;
    }

    public override float GetStringLength(DynamicSpriteFont font) => Texture.Width * Scale;
}