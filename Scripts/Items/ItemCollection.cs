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
using System.Collections.Generic;

namespace UpvoidMiner
{
    /// <summary>
    /// An item collection is an ordered list of items.
    /// Can be used for invenotry, crafting, loot, etc.
    /// Items are automatically stacked.
    /// </summary>
    public class ItemCollection : IEnumerable<Item>
    {
        /// <summary>
        /// The items.
        /// </summary>
        public readonly List<Item> Items = new List<Item>();

        /// <summary>
        /// Occurs when an item is added to the list.
        /// </summary>
        public event Action<Item> OnAdd;
        /// <summary>
        /// Occurs when an item is removed from the list.
        /// </summary>
        public event Action<Item> OnRemove;
        /// <summary>
        /// Occurs when the quantity of an item changed.
        /// </summary>
        public event Action<Item> OnQuantityChange;

        /// <summary>
        /// Returns the item with the given id or null if no such item exists in this collection.
        /// </summary>
        public Item ItemById(long id)
        {
            foreach (Item item in Items)
            {
                if (item.Id == id)
                    return item;
            }

            return null;
        }

        /// <summary>
        /// Creates a new item collection.
        /// </summary>
        /// <param name="cloneFrom">if non-null, creates a deep copy of the item.</param>
        public ItemCollection(ItemCollection cloneFrom = null)
        {
            if ( cloneFrom != null )
                foreach (var item in cloneFrom)
                    Items.Add(item.Clone());
        }

        /// <summary>
        /// Adds an item to the list or merge it with an existing one if possible.
        /// Returns true if a new item was appended.
        /// </summary>
        public bool AddItem(Item item)
        {
            // Try to merge with already possessed item.
            foreach (var it in Items)
                if (it.TryMerge(item, false, false))
                {
                    if (OnQuantityChange != null)
                        OnQuantityChange(it);
                    return false;
                }

            // If unsuccessful: add item
            Items.Add(item);

            // Trigger event.
            if ( OnAdd != null )
                OnAdd(item);

            return true;
        }

        /// <summary>
        /// Removes the amount of items represented by the argument item from the collection.
        /// Does not necessarily decrease the number of items in the list.
        /// Returns false if the item could not be fully removed (i.e. the collection had less of this item than the item indicated).
        /// If force is true, items will be removed even if the result is negative (e.g. useful for removing resources).
        /// </summary>
        public bool RemoveItem(Item item, bool force)
        {
            // Try to substract with already possessed items.
            foreach (var it in Items)
            {
                if ( it.TryMerge(item, true, force) )
                {
                    if (it.IsEmpty)
                    {
                        Items.Remove(it);
                        if (OnRemove != null)
                            OnRemove(it);
                    }
                    else
                    {
                        if (OnQuantityChange != null)
                            OnQuantityChange(it);
                    }
                    return true;
                }
            }

            // Unable to remove.
            return false;
        }

        /// <summary>
        /// Returns true iff the given item is contained in this collection.
        /// </summary>
        public bool ContainsItem(Item item)
        {
            foreach (var it in Items) {
                if (it.TryMerge(item, true, false, true))
                    return true;
            }
            return false;
        }

        #region IEnumerable implementation

        public IEnumerator<Item> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        #endregion

        #region IEnumerable implementation

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}

