using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EfficientUI;
using Engine.Resources;

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

            [UICollection("ItemIcon")]
            public List<IconUI> IconStack { get; private set; }

            public ItemUI(Item item)
            {
                this.item = item;
                IconStack = new List<IconUI>();
                foreach (var icon in item.Icon.Split(','))
                    IconStack.Add(new IconUI(Resources.UseTextureData("Items/Icons/" + icon, UpvoidMiner.ModDomain)));
            }
        }

        public class ResourceItemUI : ItemUI
        {
            private readonly ResourceItem item;

            [UIString]
            public string Volume { get { return item.Volume.ToString("0.0") + "m&sup3;"; } }

            public ResourceItemUI(ResourceItem item)
                : base(item)
            {
                this.item = item;
            }
        }

        public class ToolItemUI : ItemUI
        {
            private readonly Item item;

            public ToolItemUI(Item item)
                : base(item)
            {
                this.item = item;
            }
        }
        public class MaterialItemUI : ItemUI
        {
            private readonly MaterialItem item;

            [UIString]
            public string StackSize { get { return item.StackSize + "x"; } }

            public MaterialItemUI(MaterialItem item)
                : base(item)
            {
                this.item = item;
            }
        }

        [UICollection("ToolItem")]
        public List<ToolItemUI> ToolItems { get; private set; }
        [UICollection("MaterialItem")]
        public List<MaterialItemUI> MaterialItems { get; private set; }
        [UICollection("ResourceItem")]
        public List<ResourceItemUI> ResourceItems { get; private set; }

        public InventoryUI()
            : base("Inventory")
        {
            ToolItems = new List<ToolItemUI>();
            MaterialItems = new List<MaterialItemUI>();
            ResourceItems = new List<ResourceItemUI>();

            UIProxyManager.AddProxy(this);

            LocalScript.player.Inventory.Items.OnAdd += ItemsOnOnAdd;
            LocalScript.player.Inventory.Items.OnRemove += ItemsOnOnRemove;
        }

        private void ItemsOnOnRemove(Item item)
        {
            ToolItems.RemoveAll(i => i.Item == item);
            MaterialItems.RemoveAll(i => i.Item == item);
            ResourceItems.RemoveAll(i => i.Item == item);
        }

        private void ItemsOnOnAdd(Item item)
        {
            if (item is MaterialItem)
                MaterialItems.Add(new MaterialItemUI(item as MaterialItem));
            else if (item is ResourceItem)
                ResourceItems.Add(new ResourceItemUI(item as ResourceItem));
            else ToolItems.Add(new ToolItemUI(item));
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
