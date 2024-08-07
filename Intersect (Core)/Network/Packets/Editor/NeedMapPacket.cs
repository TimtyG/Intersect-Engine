﻿using MessagePack;

namespace Intersect.Network.Packets.Editor;

[MessagePackObject]
public partial class NeedMapPacket : EditorPacket
{
    //Parameterless Constructor for MessagePack
    public NeedMapPacket()
    {
    }

    public NeedMapPacket(Guid mapId)
    {
        MapId = mapId;
    }

    [Key(0)]
    public Guid MapId { get; set; }

}
