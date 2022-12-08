using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.UI;
using Terraria.ModLoader;

namespace ImageChat.UI;

public class UISystem : ModSystem
{
    internal UserInterface UserInterface;

    public override void Load() {
        if (!Main.dedServ) {
            UserInterface = new UserInterface();
            UserInterface.SetState(new ImageChatUI());
        }

        base.Load();
    }

    public override void Unload() {
        UserInterface = null;

        base.Unload();
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