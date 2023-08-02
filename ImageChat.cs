using ImageChat.Contents;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageChat.Core;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ImageChat;

public class ImageChat : Mod
{
    internal static string FolderName => Main.SavePath + Path.DirectorySeparatorChar + "Captures" +
                                         Path.DirectorySeparatorChar + "CachedImages" + Path.DirectorySeparatorChar;

    internal static Configuration Config;
    internal static ClientConfig ClientConfig;
    internal static ImageChat Instance;
    internal static List<byte> CachedData;

    public override void Load() {
        CachedData = new List<byte>();
        Instance = this;

        // 自动清理缓存
        if (!ClientConfig.AutoClear || !Directory.Exists(FolderName)) return;

        var folder = new DirectoryInfo(FolderName);
        // 获取文件夹下所有的文件
        var fileList = folder.GetFiles();
        foreach (var file in fileList) {
            // 判断文件的扩展名是否为 .png 或 .jpeg
            if (file.Extension is ".png" or ".jpeg") {
                file.Delete(); // 删除
            }
        }
    }

    public override void Unload() {
        CachedData = null;
        Instance = null;
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI) {
        switch (reader.ReadByte()) {
            case 0: // 传输中
                if (Main.netMode is NetmodeID.Server) {
                    var p = GetPacket();
                    ushort length = reader.ReadUInt16();
                    var data = reader.ReadBytes(length);
                    p.Write((byte) 0);
                    p.Write(length);
                    p.Write(data);
                    p.Send(ignoreClient: whoAmI);
                }
                else {
                    ushort length = reader.ReadUInt16();
                    var data = reader.ReadBytes(length);
                    CachedData.AddRange(data);
                }

                break;
            case 1: // 完成包
                if (Main.netMode is NetmodeID.Server) {
                    var p = GetPacket();
                    p.Write((byte) 1); // 包类型
                    p.Write(reader.ReadString());
                    p.Send(ignoreClient: whoAmI);
                }
                else {
                    string name = reader.ReadString();

                    if (!Utils.TryCreatingDirectory(FolderName))
                        break;

                    using var stream = CachedData.ToArray().ToMemoryStream();

                    Main.NewText(name);

                    string fileName = FolderName + DateTime.Now.ToFileTime() + ".png";
                    RemadeChatMonitorHooks.PostToChat(stream, fileName);
                    CachedData.Clear();
                }

                break;
        }
    }

    public void SendImagePacket(MemoryStream stream) {
        string name = $"<{Main.LocalPlayer.name}>";

        // 发包
        var imageBytes = stream.ToArray();
        Main.NewText(imageBytes.Length);

        const int batchSize = 50000;
        int totalBytes = imageBytes.Length;
        int startIndex = 0;

        while (startIndex < totalBytes) {
            int endIndex = Math.Min(startIndex + batchSize, totalBytes); // 发送[startIndex, endIndex)索引内的所有byte
            var data = imageBytes[startIndex..endIndex];

            var p = GetPacket();
            p.Write((byte) 0); // 包类型
            p.Write((ushort) data.Length); // byte数组长度
            p.Write(data); // 数据
            p.Send();

            startIndex = endIndex;
        }

        var finishPacket = GetPacket();
        finishPacket.Write((byte) 1); // 包类型
        finishPacket.Write(name);
        finishPacket.Send();
    }

    public static void LocalSendImage(MemoryStream imageStream, string path) {
        if (imageStream.Length > Config.MaximumFileSize * 1024 * 1024) {
            string warning = Language.GetTextValue("Mods.ImageChat.ImageTooLarge", Config.MaximumFileSize);
            if (ClientConfig.WindowWarning) {
                MessageBox.Show(warning, Language.GetTextValue("Mods.ImageChat.Warn"));
            }
            else {
                Main.NewText(warning, G: 50, B: 50);
            }

            return;
        }

        if (BasicsSystem.SendDelay > 0) {
            string warning = Language.GetTextValue("Mods.ImageChat.Wait", BasicsSystem.SendDelay.ToString("F1"));
            if (ClientConfig.WindowWarning) {
                MessageBox.Show(warning, Language.GetTextValue("Mods.ImageChat.Warn"));
            }
            else {
                Main.NewText(warning, G: 50, B: 50);
            }

            return;
        }

        // 设置冷却
        BasicsSystem.SendDelay = Config.SendCap;

        // 发送图片
        Main.NewText($"<{Main.LocalPlayer.name}>");
        RemadeChatMonitorHooks.PostToChat(imageStream, path);

        // 多人发包
        if (Main.netMode is NetmodeID.MultiplayerClient) {
            Instance.SendImagePacket(imageStream);
        }
    }
}