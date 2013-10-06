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
        
        public ResourceItem(TerrainMaterial material, float volume = 0f) :
            base(material.Name, "The terrain resource " + material.Name, 1.0f, false, ItemCategory.Resources, volume)
        {
            Material = material;
        }

        /// <summary>
        /// This can be merged with resource items of the same resource
        /// </summary>
        public override bool TryMerge(Item rhs, bool subtract, bool force, bool dryrun = false)
        {
            ResourceItem item = rhs as ResourceItem;
            if ( item == null ) return false;
            if ( item.Material.MaterialIndex != Material.MaterialIndex ) return false;

            return Merge(item, subtract, force, dryrun);
        }

        /// <summary>
        /// Creates a copy of this item.
        /// </summary>
        public override Item Clone()
        {
            return new ResourceItem(Material, Volume);
        }
    }
}

