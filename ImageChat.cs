using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using NetSimplified;
using Terraria.ModLoader;

namespace ImageChat;

public class ImageChat : Mod
{
    public override void Load() {
        AddContent<NetModuleLoader>();
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI) {
        NetModule.ReceiveModule(reader, whoAmI);
    }
}