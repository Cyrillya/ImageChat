using ImageChat.Contents;
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
        var imageButton =
            new UIImageButton(ModContent.Request<Texture2D>("ImageChat/Images/Directory",
                AssetRequestMode.ImmediateLoad)) {
                Top = new StyleDimension(-37, 1),
                Left = new StyleDimension(-160, 1),
            };
        imageButton.OnClick += delegate {
            if (BasicsSystem.SendDelay > 0) {
                MessageBox.Show(Language.GetTextValue("Mods.ImageChat.Common.Wait", BasicsSystem.SendDelay.ToString("F1")),
                    Language.GetTextValue("Mods.ImageChat.Common.Warn"));
                return;
            }

            SoundEngine.PlaySound(SoundID.MenuOpen);

            var extensions = new ExtensionFilter[] {
                new("Image files", "png", "jpg", "jpeg")
            };

            string path =
                FileBrowser.OpenFilePanel(Language.GetTextValue("Mods.ImageChat.Common.SelectImage"), extensions);
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

                var config = ImageChat.Config;
                if (tex.Width > config.MaximumWidth || tex.Height > config.MaximumHeight) {
                    MessageBox.Show(
                        Language.GetTextValue("Mods.ImageChat.Common.ImageTooLarge", config.MaximumWidth,
                            config.MaximumHeight), Language.GetTextValue("Mods.ImageChat.Common.Warn"));
                    return;
                }

                // 设置冷却
                BasicsSystem.SendDelay = config.SendCap;

                // 发送图片
                Main.NewText($"<{Main.LocalPlayer.name}>");
                RemadeChatMonitorHooks.SendTexture(tex, path);

                // 多人发包
                if (Main.netMode is NetmodeID.MultiplayerClient) {
                    ImageChat.Instance.SendImagePacket(tex);
                }
            }
        };
        imageButton.OnUpdate += element => {
            if (!element.IsMouseHovering) return;

            Main.LocalPlayer.mouseInterface = true;
            Main.LocalPlayer.cursorItemIconText = Language.GetTextValue("Mods.ImageChat.Common.SelectImage");
        };
        Append(imageButton);
    }
}