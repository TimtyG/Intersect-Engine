using Intersect.GameObjects;
using Intersect.Network.Packets.Server;

namespace Intersect.Client.Framework.Items;

public interface IItem
{
    Guid? BagId { get; set; }
    ItemBase Base { get; }
    Guid ItemId { get; set; }
    int Quantity { get; set; }
    ItemProperties ItemProperties { get; set; }

    void Load(Guid id, int quantity, Guid? bagId, ItemProperties itemProperties);
}
