using System;
using Engine.Universe;
using System.Diagnostics;
using System.Collections.Generic;

namespace UpvoidMiner
{
    /// <summary>
    /// A material of the terrain.
    /// </summary>
    public abstract partial class TerrainResource
    {
        /// <summary>
        /// The underlying terrain material.
        /// </summary>
        public readonly TerrainMaterial Material;
        /// <summary>
        /// The index of the material
        /// </summary>
        public readonly int Index;
        /// <summary>
        /// Material name.
        /// </summary>
        public readonly string Name;

        public TerrainResource(string name, bool translucent = false)
        {
            Material = terrain.RegisterMaterial(name, translucent);
            Index = Material.MaterialIndex;
            Name = name;
        }
    }
}

