using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImageChat.Contents;

[Label("$Mods.ImageChat.Config.ServerConfiguration")]
public class ServerConfiguration : ModConfig
{
    public enum ResizeLevel
    {
        [Label("100%")] Full,
        [Label("75%")] ThreeQuarters,
        [Label("50%")] Half,
        [Label("25%")] Quarter
    }
    
    public override ConfigScope Mode => ConfigScope.ServerSide;

    public override void OnLoaded() {
        ImageChat.ServerConfig = this;
    }
    
    [Label("$Mods.ImageChat.Config.MaximumWidth.Label")]
    [Tooltip("$Mods.ImageChat.Config.MaximumWidth.Tooltip")]
    [DefaultValue(2000)]
    [Range(5, 2000)]
    public int MaximumWidth;

    [Label("$Mods.ImageChat.Config.MaximumHeight.Label")]
    [Tooltip("$Mods.ImageChat.Config.MaximumHeight.Tooltip")]
    [DefaultValue(2000)]
    [Range(5, 2000)]
    public int MaximumHeight;

    [Label("$Mods.ImageChat.Config.SendCap.Label")]
    [Tooltip("$Mods.ImageChat.Config.SendCap.Tooltip")]
    [DefaultValue(1)]
    [Range(1, 60)]
    public int SendCap;
    
    [Label("$Mods.ImageChat.Config.Resize.Label")]
    [Tooltip("$Mods.ImageChat.Config.Resize.Tooltip")]
    [DefaultValue(ResizeLevel.ThreeQuarters)]
    [DrawTicks]
    public ResizeLevel Resize;
}

[Label("$Mods.ImageChat.Config.ClientConfiguration")]
public class ClientConfiguration : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    public override void OnLoaded() {
        ImageChat.ClientConfig = this;
    }

    [Label("$Mods.ImageChat.Config.AutoClear.Label")]
    [Tooltip("$Mods.ImageChat.Config.AutoClear.Tooltip")]
    [DefaultValue(true)]
    public bool AutoClear;

    [Label("$Mods.ImageChat.Config.ScreenshotClipboard.Label")]
    [Tooltip("$Mods.ImageChat.Config.ScreenshotClipboard.Tooltip")]
    [DefaultValue(true)]
    public bool ScreenshotClipboard;

    [Label("$Mods.ImageChat.Config.ScreenshotToChat.Label")]
    [Tooltip("$Mods.ImageChat.Config.ScreenshotToChat.Tooltip")]
    [DefaultValue(true)]
    public bool ScreenshotToChat;
}