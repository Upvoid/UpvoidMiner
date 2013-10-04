using System;
using Engine.Universe;

namespace UpvoidMiner
{
    /// <summary>
    /// An item that represents a resource based on a terrain material
    /// </summary>
    public class ResourceItem : VolumeItem
    {
        /// <summary>
        /// The terrain material that this resource represents.
        /// </summary>
        public readonly TerrainMaterial Material;
        
        public ResourceItem(TerrainMaterial material) :
            base(material.Name, "The terrain resource " + material.Name, 1.0f, false, ItemCategory.Resources)
        {
            Material = material;
        }
    }
}

