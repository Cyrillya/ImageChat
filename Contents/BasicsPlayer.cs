using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ImageChat.Contents;

public class BasicsPlayer : ModPlayer
{
    public override void OnEnterWorld() {
        BasicsSystem.Instance.OnEnterWorld();
    }

    public override void ProcessTriggers(TriggersSet triggersSet) {
        if (BasicsSystem.ScrnshotKeybind.JustPressed) {
            BasicsSystem.Screenshoting = true;
        }

        if (BasicsSystem.Screenshoting) {
            Player.mouseInterface = true;
        }
    }
}