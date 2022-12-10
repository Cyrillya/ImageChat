using ImageChat.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;

namespace ImageChat.Contents;

public class BasicsSystem : ModSystem
{
    private int _screenshotTimer;
    private System.Drawing.Point _startPointWindow; // 在计算机上的鼠标位置
    private System.Drawing.Point _endPointWindow; // 在计算机上的鼠标位置
    private Vector2 _startPosition;
    private Vector2 _endPosition;
    private bool _rangeSelecting;
    internal static bool Screenshoting;
    internal static float SendDelay;
    internal static ModKeybind ScrnshotKeybind { get; private set; }
    internal static BasicsSystem Instance;

    public override void Load() {
        ScrnshotKeybind = KeybindLoader.RegisterKeybind(Mod, "Screenshot", "OemPipe");
        Instance = this;
    }

    public override void Unload() {
        ScrnshotKeybind = null;
        Instance = null;
    }

    public override void UpdateUI(GameTime gameTime) {
        SendDelay -= (float) gameTime.ElapsedGameTime.TotalSeconds;
    }

    #region Screenshot

    public void OnEnterWorld() {
        Screenshoting = false;
        _rangeSelecting = false;
        _screenshotTimer = -1;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
        int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        if (mouseTextIndex != -1) {
            layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("ImageChat: Screenshot Border", delegate {
                    if (!Screenshoting) {
                        return true;
                    }

                    if (Main.mouseRight) {
                        Screenshoting = false;
                        _rangeSelecting = false;
                        return true;
                    }

                    if (!_rangeSelecting) {
                        if (Main.mouseLeft) {
                            _rangeSelecting = true;
                            _startPosition = Main.MouseScreen;
                            _startPointWindow = NativeMethods.GetCursorPosition();
                        }
                        else {
                            return true;
                        }
                    }

                    if (_screenshotTimer is -1) {
                        _endPosition = Main.MouseScreen;
                        _endPointWindow = NativeMethods.GetCursorPosition();
                    }

                    if (_screenshotTimer > 0) {
                        _screenshotTimer--;
                        return true;
                    }

                    if (_screenshotTimer is 0) {
                        Screenshoting = false;
                        _rangeSelecting = false;
                        _screenshotTimer = -1;

                        // 截屏区域
                        var leftTop = new System.Drawing.Point(Math.Min(_startPointWindow.X, _endPointWindow.X),
                            Math.Min(_startPointWindow.Y, _endPointWindow.Y));
                        var size = new System.Drawing.Size(Math.Max(_startPointWindow.X, _endPointWindow.X) - leftTop.X,
                            Math.Max(_startPointWindow.Y, _endPointWindow.Y) - leftTop.Y);

                        if (size.Width <= 0 || size.Height <= 0) return true;

                        // 截屏
                        var bm = NativeMethods.CaptureRectangleNative(new System.Drawing.Rectangle(leftTop, size));

                        // 发送截屏
                        if (!ImageChat.Config.ScreenshotToChat) return true;

                        var tex = ImageChat.Bitmap2Tex2D(bm);
                        if (!Utils.TryCreatingDirectory(ImageChat.FolderName)) return true;

                        ImageChat.LocalSendImage(tex, ImageChat.FolderName + DateTime.Now.ToFileTime() + ".png");
                        return true;
                    }

                    if (Main.mouseLeftRelease && !Main.mouseLeft) {
                        _screenshotTimer = 2;
                        return true;
                    }

                    var leftTopDrawing = new Vector2((int) Math.Min(_startPosition.X, _endPosition.X),
                        (int) Math.Min(_startPosition.Y, _endPosition.Y));
                    int width = (int) (Math.Max(_startPosition.X, _endPosition.X) - leftTopDrawing.X);
                    int height = (int) (Math.Max(_startPosition.Y, _endPosition.Y) - leftTopDrawing.Y);

                    var color = Color.GreenYellow;
                    DrawBorder(new Vector2(leftTopDrawing.X, leftTopDrawing.Y), width, height, color * 0.35f, color);

                    return true;
                },
                InterfaceScaleType.UI)
            );
        }
    }

    public static void DrawBorder(Vector2 position, float width, float height, Color backgroundColor,
        Color borderColor) {
        Texture2D texture = TextureAssets.MagicPixel.Value;
        Vector2 scale = new(width, height);
        Main.spriteBatch.Draw(
            texture,
            position,
            new Rectangle(0, 0, 1, 1),
            backgroundColor,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(
            texture,
            position + Vector2.UnitX * -2f + Vector2.UnitY * -2f,
            new Rectangle(0, 0, 1, 1),
            borderColor, 0f, Vector2.Zero,
            new Vector2(2f, scale.Y + 4),
            SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + Vector2.UnitX * scale.X + Vector2.UnitY * -2f,
            new Rectangle(0, 0, 1, 1),
            borderColor, 0f, Vector2.Zero,
            new Vector2(2f, scale.Y + 4), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + Vector2.UnitY * -2f,
            new Rectangle(0, 0, 1, 1),
            borderColor, 0f, Vector2.Zero,
            new Vector2(scale.X, 2f), SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(texture,
            position + Vector2.UnitY * scale.Y,
            new Rectangle(0, 0, 1, 1),
            borderColor, 0f, Vector2.Zero,
            new Vector2(scale.X, 2f), SpriteEffects.None, 0f);
    }

    #endregion
}