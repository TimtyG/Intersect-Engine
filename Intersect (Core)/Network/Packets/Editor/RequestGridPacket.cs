﻿using MessagePack;

namespace Intersect.Network.Packets.Editor;

[MessagePackObject]
public partial class RequestGridPacket : EditorPacket
{
    //Parameterless Constructor for MessagePack
    public RequestGridPacket()
    {
    }

    public RequestGridPacket(Guid mapId)
    {
        MapId = mapId;
    }

    [Key(0)]
    public Guid MapId { get; set; }

}
