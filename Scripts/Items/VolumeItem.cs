using System;

namespace UpvoidMiner
{
    /// <summary>
    /// Item with a volume as stack size.
    /// </summary>
    public abstract class VolumeItem : Item
    {
        /// <summary>
        /// Volume in m^3 that this item represents.
        /// </summary>
        public float Volume;

        /// <summary>
        /// A textual description of the stack size. Empty string equals "one".
        /// </summary>
        public override string StackDescription { get { return Volume.ToString("0.0") + "m³"; } }

        /// <summary>
        /// True iff the item represents an empty (or negative) amount of the item.
        /// </summary>
        public override bool IsEmpty { get { return Volume <= .0001f; } }

        public VolumeItem(string name, string description, float weight, bool isUsable, ItemCategory category, float volume = 0f) :
            base(name, description, weight, isUsable, category)
        {
            Volume = volume;
        }
    }
}

