using System;

namespace UpvoidMiner
{
    /// <summary>
    /// Items that can be discretely stackes.
    /// </summary>
    public class DiscreteItem : Item
    {
        public DiscreteItem(string name, string description, float weight, bool isUsable, ItemCategory category) :
            base(name, description, weight, isUsable, category)
        {
        }
    }
}

