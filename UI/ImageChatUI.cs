using ImageChat.Contents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
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

                if (tex.Width > 2048 || tex.Height > 2048) {
                    string warn = "Your image is too large. Please select an image with a size below 2048x2048.";
                    if (Language.ActiveCulture.Name is "zh-Hans") {
                        warn = "图像过大。请选择尺寸低于2048x2048的图像。";
                    }
                    MessageBox.Show(warn, typeof(Main).GetField("_cachedTitle", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(Main.instance) as string);
                    return;
                }

                Main.NewText($"<{Main.LocalPlayer.name}>");
                RemadeChatMonitorHooks.SendTexture(tex);

                if (Main.netMode is NetmodeID.MultiplayerClient) {
                    ImageChat.Instance.SendImagePacket(tex);
                }
            }
        };
        Append(colorBtn);
    }

    public override void Update(GameTime gameTime) {
        if (IsMouseHovering) {
            Main.LocalPlayer.cursorItemIconText = "Select image";
            Main.LocalPlayer.mouseInterface = true;
        }

        base.Update(gameTime);
    }
}