using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Universe;

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
        /// Sets a given item to a given quickaccess slot. Item may be null.
        /// </summary>
        public void SetQuickAccess(Item item, int idx)
        {
            Debug.Assert(0 <= idx && idx <= 9);

            if (quickAccessItems[idx] != null)
                quickAccessItems[idx].QuickAccessIndex = -1;

            quickAccessItems[idx] = item;

            if ( item != null )
                item.QuickAccessIndex = idx;
        }

        /// <summary>
        /// Adds an item. Is automatically assigned to quickAcess if free.
        /// </summary>
        public void AddItem(Item item)
        {
            Debug.Assert(item != null);
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

        /// <summary>
        /// Removes a given item.
        /// </summary>
        public void RemoveItem(Item item)
        {
            Debug.Assert(item != null);

            for (int i = 0; i < Items.Count; ++i)
            {
                if (Items[i] == item)
                {
                    Items.RemoveAt(i);
                    break;
                }
            }

            if (item.QuickAccessIndex >= 0)
                SetQuickAccess(null, item.QuickAccessIndex);
        }

        /// <summary>
        /// Adds a resource of a given terrain material type. Amount can be negative.
        /// </summary>
        public void AddResource(TerrainMaterial mat, float amount)
        {
            foreach (var item in Items)
            {
                if ( item is ResourceItem )
                {
                    ResourceItem ritem = item as ResourceItem;
                    if ( ritem.Material.MaterialIndex == mat.MaterialIndex )
                    {
                        ritem.Volume += amount;
                        if ( ritem.Volume < .001f )
                            RemoveItem(ritem);
                        return;
                    }
                }
            }

            // We have not found this resource so far: create a new one.
            if (amount > 0)
            {
                ResourceItem newItem = new ResourceItem(mat);
                newItem.Volume += amount;
                AddItem(newItem);
            }
        }
    }
}

