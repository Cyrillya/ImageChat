using ImageChat.Contents;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Windows.Forms;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
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
        float x = ModLoader.HasMod("BetterChat") ? -110 : -220;
        var imageButton = new UIImageButton(TextureAssets.Camera[6]) {
                Top = new StyleDimension(-37, 1),
                Left = new StyleDimension(x, 1),
            };
        imageButton.OnLeftClick += delegate {
            if (BasicsSystem.SendDelay > 0) {
                MessageBox.Show(Language.GetTextValue("Mods.ImageChat.Wait", BasicsSystem.SendDelay.ToString("F1")),
                    Language.GetTextValue("Mods.ImageChat.Warn"));
                return;
            }

            SoundEngine.PlaySound(SoundID.MenuOpen);

            var extensions = new ExtensionFilter[] {
                new("Image files", "png", "jpg", "jpeg")
            };

            string path =
                FileBrowser.OpenFilePanel(Language.GetTextValue("Mods.ImageChat.SelectImage"), extensions);
            if (path == null) return;

            try {
                byte[] data = File.ReadAllBytes(path);
                using var stream = new MemoryStream(data);

                ImageChat.LocalSendImage(stream, path);
            }
            catch (Exception exception) {
                FancyErrorPrinter.ShowFailedToLoadAssetError(exception, path);
            }
        };
        imageButton.OnUpdate += element => {
            if (!element.IsMouseHovering) return;

            Main.LocalPlayer.mouseInterface = true;
            Main.instance.MouseText(Language.GetTextValue("Mods.ImageChat.SelectImage"));
        };
        Append(imageButton);
    }
}