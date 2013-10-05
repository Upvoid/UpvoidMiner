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
        public int StackSize;

        /// <summary>
        /// A textual description of the stack size. Empty string equals "one".
        /// </summary>
        public override string StackDescription { get { return StackSize == 1 ? "" : "x" + StackSize; } }

        public DiscreteItem(string name, string description, float weight, bool isUsable, ItemCategory category, int stackSize = 1) :
            base(name, description, weight, isUsable, category)
        {
            StackSize = stackSize;
        }
    }
}

