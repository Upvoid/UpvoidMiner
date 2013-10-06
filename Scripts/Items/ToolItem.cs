using System;
using System.Diagnostics;

namespace UpvoidMiner
{
    /// <summary>
    /// Type of tools
    /// </summary>
    public enum ToolType
    {
        Pickaxe,
        Shovel,
        Axe,
        SteamHammer,

        DroneChain
    }

    /// <summary>
    /// An item that is a tool
    /// </summary>
    public class ToolItem : DiscreteItem
    {
        /// <summary>
        /// Type of tool.
        /// </summary>
        public readonly ToolType ToolType;

        public ToolItem(ToolType type, int stackSize = 1) :
            base("", "", 1.0f, true, ItemCategory.Tools, stackSize)
        {
            ToolType = type;
            switch (ToolType)
            {
                case ToolType.Pickaxe:
                    Name = "Pickaxe";
                    Description = "Tool used for mining stone.";
                    break;
                case ToolType.Shovel:
                    Name = "Shovel";
                    Description = "Tool used for excavating earth.";
                    break;
                case ToolType.Axe:
                    Name = "Axe";
                    Description = "Tool used for chopping trees.";
                    break;
                case ToolType.SteamHammer:
                    Name = "Steam Hammer";
                    Description = "Tool used for crafting mechanics.";
                    break;

                case ToolType.DroneChain:
                    Name = "Chain Drone";
                    Description = "Drone used for creating chains of vertical digging constraints.";
                    break;

                default: Debug.Fail("Unknown tool type"); break;
            }
        }

        /// <summary>
        /// This can be merged with material items of the same resource and shape and size.
        /// </summary>
        public override bool TryMerge(Item rhs, bool subtract, bool force, bool dryrun = false)
        {
            ToolItem item = rhs as ToolItem;
            if ( item == null ) return false;
            if ( item.ToolType != ToolType ) return false;
            
            return Merge(item, subtract, force, dryrun);
        }

        /// <summary>
        /// Creates a copy of this item.
        /// </summary>
        public override Item Clone()
        {
            return new ToolItem(ToolType, StackSize);
        }
    }
}

