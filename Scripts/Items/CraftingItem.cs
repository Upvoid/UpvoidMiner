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
            ShovelBlade
        }

        public enum MaterialType
        {
            Wood,
            Stone,
            Copper,
            Other
        }

        private static string Adjective(MaterialType mat)
        {
            switch (mat)
            {
                case MaterialType.Wood:
                    return "wooden";
                case MaterialType.Stone:
                    return "stone";
                case MaterialType.Copper:
                    return "copper";
                case MaterialType.Other:
                    return "";
            }
            return "";
        }

        private static string ItemName(ItemType type, MaterialType mat)
        {
            var adjective = Adjective(mat);
            adjective = adjective.Length > 0 ? char.ToUpper(adjective[0]) + adjective.Substring(1) : "";

            switch (type)
            {
                case ItemType.Handle:
                    return "Handle";
                case ItemType.ShovelBlade:
                    return adjective + " Shovel Blade";
            }
            return "";
        }

        private static string Description(ItemType type, MaterialType mat)
        {
            switch (type)
            {
                case ItemType.Handle:
                    return "A wooden handle.";
                case ItemType.ShovelBlade:
                    return "A " + Adjective(mat) + " shovel blade.";
            }
                
            return "A nondescript object.";
        }
        private static float Weight(ItemType type, MaterialType mat)
        {
            switch (type)
            {
                case ItemType.Handle:
                    return 0.3f;
                case ItemType.ShovelBlade:
                    return 1.0f;
            }

            return 0.0f;
        }
        private static string IconName(ItemType type, MaterialType mat)
        {
            var suffix = mat != MaterialType.Other ? "," + mat + "Mat" : "";
            switch (type)
            {
                case ItemType.Handle:
                    return "Handle" + suffix;
                case ItemType.ShovelBlade:
                    return "ShovelBlade" + suffix;
            }

            return "";
        }

        public readonly ItemType Type;
        public readonly MaterialType Material;

        public CraftingItem(ItemType type, MaterialType mat = MaterialType.Other, int stacksize = 1) :
            base(ItemName(type, mat), Description(type, mat), Weight(type, mat), ItemCategory.Crafting, stacksize)
        {
            Type = type;
            Material = mat;
        }

        public override string Identifier
        {
            get { return "04-Crafting." + Name; }
        }

        public override string Icon
        {
            get { return IconName(Type,Material); }
        }

        public override bool TryMerge(Item rhs, bool substract, bool force, bool dryrun = false)
        {
            var item = rhs as CraftingItem;
            if (item == null) return false;
            if (item.Type != Type) return false;
            if (item.Material != Material) return false;

            return Merge(item, substract, force, dryrun);
        }

        public override Item Clone()
        {
            return new CraftingItem(Type,Material,StackSize);
        }
    }
}
