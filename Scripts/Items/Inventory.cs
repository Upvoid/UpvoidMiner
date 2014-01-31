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
using System.Diagnostics;
using Engine.Universe;

namespace UpvoidMiner
{
    /// <summary>
    /// Class for managing an Inventory for a player.
    /// </summary>
    public partial class Inventory
    {
        public event Action<int, Item> OnQuickAccessChanged;
        public event Action<int, Item> OnSelectionChanged;

        /// <summary>
        /// Backref to player.
        /// </summary>
        //private Player player;

        /// <summary>
        /// A list of all items.
        /// </summary>
        public readonly ItemCollection Items = new ItemCollection();

        /// <summary>
        /// Quick access items (indices 1-9 indicate user-definable quick access 1-9, 0 is special for selected item)
        /// </summary>
        private Item[] quickAccessItems = new Item[10];

        /// <summary>
        /// List of all crafting rules, discovered or undiscovered.
        /// </summary>
        private List<CraftingRule> craftingRules = new List<CraftingRule>();

        /// <summary>
        /// Index of the selected item (index points into quickAccessItems array)
        /// </summary>
        private int selectedItem = 0;

        public Inventory(Player player)
        {
            //this.player = player;

            Items.OnAdd += setDefaultQuickAccess;
            Items.OnRemove += removeFromQuickAccess;
        }

        /// <summary>
        /// Gets the selected item, may be null.
        /// </summary>
        public Item Selection { get { return quickAccessItems[selectedItem]; } }
        /// <summary>
        /// Gets the selected quick access index. Between 0 and 9 (inclusive).
        /// </summary>
        public int SelectionIndex { get { return selectedItem; } }
        /// <summary>
        /// Gets the number of quickaccess slots.
        /// </summary>
        public int QuickaccessSlots { get { return quickAccessItems.Length; } }

        public Item[] QuickAccessItems { get { return quickAccessItems; } }

        /// <summary>
        /// Sets the currently selected item
        /// CAUTION: '1'-'9' is mapped to 0-8, '0' to 9
        /// </summary>
        public void Select(int idx)
        {
            Debug.Assert(0 <= idx && idx <= 9);

            if ( idx == selectedItem ) return;

            if ( quickAccessItems[selectedItem] != null )
                quickAccessItems[selectedItem].OnDeselect();
            selectedItem = idx;
            if ( quickAccessItems[selectedItem] != null )
                quickAccessItems[selectedItem].OnSelect();

            if (OnSelectionChanged != null)
            {
                OnSelectionChanged(idx, Selection);
            }
        }

        private void setDefaultQuickAccess(Item item)
        {
            // If appended and enough space, also add it to quickAccess.
            // Caution: highest quick access idx is only temporary.
            for (int i = 0; i < quickAccessItems.Length - 1; ++i)
            {
                if (quickAccessItems[i] == null)
                {
                    SetQuickAccess(item, i);
                    break;
                }
            }
        }

        private void removeFromQuickAccess(Item item)
        {
            if (item.QuickAccessIndex >= 0)
                SetQuickAccess(null, item.QuickAccessIndex);
        }

        /// <summary>
        /// Removes all quick-access items
        /// </summary>
        public void ClearQuickAccess()
        {
            for (int i = 0; i < QuickaccessSlots; ++i)
                SetQuickAccess(null, i);
        }

        /// <summary>
        /// Sets a given item to a given quickaccess slot. Item may be null.
        /// </summary>
        public void SetQuickAccess(Item item, int idx)
        {
            Debug.Assert(0 <= idx && idx <= 9);

            if (quickAccessItems[idx] != null)
            {
                if ( idx == selectedItem )
                    quickAccessItems[idx].OnDeselect();
                quickAccessItems[idx].QuickAccessIndex = -1;
            }

            quickAccessItems[idx] = item;

            if ( item != null )
            {
                item.QuickAccessIndex = idx;                
                if ( idx == selectedItem )
                    item.OnSelect();
            }

            if (OnQuickAccessChanged != null)
                OnQuickAccessChanged(idx, item);

        }

        /// <summary>
        /// Adds an item. Is automatically assigned to quickAcess if free.
        /// </summary>
        public void AddItem(Item item)
        {
            Debug.Assert(item != null);

            Items.AddItem(item);

            // Check if new rules were discovered
            return; // DEBUG: Disable crafting
            foreach (var rule in craftingRules) 
            {
                if ( rule.Discovered ) continue;
                else if ( rule.CouldBeDismantled(item) ) rule.Discover();
                else if ( rule.IsCraftable(item, Items) ) rule.Discover();
            }
        }

        /// <summary>
        /// Removes a given item.
        /// </summary>
        public void RemoveItem(Item item)
        {
            Debug.Assert(item != null);

            // Inventory remove is always with force.
            Items.RemoveItem(item, true);
        }

        /// <summary>
        /// Adds a resource of a given terrain material type. Amount can be negative.
        /// </summary>
        public void AddResource(TerrainResource mat, float amount)
        {
            Debug.Assert(mat != null);

            if ( amount > 0 )
                AddItem(new ResourceItem(mat, amount));
            else
                RemoveItem(new ResourceItem(mat, -amount));
        }

        /// <summary>
        /// Gets a list of all discovered rules.
        /// Can implicitly discover applicable rules.
        /// </summary>
        public List<CraftingRule> DiscoveredRules
        {
            get
            {
                List<CraftingRule> rules = new List<CraftingRule>();
                foreach (var rule in craftingRules)
                    if ( rule.Discovered ) 
                        rules.Add(rule);
                return rules;
            }
        }
    }
}

