using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace UpvoidMiner
{
    /// <summary>
    /// Class for managing an Inventory.
    /// </summary>
    public class Inventory
    {
        /// <summary>
        /// Backref to player.
        /// </summary>
        private Player player;

        /// <summary>
        /// A list of all items.
        /// </summary>
        public readonly List<Item> Items = new List<Item>();

        /// <summary>
        /// Quick access items (indices 1-9 indicate user-definable quick access 1-9, 0 is special for selected item)
        /// </summary>
        private Item[] quickAccessItems = new Item[10];

        /// <summary>
        /// Index of the selected item (index points into quickAccessItems array)
        /// </summary>
        private int selectedItem = 0;

        public Inventory(Player player)
        {
            this.player = player;
        }

        /// <summary>
        /// Gets the selected item, may be null.
        /// </summary>
        public Item Selection { get { return quickAccessItems[selectedItem]; } }

        /// <summary>
        /// Sets the currently selected item
        /// </summary>
        public void Select(int idx)
        {
            Debug.Assert(0 <= idx && idx <= 9);
            selectedItem = idx;
        }

        /// <summary>
        /// Sets a given item to a given quickaccess slot.
        /// </summary>
        public void SetQuickAccess(Item item, int idx)
        {
            Debug.Assert(0 <= idx && idx <= 9);

            if (quickAccessItems[idx] != null)
                quickAccessItems[idx].QuickAccessIndex = -1;
            quickAccessItems[idx] = item;
            item.QuickAccessIndex = idx;
        }

        /// <summary>
        /// Adds an item. Is automatically assigned to quickAcess if free.
        /// </summary>
        public void AddItem(Item item)
        {
            Items.Add(item);

            for (int i = 0; i < quickAccessItems.Length; ++i)
            {
                if (quickAccessItems[i] == null)
                {
                    SetQuickAccess(item, i);
                    break;
                }
            }
        }
    }
}

