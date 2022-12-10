using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace ImageChat.UI;

public class UISystem : ModSystem
{
    internal UserInterface UserInterface;

    public override void PostSetupContent() {
        if (Main.dedServ) return;

        UserInterface = new UserInterface();
        UserInterface.SetState(new ImageChatUI());
    }

    public override void Unload() {
        if (Main.dedServ) return;

        UserInterface = null;
    }

    private GameTime _lastUpdateUiGameTime;

    public override void UpdateUI(GameTime gameTime) {
        _lastUpdateUiGameTime = gameTime;

        if (Main.drawingPlayerChat) {
            UserInterface.Update(gameTime);
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex == -1) return;

        layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
            "ImageChat: UI",
            delegate {
                if (Main.drawingPlayerChat) {
                    UserInterface.Draw(Main.spriteBatch, _lastUpdateUiGameTime);
                }

                return true;
            },
            InterfaceScaleType.UI)
        );
    }
}