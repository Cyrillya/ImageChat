using ImageChat.Contents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImageChat;

public class ImageChat : Mod
{
    public static ImageChat Instance;
    public static List<Color> CacheColors;

    public override void Load() {
        CacheColors = new List<Color>();
        Instance = this;
    }

    public override void Unload() {
        CacheColors = null;
        Instance = null;
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI) {
        switch (reader.ReadByte()) {
            case 0: // 传输中
                if (Main.netMode is NetmodeID.Server) {
                    var p = GetPacket();
                    ushort length = reader.ReadUInt16();
                    p.Write((byte) 0);
                    p.Write(length);
                    for (int i = 0; i < length; i++) {
                        p.Write(reader.ReadUInt32());
                    }

                    p.Send();
                }
                else {
                    ushort length = reader.ReadUInt16();
                    for (int i = 0; i < length; i++) {
                        CacheColors.Add(new Color {
                            PackedValue = reader.ReadUInt32()
                        });
                    }
                }

                break;
            case 1: // 完成包
                if (Main.netMode is NetmodeID.Server) {
                    var p = GetPacket();
                    p.Write((byte) 1); // 包类型
                    p.Write(reader.ReadString());
                    p.Write(reader.ReadUInt16());
                    p.Write(reader.ReadUInt16());
                    p.Send();
                }
                else {
                    string name = reader.ReadString();
                    ushort width = reader.ReadUInt16();
                    ushort height = reader.ReadUInt16();

                    var tex = new Texture2D(Main.graphics.GraphicsDevice, width, height);
                    tex.SetData(0, new Rectangle(0, 0, width, height), CacheColors.ToArray(), 0, width * height);
                    Main.NewText(name);
                    RemadeChatMonitorHooks.SendTexture(tex);
                    CacheColors.Clear();
                }

                break;
        }
    }

    public void SendImagePacket(Texture2D tex) {
        string name = $"<{Main.LocalPlayer.name}>";
        ushort width = (ushort) tex.Width;
        ushort height = (ushort) tex.Height;
        var colors = new Color[tex.Width * tex.Height];
        tex.GetData(colors);
        int i = 0;
        
        while (true) {
            int end = Math.Min(i + 10000, colors.Length); // 发送[i,end)索引内的所有Color

            var p = GetPacket();
            p.Write((byte) 0); // 包类型
            p.Write((ushort) (end - i)); // 发送的Color数量
            for (; i < end; i++) {
                p.Write(colors[i].PackedValue); // 发送所有颜色
            }

            p.Send();

            if (end == colors.Length) {
                break;
            }
        }

        var finishPacket = GetPacket();
        finishPacket.Write((byte) 1); // 包类型
        finishPacket.Write(name);
        finishPacket.Write(width);
        finishPacket.Write(height);
        finishPacket.Send();
    }
}