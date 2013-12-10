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

        public DiscreteItem(string name, string description, float weight, ItemCategory category, int stackSize = 1) :
            base(name, description, weight, category)
        {
            StackSize = stackSize;
        }

        /// <summary>
        /// Helper for implementing TryMerge for discrete items
        /// </summary>
        protected bool Merge(DiscreteItem item, bool subtract, bool force, bool dryrun)
        {
            if ( subtract )
            {
                if ( !force && StackSize < item.StackSize )
                    return false;

                if ( !dryrun )
                    StackSize -= item.StackSize;
            }
            else 
            {
                if ( !dryrun )
                    StackSize += item.StackSize;
            }

            return true;
        }
    }
}

