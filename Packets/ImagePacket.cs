using System.Diagnostics;
using System.Drawing;
using System.IO;
using NetSimplified;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Runtime.Serialization;
using ImageChat.Contents;
using Microsoft.Xna.Framework.Graphics;
using NetSimplified.Syncing;
using XnaColor = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ImageChat.Packets;

public class ImagePacket : NetModule
{
    private Texture2D _tex;

    // private Bitmap _bitmap;
    [AutoSync] private string _name;
    [AutoSync] private ushort width;
    [AutoSync] private ushort height;

    public static ImagePacket Get(Texture2D tex) {
        var p = NetModuleLoader.Get<ImagePacket>();
        p._tex = tex;
        p._name = $"<{Main.LocalPlayer.name}>";
        p.width = (ushort) tex.Width;
        p.height = (ushort) tex.Height;
        return p;
    }

    public override void Send(ModPacket p) {
        Debug.Assert(_tex != null, nameof(_tex) + " != null");

        var colors = new XnaColor[width * height];
        _tex.GetData(colors);
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                p.WriteRGB(colors[x + y * width]);
            }
        }
    }

    public override void Read(BinaryReader r) {
        var colors = new XnaColor[width * height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                colors[x + y * width] = new XnaColor(r.ReadByte(), r.ReadByte(), r.ReadByte());
            }
        }

        _tex = new Texture2D(Main.graphics.GraphicsDevice, width, height);
        _tex.SetData(0, new Rectangle(0, 0, width, height), colors, 0, width * height);
    }

    public override void Receive() {
        Main.NewText(_name);
        RemadeChatMonitorHooks.SendTexture(_tex);
        
        if (Main.netMode is NetmodeID.Server) {
            Send(ignoreClient: Sender);
        }
    }
}