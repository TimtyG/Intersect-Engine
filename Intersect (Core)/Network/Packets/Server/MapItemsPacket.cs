﻿using MessagePack;

namespace Intersect.Network.Packets.Server;

[MessagePackObject]
public partial class MapItemsPacket : IntersectPacket
{
    //Parameterless Constructor for MessagePack
    public MapItemsPacket()
    {
    }


    [Key(0)]
    public Guid MapId;

    public MapItemsPacket(Guid mapId, MapItemUpdatePacket[] items)
    {
        MapId = mapId;
        Items = items;
    }

    [Key(1)]
    public MapItemUpdatePacket[] Items { get; set; }

}