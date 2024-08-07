﻿using MessagePack;

namespace Intersect.Network.Packets.Client;

[MessagePackObject]
public partial class QuestResponsePacket : IntersectPacket
{
    //Parameterless Constructor for MessagePack
    public QuestResponsePacket()
    {
    }

    public QuestResponsePacket(Guid questId, bool accepting)
    {
        QuestId = questId;
        AcceptingQuest = accepting;
    }

    [Key(0)]
    public Guid QuestId { get; set; }

    [Key(1)]
    public bool AcceptingQuest { get; set; }

}
