using System;
using System.Diagnostics;
using System.Collections.Generic;

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
        Hammer,

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

        public override string Identifier
        {
            get
            {
                return "00-Tools." + ((int)ToolType).ToString("00") + "-" + Name;
            }
        }

        public ToolItem(ToolType type, int stackSize = 1) :
            base("", "", 1.0f, ItemCategory.Tools, stackSize)
        {
            ToolType = type;
            Icon = ToolType.ToString();
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
                case ToolType.Hammer:
                    Name = "Hammer";
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

        public override void OnUse(Player player, Engine.vec3 _worldPos)
        {
            switch (ToolType)
            {
                case ToolType.Pickaxe:
                    // Pickaxe has small radius but can dig everywhere
					player.DigSphere(_worldPos, 0.8f, null);
                    return;

                case ToolType.Shovel:
                    // Shovel has big radius but can only dig dirt
                    player.DigSphere(_worldPos, 1.4f, new [] { TerrainResource.FromName("Dirt").Index });
                    return;

                case ToolType.Axe:
                    // TODO
                    return;

                case ToolType.DroneChain:
                    // Add a drone to the use-position.
                    player.AddDrone(_worldPos);
                    // Remove that dron from inventory.
                    player.Inventory.RemoveItem(new ToolItem(ToolType));
                    return;

                case ToolType.Hammer:
                    // TODO
                    return;

                default: 
                    throw new InvalidOperationException("Unknown tool");
            }
        }
    }
}

