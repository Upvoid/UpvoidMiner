using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using EfficientUI;
using Engine.Resources;
using Engine.Universe;
using UpvoidMiner.Items;
using UpvoidMiner.UI;

namespace UpvoidMiner
{
    public class InventoryUI : UIProxy
    {
        public class IconUI : UIProxy
        {
            [UIImage]
            public TextureDataResource Icon { get; set; }

            public IconUI(TextureDataResource icon)
            {
                Icon = icon;
            }
        }

        public class ItemUI : UIProxy
        {
            private readonly Item item;

            public Item Item { get { return item; } }

            [UIString]
            public string Name { get { return item.Name; } }

            [UIString]
            public string Description { get { return item.Description; } }

            [UIString]
            public string Hotkey { get { return item.QuickAccessIndex < 0 ? "" : ((item.QuickAccessIndex + 1) % 10).ToString(CultureInfo.InvariantCulture); } }

            [UICollection("ItemIcon")]
            public List<IconUI> IconStack { get; private set; }

            [UICollection("ItemInfo")]
            public ItemUI ItemInfo { get { return this; } }

            [UIObject]
            public bool IsDroppable { get { return item.IsDroppable; } }

            [UIObject]
            public bool IsDestructible { get { return item is MaterialItem; } }

            [UIObject]
            public bool IsConvertible { get { return item is MaterialItem; } }

            [UIObject]
            public bool IsSelected { get { return LocalScript.player != null && LocalScript.player.Inventory.Selection == item; } }

            [UICallback]
            public void SetHotkey(int idx)
            {
                if (LocalScript.player == null)
                    return;

                LocalScript.player.Inventory.SetQuickAccess(item, idx);
            }

            [UICallback]
            public void BtnConvertOne()
            {
                if (LocalScript.player == null)
                    return;

                LocalScript.player.Convert(item, false);
            }

            [UICallback]
            public void BtnConvertAll()
            {
                if (LocalScript.player == null)
                    return;

                LocalScript.player.Convert(item, true);
            }

            [UICallback]
            public void BtnDrop()
            {
                if (LocalScript.player == null)
                    return;

                LocalScript.player.DropItem(item);
            }

            [UICallback]
            public void BtnDestroy()
            {
                if (LocalScript.player == null)
                    return;

                LocalScript.player.Inventory.RemoveItem(item);
            }

            [UICallback]
            public void SelectMe()
            {
                if (LocalScript.player == null)
                    return;

                LocalScript.player.Inventory.SelectItem(item);
            }

            public ItemUI(Item item)
            {
                this.item = item;
                IconStack = new List<IconUI>();
                foreach (var icon in item.Icon.Split(','))
                    IconStack.Add(new IconUI(Resources.UseTextureData("Items/Icons/" + icon, UpvoidMiner.ModDomain)));
            }
        }

        public class RecipeItemUI : ItemUI
        {
            private readonly Item item;

            [UICollection("RecipeItem")]
            public ItemUI ThisItem { get { return this; } }

            public RecipeItemUI(Item item)
                : base(item)
            {
                this.item = item;
            }
        }

        public class CraftingItemUI : ItemUI
        {
            private readonly Item item;

            [UICollection("CraftingItem")]
            public ItemUI ThisItem { get { return this; } }

            [UIString]
            public string StackSize
            {
                get
                {
                    if (LocalScript.player == null)
                        return null;

                    if (item is DiscreteItem && !LocalScript.player.GodMode)
                        return ((item as DiscreteItem).StackSize + "x");

                    return "";
                }
            }

            public CraftingItemUI(Item item)
                : base(item)
            {
                this.item = item;
            }
        }

        public class ResourceItemUI : ItemUI
        {
            private readonly ResourceItem item;

            [UICollection("ResourceItem")]
            public ItemUI ThisItem { get { return this; } }

            [UIString]
            public string Volume { get { return LocalScript.player == null || LocalScript.player.GodMode ? "" 
                    : item.Volume >= 1000 ? "&gt;999m&sup3;" : item.Volume.ToString("0.0") + "m&sup3;"; } }

            public ResourceItemUI(ResourceItem item)
                : base(item)
            {
                this.item = item;
            }
        }

        public class ToolItemUI : ItemUI
        {
            private readonly Item item;
            
            [UICollection("ToolItem")]
            public ItemUI ThisItem { get { return this; } }

            [UIString]
            public string StackSize { get { return LocalScript.player == null || LocalScript.player.GodMode || !(item is DiscreteItem) || (item as DiscreteItem).StackSize == 1 ? "" 
                    : (item as DiscreteItem).StackSize + "x"; } }

            public ToolItemUI(Item item)
                : base(item)
            {
                this.item = item;
            }
        }
        public class MaterialItemUI : ItemUI
        {
            private readonly MaterialItem item;
            
            [UICollection("MaterialItem")]
            public ItemUI ThisItem { get { return this; } }

