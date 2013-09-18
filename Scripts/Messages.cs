using System;
using Engine.Universe;

namespace UpvoidMiner
{
    [Serializable]
    public class InteractionMessage
    {
        public Entity Sender;

        public InteractionMessage(Entity sender)
        {
            Sender = sender;
        }
    }

    [Serializable]
    public class AddItemMessage
    {
        public Item PickedItem;

        public AddItemMessage(Item pickedItem)
        {
            PickedItem = pickedItem;
        }
    }
}
