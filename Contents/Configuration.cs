using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImageChat.Contents;

public class Configuration : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    public override void OnLoaded() {
        ImageChat.Config = this;
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

    [Label("$Mods.ImageChat.Config.AutoClear.Label")]
    [Tooltip("$Mods.ImageChat.Config.AutoClear.Tooltip")]
    [DefaultValue(true)]
    public bool AutoClear;
}