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
using System.Diagnostics;
using System.Collections.Generic;

namespace UpvoidMiner
{
    /// <summary>
    /// Abstract interface for a crafting rule.
    /// For now, crafting must be unique per item. (i.e. no item should be craftable in more than one way).
    /// </summary>
    public abstract class CraftingRule
    {
        /// <summary>
        /// If true, this rule is already discovered
        /// </summary>
        public bool Discovered { get; protected set; }

        /// <summary>
        /// The resulting item for this rule (in its current configuration).
        /// </summary>
        public virtual Item Result { get; protected set; }

        /// <summary>
        /// Gets a set of ingredients required for this rule (in its current configuration).
        /// May not be null or empty!
        /// </summary>
        public virtual IEnumerable<Item> Ingredients { get; protected set; }

        /// <summary>
        /// Gets a set of items resulting from dismantling the result item (in its current configuration).
        /// Null or empty means no dismantling possible.
        /// </summary>
        public virtual IEnumerable<Item> DismantleResult { get; protected set; }

        /// <summary>
        /// Returns true if the item can be dismantled (i.e. has a properly defined dismantle result).
        /// </summary>
        public bool CanBeDismantled
        {
            get 
            {
                var res = DismantleResult;
                return res != null && res.GetEnumerator().MoveNext();
            }
        }

        public CraftingRule()
        {
            Discovered = false;
        }

        /// <summary>
        /// Set the discovered flag to true, i.e. this rule is now discovered.
        /// </summary>
        public void Discover()
        {
            Discovered = true;
        }

        /// <summary>
        /// Checks if this rule is applicable on the given items
        /// The refItem is used as a reference for special rules (e.g. material ones) and can be null
        /// </summary>
        public virtual bool IsCraftable(Item refItem, ItemCollection _items)
        {
            // Copy items.
            ItemCollection items = new ItemCollection(_items);

            // Try to remove each ingredient.
            foreach (var item in Ingredients)
                if ( !items.RemoveItem(item, false) )
                    return false;

            // Only if all ingredients can be removed, the rule is applicable.
            return true;
        }

        /// <summary>
        /// Applies this rule on the given item collection.
        /// A clone of the result is added to the items.
        /// Craftability has to be checked before calling this function!
        /// The refItem is used as a reference for special rules (e.g. material ones) and can be null
        /// </summary>
        public virtual void Craft(Item refItem, ItemCollection items)
        {
            Debug.Assert(IsCraftable(refItem, items), "Not craftable!");
            
            // Remove each ingredient.
            foreach (var item in Ingredients)
                if ( !items.RemoveItem(item, false) )
                    Debug.Fail("Item should be in collection!");

            // Add the resulting item.
            items.AddItem(Result.Clone());
        }

        /// <summary>
        /// Determines whether the given item can be crafted by this rules given unlimited resources.
        /// </summary>
        public virtual bool CouldBeCraftable(Item item)
        {
            if ( !item.TryMerge(Result, false, false, true) )
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether the given item could be dismantled by this rule.
        /// </summary>
        public virtual bool CouldBeDismantled(Item item)
        {
            if ( !item.TryMerge(Result, true, false, true) )
                return false;

            return true;
        }
    }
}

