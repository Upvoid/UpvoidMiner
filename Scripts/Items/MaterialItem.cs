using System;
using System.Diagnostics;
using Engine;
using Engine.Universe;

namespace UpvoidMiner
{
    /// <summary>
    /// Shape of a material item.
    /// </summary>
    public enum MaterialShape
    {
        Cube,
        Cylinder,
        Sphere
    }

    /// <summary>
    /// An item that is a material instance of a given resource.
    /// E.g. an Iron Sphere
    /// </summary>
    public class MaterialItem : DiscreteItem
    {
        /// <summary>
        /// The resource that his item is made of.
        /// </summary>
        public readonly TerrainMaterial Material;
        /// <summary>
        /// The shape of this item.
        /// </summary>
        public readonly MaterialShape Shape;
        /// <summary>
        /// Size of this item:
        /// Cube: width, height, depth
        /// Cylinder: height, radius, (z = radius)
        /// Sphere: radius, (y/z = radius)
        /// </summary>
        public readonly vec3 Size;

        /// <summary>
        /// Volume of this material item.
        /// </summary>
        public float Volume
        {
            get 
            {
                switch (Shape)
                {
                    case MaterialShape.Cube: return Size.x * Size.y * Size.z;
                    case MaterialShape.Cylinder: return 2 * (float)Math.PI * Size.x * Size.y * Size.z;
                    case MaterialShape.Sphere: return 4f / 3f * (float)Math.PI * Size.x * Size.y * Size.z;
                    default: Debug.Fail("Invalid shape"); return -1;
                }
            }
        }

        /// <summary>
        /// Gets a textual description of the dimensions
        /// </summary>
        public string DimensionString
        {
            get 
            {
                switch (Shape)
                {
                    case MaterialShape.Cube: return "size " + Size.x.ToString("0.0") + " m x " + Size.y.ToString("0.0") + " m x " + Size.z.ToString("0.0") + " m";
                    case MaterialShape.Cylinder: return Size.y.ToString("0.0") + " m radius and " + Size.x.ToString("0.0") + " m height";
                    case MaterialShape.Sphere: return Size.x.ToString("0.0") + " m radius";
                    default: Debug.Fail("Invalid shape"); return "<invalid>";
                }
            }
        }

        public MaterialItem(TerrainMaterial material, MaterialShape shape, vec3 size, int stackSize = 1):
            base(material.Name + " " + shape, null, 1.0f, false, ItemCategory.Material, stackSize)
        {
            Material = material;
            Shape = shape;
            Size = size;
            Description = "A " + shape + " made of " + material.Name + " with " + DimensionString;
        }

        /// <summary>
        /// This can be merged with material items of the same resource and shape and size.
        /// </summary>
        public override bool TryMerge(Item rhs)
        {
            MaterialItem item = rhs as MaterialItem;
            if ( item == null ) return false;
            if ( item.Material.MaterialIndex != Material.MaterialIndex ) return false;
            if ( item.Shape != Shape ) return false;
            if ( item.Size != Size ) return false;

            StackSize += item.StackSize;
            return true;
        }
    }
}

