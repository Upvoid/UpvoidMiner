using System;

namespace UpvoidMiner
{
    /// <summary>
    /// Items that can be discretely stackes.
    /// </summary>
    public class DiscreteItem : Item
    {
        /// <summary>
        /// Amount of items of this type.
        /// </summary>
        public int StackSize = 1;

        public DiscreteItem(string name, string description, float weight, bool isUsable, ItemCategory category) :
            base(name, description, weight, isUsable, category)
        {
        }
    }
}

