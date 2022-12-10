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

    internal static Configuration Config;
    internal static ImageChat Instance;
    internal static List<Color> CacheColors;

    public override void Load() {
        CacheColors = new List<Color>();
        Instance = this;

        // �Զ�������
        if (!Config.AutoClear || !Directory.Exists(FolderName)) return;

        var folder = new DirectoryInfo(FolderName);
        // ��ȡ�ļ��������е��ļ�
        var fileList = folder.GetFiles();
        foreach (var file in fileList) {
            // �ж��ļ�����չ���Ƿ�Ϊ .png
            if (file.Extension == ".png") {
                file.Delete(); // ɾ��
            }
        }
    }

    public override void Unload() {
        CacheColors = null;
        Instance = null;
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI) {
        switch (reader.ReadByte()) {
            case 0: // ������
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
            case 1: // ��ɰ�
                if (Main.netMode is NetmodeID.Server) {
                    var p = GetPacket();
                    p.Write((byte) 1); // ������
                    p.Write(reader.ReadString());
                    p.Write(reader.ReadUInt16());
                    p.Write(reader.ReadUInt16());
                    p.Send(ignoreClient: whoAmI);
                }
                else {
                    string name = reader.ReadString();
                    ushort width = reader.ReadUInt16();
                    ushort height = reader.ReadUInt16();

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
        string name = $"<{Main.LocalPlayer.name}>";
        ushort width = (ushort) tex.Width;
        ushort height = (ushort) tex.Height;
        var colors = new Color[tex.Width * tex.Height];

        tex.GetData(0, new Rectangle(0, 0, width, height), colors, 0, width * height);

        int i = 0;

        while (true) {
            int end = Math.Min(i + 10000, colors.Length); // ����[i,end)�����ڵ�����Color

            var p = GetPacket();
            p.Write((byte) 0); // ������
            p.Write((ushort) (end - i)); // ���͵�Color����
            for (; i < end; i++) {
                p.Write(colors[i].PackedValue); // ����������ɫ
            }

            p.Send();

            if (end == colors.Length) {
                break;
            }
        }

        var finishPacket = GetPacket();
        finishPacket.Write((byte) 1); // ������
        finishPacket.Write(name);
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

        if (tex.Width > Config.MaximumWidth || tex.Height > Config.MaximumHeight) {
            MessageBox.Show(Language.GetTextValue("Mods.ImageChat.Common.ImageTooLarge", Config.MaximumWidth,
                Config.MaximumHeight), Language.GetTextValue("Mods.ImageChat.Common.Warn"));
            return;
        }

        // ������ȴ
        BasicsSystem.SendDelay = Config.SendCap;

        // ����ͼƬ
        Main.NewText($"<{Main.LocalPlayer.name}>");
        RemadeChatMonitorHooks.SendTexture(tex, path);

        // ���˷���
        if (Main.netMode is NetmodeID.MultiplayerClient) {
            Instance.SendImagePacket(tex);
        }
    }

    public static Texture2D Bitmap2Tex2D(System.Drawing.Bitmap bm) {
        using var s = new MemoryStream();
        bm.Save(s, System.Drawing.Imaging.ImageFormat.Png);
        return Texture2D.FromStream(Main.instance.GraphicsDevice, s);
    }
}