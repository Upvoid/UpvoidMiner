using System;

namespace UpvoidMiner
{
    /// <summary>
    /// Items that can be discretely stackes.
    /// </summary>
    public abstract class DiscreteItem : Item
    {
        /// <summary>
        /// Amount of items of this type.
        /// </summary>
        public int StackSize;

        /// <summary>
        /// A textual description of the stack size. Empty string equals "one".
        /// </summary>
        public override string StackDescription { get { return StackSize == 1 ? "" : "x" + StackSize; } }

        /// <summary>
        /// True iff the item represents an empty (or negative) amount of the item.
        /// </summary>
        public override bool IsEmpty { get { return StackSize <= 0; } }

        public DiscreteItem(string name, string description, float weight, bool isUsable, ItemCategory category, int stackSize = 1) :
            base(name, description, weight, isUsable, category)
        {
            StackSize = stackSize;
        }
    }
}

