﻿using MessagePack;

namespace Intersect.Network.Packets.Server;

[MessagePackObject]
public partial class PlayAnimationPackets : IntersectPacket
{
    //Parameterless Constructor for MessagePack
    public PlayAnimationPackets()
    {
    }

    public PlayAnimationPackets(PlayAnimationPacket[] packets)
    {
        Packets = packets;
    }

    [Key(0)]
    public PlayAnimationPacket[] Packets { get; set; }

}
