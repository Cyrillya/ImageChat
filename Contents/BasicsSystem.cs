using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ImageChat.Contents;

public class BasicsSystem : ModSystem
{
    internal static float SendDelay;

    public override void UpdateUI(GameTime gameTime) {
        SendDelay -= (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}