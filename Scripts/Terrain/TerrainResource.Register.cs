using System;
using Engine;
using Engine.Universe;
using Engine.Scripting;
using System.Collections.Generic;

namespace UpvoidMiner
{
    /// <summary>
    /// In this part of the class, all resources are registered.
    /// </summary>
    public partial class TerrainResource
    {
        /// <summary>
        /// Registers all the resources.
        /// Currently only implemented for Host.
        /// </summary>
        public static void RegisterResources(TerrainEngine _terrain)
        {
            terrain = _terrain;

            if ( Scripting.IsHost )
            {            
                // Register some simple solid materials.

                // Dirt
                addResource(new VegetatedTerrainResource("Dirt", "Terrain/Dirt", "::Particle/Rock"));

                // Stones
                for (int i = 1; i <= 14; ++i) 
                    addResource(new SolidTerrainResource("Stone." + i.ToString("00"), "Terrain/Rock" + i.ToString("00"), "::Particle/Rock"));

                // Wood
                addResource(new SolidTerrainResource("Wood", "Terrain/Wood", "::Particle/Rock"));

                // Ores + Metals
                addResource(new SolidTerrainResource("Coal", "Terrain/Coal", "::Particle/Rock"));
                addResource(new SolidTerrainResource("Copper", "Terrain/Copper", "::Particle/Rock"));
                addResource(new SolidTerrainResource("Iron", "Terrain/Iron", "::Particle/Rock"));
                addResource(new SolidTerrainResource("Gold", "Terrain/Gold", "::Particle/Rock"));

                // Rares
                addResource(new SolidTerrainResource("AoiCrystal", "Terrain/AoiCrystal", "::Particle/Rock"));
                addResource(new SolidTerrainResource("FireRock", "Terrain/FireRock", "::Particle/Rock"));
                addResource(new SolidTerrainResource("AlienRock", "Terrain/AlienRock", "::Particle/Rock"));

            }
            else
            {
                // TODO: implement client-side
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// The associated terrain engine
        /// </summary>
        private static TerrainEngine terrain;
        /// <summary>
        /// Mapping from material index to terrain resource.
        /// </summary>
        private static Dictionary<int, TerrainResource> indexToResource = new Dictionary<int, TerrainResource>();
        /// <summary>
        /// Mapping from material name to terrain resource.
        /// </summary>
        private static Dictionary<string, TerrainResource> nameToResource = new Dictionary<string, TerrainResource>();
        /// <summary>
        /// Gets a terrain resource based on material index
        /// </summary>
        public static TerrainResource FromIndex(int idx) 
        {
            TerrainResource res;
            if (indexToResource.TryGetValue(idx, out res))
                return res;
            else
                return null;
        }
        /// <summary>
        /// Gets a terrain resource based on material name
        /// </summary>
        public static TerrainResource FromName(string name) 
        {
            TerrainResource res;
            if (nameToResource.TryGetValue(name, out res))
                return res;
            else
                return null;
        }
        
        /// <summary>
        /// Adds a resource to the global dictionary.
        /// </summary>
        private static void addResource(TerrainResource res)
        {
            indexToResource.Add(res.Index, res);
            nameToResource.Add(res.Name, res);
        }
    }
}

