using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImageChat.Contents;

public class Configuration : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    public override void OnLoaded() {
        ImageChat.Config = this;
    }
    
    [DefaultValue(1f)]
    [Range(0.5f, 10f)]
    [Increment(0.1f)]
    public float MaximumFileSize;

    [DefaultValue(1)]
    [Range(1, 60)]
    public int SendCap;
}

public class ClientConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    public override void OnLoaded() {
        ImageChat.ClientConfig = this;
    }
    
    [DefaultValue(false)]
    public bool WindowWarning;

    [DefaultValue(true)]
    public bool AutoClear;

    [DefaultValue(true)]
    public bool ScreenshotClipboard;

    [DefaultValue(true)]
    public bool ScreenshotToChat;
}