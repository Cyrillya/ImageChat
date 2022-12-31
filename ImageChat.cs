using ImageChat.Contents;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ImageChat;

public class ImageChat : Mod
{
    internal static string FolderName => Main.SavePath + Path.DirectorySeparatorChar + "Captures" +
                                         Path.DirectorySeparatorChar + "CachedImages" + Path.DirectorySeparatorChar;

    internal static ServerConfiguration ServerConfig;
    internal static ClientConfiguration ClientConfig;
    internal static ImageChat Instance;
    internal static List<Color> CacheColors;

    public static string GenerateFileName() =>
        FolderName + DateTime.Now.ToFileTime() + ".png";

    public override void Load() {
        CacheColors = new List<Color>();
        Instance = this;

        // 自动清理缓存
        if (!ClientConfig.AutoClear || !Directory.Exists(FolderName)) return;

        var folder = new DirectoryInfo(FolderName);
        // 获取文件夹下所有的文件
        var fileList = folder.GetFiles();
        foreach (var file in fileList) {
            // 判断文件的扩展名是否为 .png
            if (file.Extension == ".png") {
                file.Delete(); // 删除
            }
        }
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

                    p.Send(ignoreClient: whoAmI);
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
                byte sender = reader.ReadByte();
                ushort width = reader.ReadUInt16();
                ushort height = reader.ReadUInt16();
                string name = $"<{Main.player[sender].name}>";
                if (Main.netMode is NetmodeID.Server) {
                    var p = GetPacket();
                    p.Write((byte) 1); // 包类型
                    p.Write(sender);
                    p.Write(width);
                    p.Write(height);
                    p.Send(ignoreClient: whoAmI);
                    Console.WriteLine($"{name} [Image]");
                }
                else {
                    if (!Utils.TryCreatingDirectory(FolderName))
                        break;

                    var tex = new Texture2D(Main.graphics.GraphicsDevice, width, height);
                    tex.SetData(0, new Rectangle(0, 0, width, height), CacheColors.ToArray(), 0, width * height);

                    Main.NewText(name);

                    string fileName = FolderName + DateTime.Now.ToFileTime() + ".png";
                    RemadeChatMonitorHooks.SendTexture(tex, fileName);
                    CacheColors.Clear();
                }

                break;
        }
    }

    public void SendImagePacket(Texture2D tex) {
        // string name = $"<{Main.LocalPlayer.name}>";
        ushort width = (ushort) tex.Width;
        ushort height = (ushort) tex.Height;
        var colors = new Color[tex.Width * tex.Height];

        tex.GetData(0, new Rectangle(0, 0, width, height), colors, 0, width * height);

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
        finishPacket.Write((byte) Main.LocalPlayer.whoAmI);
        finishPacket.Write(width);
        finishPacket.Write(height);
        finishPacket.Send();
    }

    public static void LocalSendImage(Texture2D tex, string path) {
        if (BasicsSystem.SendDelay > 0) {
            MessageBox.Show(Language.GetTextValue("Mods.ImageChat.Common.Wait", BasicsSystem.SendDelay.ToString("F1")),
                Language.GetTextValue("Mods.ImageChat.Common.Warn"));
            return;
        }

        if (tex.Width > ServerConfig.MaximumWidth || tex.Height > ServerConfig.MaximumHeight) {
            MessageBox.Show(Language.GetTextValue("Mods.ImageChat.Common.ImageTooLarge", ServerConfig.MaximumWidth,
                ServerConfig.MaximumHeight), Language.GetTextValue("Mods.ImageChat.Common.Warn"));
            return;
        }

        // 设置冷却
        BasicsSystem.SendDelay = ServerConfig.SendCap;

        // 发送图片
        Main.NewText($"<{Main.LocalPlayer.name}>");
        RemadeChatMonitorHooks.SendTexture(tex, path);

        // 多人发包
        if (Main.netMode is NetmodeID.MultiplayerClient) {
            Instance.SendImagePacket(tex);
        }
    }
}