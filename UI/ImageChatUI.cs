using System;
using System.Drawing;
using System.IO;
using ImageChat.Contents;
using ImageChat.Packets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.Utilities.FileBrowser;

namespace ImageChat.UI;

public class ImageChatUI : UIState
{
    public ImageChatUI() {
        var colorBtn = new UIImageButton(ModContent.Request<Texture2D>("ImageChat/Images/Directory", AssetRequestMode.ImmediateLoad)) {
            Top = new StyleDimension(-37, 1),
            Left = new StyleDimension(-160, 1),
        };
        colorBtn.OnClick += delegate {
            SoundEngine.PlaySound(SoundID.MenuOpen);

            var extensions = new ExtensionFilter[] {
                new("Image files", "png", "jpg", "jpeg")
            };

            string path = FileBrowser.OpenFilePanel("Select image", extensions);
            if (path != null) {
                Texture2D tex = null;
                try {
                    var stream = File.OpenRead(path);
                    tex = Texture2D.FromStream(Main.graphics.GraphicsDevice, stream);
                }
                catch (Exception exception) {
                    FancyErrorPrinter.ShowFailedToLoadAssetError(exception, path);
                }

                if (tex is null) return;

                Main.NewText($"<{Main.LocalPlayer.name}>");
                RemadeChatMonitorHooks.SendTexture(tex);

                if (Main.netMode is NetmodeID.MultiplayerClient) {
                    ImagePacket.Get(tex).Send();
                }
            }
        };
        Append(colorBtn);
    }

    public override void Update(GameTime gameTime) {
        if (IsMouseHovering) {
            Main.LocalPlayer.cursorItemIconText = "选择图片";
            Main.LocalPlayer.mouseInterface = true;
        }

        base.Update(gameTime);
    }
}