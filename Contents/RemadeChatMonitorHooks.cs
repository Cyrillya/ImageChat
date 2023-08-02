using ImageChat.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Chat;
using Terraria.ModLoader;
using Terraria.UI.Chat;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace ImageChat.Contents;

public class RemadeChatMonitorHooks : ModSystem
{
    private static Dictionary<string, FieldInfo> _fields;
    private static Dictionary<string, FieldInfo> _msgContainerFields;

    private static void HandleClipboardImage() {
        if (!Main.inputText.IsKeyDown(Keys.LeftControl) && !Main.inputText.IsKeyDown(Keys.RightControl)) return;
        if (!Main.inputText.IsKeyDown(Keys.V) || Main.oldInputText.IsKeyDown(Keys.V)) return;
        if (!NativeMethods.ClipboardTryGetBitmap(out var bitmap)) return;

        using var s = bitmap.SaveToStream();

        if (!Utils.TryCreatingDirectory(ImageChat.FolderName)) {
            s.Close();
            return;
        }

        string fileName = ImageChat.FolderName + DateTime.Now.ToFileTime() + ".png";

        ImageChat.LocalSendImage(s, fileName);
    }

    internal static void PostToChat(MemoryStream stream, string filePath) {
        var msgContainer = new ChatMessageContainer();
        msgContainer.SetContents(" ", Color.White, -1);

        if (_msgContainerFields["_parsedText"].GetValue(msgContainer) is not List<TextSnippet[]> textSnippetsList ||
            _fields["_messages"].GetValue(Main.chatMonitor) is not List<ChatMessageContainer> msgContainersList) {
            return;
        }

        textSnippetsList = new List<TextSnippet[]> {
            new TextSnippet[] {new ImageSnippet(stream, filePath)}
        };
        _msgContainerFields["_parsedText"].SetValue(msgContainer, textSnippetsList);

        msgContainersList.Insert(0, msgContainer);
        while (msgContainersList.Count > 500) {
            msgContainersList.RemoveAt(msgContainersList.Count - 1);
        }

        _fields["_messages"].SetValue(Main.chatMonitor, msgContainersList);
    }

    public override void Load() {
        var fields = typeof(RemadeChatMonitor).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
        var msgContainerFields = typeof(ChatMessageContainer).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        _fields = new Dictionary<string, FieldInfo>();
        _msgContainerFields = new Dictionary<string, FieldInfo>();

        foreach (var f in fields) {
            _fields[f.Name] = f;
        }

        foreach (var f in msgContainerFields) {
            _msgContainerFields[f.Name] = f;
        }

        // 聊天框绘制
        On_RemadeChatMonitor.DrawChat += HookDrawChat;

        // 刷新时不应把存好的 ImageSnippet 刷新掉
        On_ChatMessageContainer.Refresh += (orig, msgContainer) => {
            if (msgContainer.Prepared ||
                _msgContainerFields["_parsedText"].GetValue(msgContainer) is not List<TextSnippet[]> {
                    Count: 1
                } textSnippetsList || textSnippetsList[0].Length is not 1 ||
                textSnippetsList[0][0] is not ImageSnippet) {
                orig.Invoke(msgContainer);
                return;
            }

            // 另开一个List，因为 textSnippetsList 是引用类型
            var savedList = new List<TextSnippet[]> {
                new[] {textSnippetsList[0][0]}
            };
            orig.Invoke(msgContainer);
            _msgContainerFields["_parsedText"].SetValue(msgContainer, savedList);
        };
    }

    private static void HookDrawChat(On_RemadeChatMonitor.orig_DrawChat orig, RemadeChatMonitor self,
        bool drawingPlayerChat) {
        // orig.Invoke(self, drawingPlayerChat);
        int showCount = (int) _fields["_showCount"].GetValue(self)!;
        int startChatLine = (int) _fields["_startChatLine"].GetValue(self)!;
        var messages = _fields["_messages"].GetValue(Main.chatMonitor) as List<ChatMessageContainer>;

        int num2 = 0;
        int num3 = 0;
        while (startChatLine > 0 && num2 < messages!.Count) {
            int num4 = Math.Min(startChatLine, messages[num2].LineCount);
            startChatLine -= num4;
            num3 += num4;
            if (num3 == messages[num2].LineCount) {
                num3 = 0;
                num2++;
            }
        }

        int offsetY = 0;
        int line = 0;
        int? num6 = null;
        int snippetIndex = -1;
        int? num7 = null;
        while (line < showCount && num2 < messages.Count) {
            ChatMessageContainer chatMessageContainer = messages[num2];
            if (!chatMessageContainer.Prepared ||
                !(drawingPlayerChat | chatMessageContainer.CanBeShownWhenChatIsClosed))
                break;

            TextSnippet[] snippetWithInversedIndex = chatMessageContainer.GetSnippetWithInversedIndex(num3);

            bool isImage = snippetWithInversedIndex.Length > 0 && snippetWithInversedIndex[0] is ImageSnippet;

            if (isImage) {
                var imageSnippet = snippetWithInversedIndex[0] as ImageSnippet;
                offsetY += (int) (imageSnippet!.Texture.Height * imageSnippet.Scale) + 6;
            }
            else {
                offsetY += 21;
            }

            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, FontAssets.MouseText.Value,
                snippetWithInversedIndex, new Vector2(88f, Main.screenHeight - 36 - offsetY), 0f, Vector2.Zero,
                Vector2.One, out int hoveredSnippet);

            if (hoveredSnippet >= 0) {
                num7 = hoveredSnippet;
                num6 = num2;
                snippetIndex = num3;
            }

            // 防止刷屏 & 只绘制了一个图片的话不断掉(也就是不包括人名不断掉)
            if (offsetY > 220 && !isImage) {
                break;
            }

            line++;
            num3++;
            if (num3 >= chatMessageContainer.LineCount) {
                num3 = 0;
                num2++;
            }
        }

        if (num6.HasValue) {
            TextSnippet[] snippetWithInversedIndex2 = messages[num6.Value].GetSnippetWithInversedIndex(snippetIndex);
            snippetWithInversedIndex2[num7.Value].OnHover();
            if (Main.mouseLeft && Main.mouseLeftRelease)
                snippetWithInversedIndex2[num7.Value].OnClick();
        }

        HandleClipboardImage();
    }

    public override void Unload() {
        _fields = null;
        _msgContainerFields = null;
    }
}