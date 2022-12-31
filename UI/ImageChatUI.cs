using ImageChat.Contents;
using ImageChat.Core.Windows;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
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
                    var bmp = new Bitmap(stream);
                    tex = bmp.ResizeImage().Bitmap2Tex2D();
                }
                catch (Exception exception) {
                    FancyErrorPrinter.ShowFailedToLoadAssetError(exception, path);
                }

                if (tex is null) return;

                ImageChat.LocalSendImage(tex, path);
            }
        };
        imageButton.OnUpdate += element => {
            if (!element.IsMouseHovering) return;

            Main.LocalPlayer.mouseInterface = true;
            Main.instance.MouseText(Language.GetTextValue("Mods.ImageChat.Common.SelectImage"));
        };
        Append(imageButton);
    }
}