            [UIString]
            public string StackSize { get { return LocalScript.player == null || LocalScript.player.GodMode ? "" 
                    : item.StackSize + "x"; } }

            public MaterialItemUI(MaterialItem item)
                : base(item)
            {
                this.item = item;
            }
        }

        [UICollection("RecipeItem")]
        public List<RecipeItemUI> RecipeItems { get; private set; }
        [UICollection("CraftingItem")]
        public List<CraftingItemUI> CraftingItems { get; private set; }
        [UICollection("ToolItem")]
        public List<ToolItemUI> ToolItems { get; private set; }
        [UICollection("MaterialItem")]
        public List<MaterialItemUI> MaterialItems { get; private set; }
        [UICollection("ResourceItem")]
        public List<ResourceItemUI> ResourceItems { get; private set; }

        [UIObject]
        public bool DeleteMenuRed { get { return Universe.Below10FPS; } }

        [UIObject]
        public bool CanDeletePhysics { get { return ItemManager.AllItemsEntities.Any(); } }

        [UICallback]
        public void DeletePhysics(bool del, bool dyn)
        {
            foreach (var kvp in ItemManager.AllItemsEntities.ToArray())
            {
                if (kvp.Value.FixedPosition == dyn)
                    continue;

                ItemManager.RemoveItemFromWorld(kvp.Value);

                if (!del && LocalScript.player != null)
                    LocalScript.player.Inventory.AddItem(kvp.Key);
            }

            // Tutorial
            if (dyn)
                Tutorials.MsgAdvancedCraftingCollectAllDynamic.Report(1);
            if (!dyn)
                Tutorials.MsgAdvancedCraftingCollectAllStatic.Report(1);
        }

        public class QuickAccessUI : UIProxy
        {
            [UICollection("QuickItem")]
            public ItemUI[] Bar { get; private set; }

            public QuickAccessUI() : base("QuickAccess")
            {
                Bar = new ItemUI[LocalScript.player.Inventory.QuickAccessItems.Length];
                UIProxyManager.AddProxy(this);
                LocalScript.player.Inventory.OnQuickAccessChanged += UpdateItem;
                for (int i = 0; i < Bar.Length; ++i)
                    UpdateItem(i, LocalScript.player.Inventory.QuickAccessItems[i]);
            }

            public void UpdateItem(int idx, Item item)
            {
                if (item is MaterialItem)
                    Bar[idx] = new MaterialItemUI(item as MaterialItem);
                else if (item is ResourceItem)
                    Bar[idx] = new ResourceItemUI(item as ResourceItem);
                else if (item is RecipeItem)
                    Bar[idx] = new RecipeItemUI(item as RecipeItem);
                else if (item is ToolItem || item is PipetteItem)
                    Bar[idx] = new ToolItemUI(item);
                else if (item != null)
                    Bar[idx] = new CraftingItemUI(item);
                else 
                    Bar[idx] = null;
            }
        }

        private QuickAccessUI quickAccess = new QuickAccessUI();

        public InventoryUI()
            : base("Inventory")
        {
            ToolItems = new List<ToolItemUI>();
            MaterialItems = new List<MaterialItemUI>();
            ResourceItems = new List<ResourceItemUI>();
            RecipeItems = new List<RecipeItemUI>();
            CraftingItems = new List<CraftingItemUI>();

            UIProxyManager.AddProxy(this);

            LocalScript.player.Inventory.Items.OnAdd += ItemsOnOnAdd;
            LocalScript.player.Inventory.Items.OnRemove += ItemsOnOnRemove;
        }

        private void ItemsOnOnRemove(Item item)
        {
            ToolItems.RemoveAll(i => i.Item == item);
            MaterialItems.RemoveAll(i => i.Item == item);
            ResourceItems.RemoveAll(i => i.Item == item);
            RecipeItems.RemoveAll(i => i.Item == item);
            CraftingItems.RemoveAll(i => i.Item == item);
        }

        private void ItemsOnOnAdd(Item item)
        {
            if (item is MaterialItem)
                MaterialItems.Add(new MaterialItemUI(item as MaterialItem));
            else if (item is ResourceItem)
                ResourceItems.Add(new ResourceItemUI(item as ResourceItem));
            else if (item is RecipeItem)
                RecipeItems.Add(new RecipeItemUI(item as RecipeItem));
            else if (item is ToolItem || item is PipetteItem)
                ToolItems.Add(new ToolItemUI(item));
            else 
                CraftingItems.Add(new CraftingItemUI(item));
        }

        [UIObject]
        public bool IsInventoryOpen
        {
            get { return LocalScript.player != null && LocalScript.player.Gui != null && LocalScript.player.Gui.IsInventoryOpen; }
        }

        [UIObject]
        public bool IsUIOpen
        {
            get { return LocalScript.player != null && LocalScript.player.Gui != null && LocalScript.player.Gui.IsUIOpen; }
        }
    }
}
