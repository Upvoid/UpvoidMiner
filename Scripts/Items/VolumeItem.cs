using System;

namespace UpvoidMiner
{
    /// <summary>
    /// Item with a volume as stack size.
    /// </summary>
    public class VolumeItem : Item
    {
        /// <summary>
        /// Volume in m^3 that this item represents.
        /// </summary>
        public float Volume = 0;

        public VolumeItem(string name, string description, float weight, bool isUsable, ItemCategory category) :
            base(name, description, weight, isUsable, category)
        {
        }
    }
}

