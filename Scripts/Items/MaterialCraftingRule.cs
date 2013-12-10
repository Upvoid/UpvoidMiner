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
        public readonly TerrainResource Material;

        /// <summary>
        /// The shape of the material.
        /// </summary>
        public readonly MaterialShape Shape;

        /// <summary>
        /// Currently configured sizes
        /// </summary>
        public vec3 Size;

        public MaterialCraftingRule(TerrainResource material, MaterialShape shape, vec3 size)
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
                item.Material.Material == Material.Material &&
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
                item.Material.Material == Material.Material &&
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
                item.Material.Material == Material.Material &&
                item.Shape == Shape )
                return !item.IsEmpty;
            else return false;
        }
    }
}

