using System;
using System.Collections.Generic;
using Engine;
using Engine.Universe;

namespace UpvoidMiner
{
    /// <summary>
    /// Crafting rule for crafting material items in a given shape with different sizes
    /// </summary>
    public class MaterialCraftingRule : CraftingRule
    {
        /// <summary>
        /// Ratio of resources lost when dismantling.
        /// </summary>
        public const float DismantleRatio = .5f;

        /// <summary>
        /// The material.
        /// </summary>
        public readonly TerrainMaterial Material;

        /// <summary>
        /// The shape of the material.
        /// </summary>
        public readonly MaterialShape Shape;

        /// <summary>
        /// Currently configured sizes
        /// </summary>
        public vec3 Size;

        public MaterialCraftingRule(TerrainMaterial material, MaterialShape shape, vec3 size)
        {
            Material = material;
            Shape = shape;
            Size = size;
        }

        /// <summary>
        /// Currently configure resulting item
        /// </summary>
        public override Item Result
        {
            get
            {
                return new MaterialItem(Material, Shape, Size);
            }
        }
        /// <summary>
        /// Ingredients are resources of the correct type and exact volume
        /// </summary>
        public override IEnumerable<Item> Ingredients
        {
            get
            {
                return new [] { new ResourceItem(Material, (Result as MaterialItem).Volume) };
            }
        }
        /// <summary>
        /// Ingredients are resources of the correct type and exact volume
        /// </summary>
        public override IEnumerable<Item> DismantleResult
        {
            get
            {
                return new [] { new ResourceItem(Material, (Result as MaterialItem).Volume * DismantleRatio) };
            }
        }

        /// <summary>
        /// Promotes the size of this rule for a given reference item.
        /// </summary>
        private void PromoteFor(Item refItem)
        {
            MaterialItem item = refItem as MaterialItem;
            if ( item != null &&
                item.Material.MaterialIndex == Material.MaterialIndex &&
                item.Shape == Shape )
                Size = item.Size;
        }
        
        /// <summary>
        /// Special rule: if only different sizes, temporarily promote rule.
        /// </summary>
        public override bool IsCraftable(Item refItem, ItemCollection items)
        {
            vec3 saveSize = Size;
            PromoteFor(refItem);
            bool result = base.IsCraftable(refItem, items);
            Size = saveSize;
            return result;
        }
        /// <summary>
        /// Special rule: if only different sizes, temporarily promote rule.
        /// </summary>
        public override void Craft(Item refItem, ItemCollection items)
        {
            vec3 saveSize = Size;
            PromoteFor(refItem);
            base.Craft(refItem, items);
            Size = saveSize;
        }

        /// <summary>
        /// Special rule: if only different sizes, it could be craftable
        /// </summary>
        public override bool CouldBeCraftable(Item _item)
        {
            MaterialItem item = _item as MaterialItem;
            if ( item != null &&
                item.Material.MaterialIndex == Material.MaterialIndex &&
                item.Shape == Shape )
                return true;
            else return false;
        }
        /// <summary>
        /// Special rule: if only different sizes, it could be dismantled
        /// </summary>
        public override bool CouldBeDismantled(Item _item)
        {
            MaterialItem item = _item as MaterialItem;
            if ( item != null &&
                item.Material.MaterialIndex == Material.MaterialIndex &&
                item.Shape == Shape )
                return !item.IsEmpty;
            else return false;
        }
    }
}

