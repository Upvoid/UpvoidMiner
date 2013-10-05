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
        public float Volume;

        /// <summary>
        /// A textual description of the stack size. Empty string equals "one".
        /// </summary>
        public override string StackDescription { get { return Volume.ToString("0.0") + "m³"; } }

        public VolumeItem(string name, string description, float weight, bool isUsable, ItemCategory category, float volume = 0f) :
            base(name, description, weight, isUsable, category)
        {
            Volume = volume;
        }
    }
}

