// Copyright (C) by Upvoid Studios
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>

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

        public VolumeItem(string name, string description, float weight, ItemCategory category, float volume = 0f) :
            base(name, description, weight, category)
        {
            Volume = volume;
        }

        /// <summary>
        /// Helper for implementing TryMerge for discrete items
        /// </summary>
        protected bool Merge(VolumeItem item, bool subtract, bool force, bool dryrun)
        {
            if ( subtract )
            {
                if ( !force && Volume + .0001f < item.Volume )
                    return false;

                if ( !dryrun )
                    Volume -= item.Volume;
            }
            else
            {
                if ( !dryrun )
                    Volume += item.Volume;
            }

            return true;
        }
    }
}

