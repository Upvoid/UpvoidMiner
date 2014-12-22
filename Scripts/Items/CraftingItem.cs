using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpvoidMiner
{
    public class CraftingItem : DiscreteItem
    {
        public enum ItemType
        {
            Handle,
            ShovelBlade,
            PickaxeHead,
            AxeHead,
        }

        private static string ItemName(ItemType type, Substance substance)
        {
            
            switch (type)
            {
                case ItemType.Handle:
                    return "Handle";
                case ItemType.ShovelBlade:
                    return substance.Name + " Shovel Blade";
                case ItemType.PickaxeHead:
                    return substance.Name + " Pickaxe Head";
                case ItemType.AxeHead:
                    return substance.Name + " Axe Head";
            }
            return "";
        }

        private static string MakeDescription(ItemType type, Substance substance)
        {
            switch (type)
            {
                case ItemType.Handle:
                    return "A wooden handle.";
                case ItemType.ShovelBlade:
                    return "A shovel blade made from " + substance.Name + ".";
                case ItemType.PickaxeHead:
                    return "A pickaxe head made from " + substance.Name + ".";
                case ItemType.AxeHead:
                    return "An axe head made from " + substance.Name + ".";
            }

            return "A nondescript object.";
        }
        private static float MakeWeight(ItemType type, Substance substance)
        {
            switch (type)
            {
                case ItemType.Handle:
                    return 0.3f;
                case ItemType.ShovelBlade:
                    return 1.0f;
                case ItemType.PickaxeHead:
                    return 1.0f;
                case ItemType.AxeHead:
                    return 1.0f;
            }

            return 0.0f;
        }
        private static string IconName(ItemType type, Substance substance)
        {
            var suffix = "";

            if (substance != null)
            {
                var matName = substance.MatOverlayName;
                if (matName != null)
                    suffix += "," + matName;
            }
            switch (type)
            {
                case ItemType.Handle:
                    return "Handle" + suffix;
                case ItemType.ShovelBlade:
                    return "ShovelBlade" + suffix;
                case ItemType.PickaxeHead:
                    return "PickaxeHead" + suffix;
                case ItemType.AxeHead:
                    return "AxeHead" + suffix;
            }

            return "";
        }

        public readonly ItemType Type;
        public readonly Substance Substance;

        public CraftingItem(ItemType type, Substance sub, int stacksize = 1) :
            base(ItemName(type, sub), MakeDescription(type, sub), MakeWeight(type, sub), ItemCategory.Crafting, stacksize)
        {
            Type = type;
            Substance = sub;
        }


        public override string Icon
        {
            get { return IconName(Type, Substance); }
        }

        public override bool TryMerge(Item rhs, bool substract, bool force, bool dryrun = false)
        {
            var item = rhs as CraftingItem;
            if (item == null) return false;
            if (item.Type != Type) return false;
            if (!substract && !Substance.GetType().IsInstanceOfType(item.Substance)) return false;
            if (substract && !item.Substance.GetType().IsInstanceOfType(Substance)) return false;

            return Merge(item, substract, force, dryrun);
        }

        public override Item Clone()
        {
            return new CraftingItem(Type, Substance, StackSize);
        }

        public override Item Clone(Substance sub)
        {
            return new CraftingItem(Type, sub, StackSize);
        }
    }
}
