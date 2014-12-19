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
using Engine;
using Engine.Universe;
using Engine.Scripting;
using Engine.Rendering;
using Engine.Resources;
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

            // Register some simple solid materials.

            // Dirt
            addResource(new VegetatedTerrainResource(new DirtSubstance(), "Terrain/Dirt", "Particles/Terrain/Dirt", 830f));


            

            SolidTerrainResource desertResource = new SolidTerrainResource(new SandSubstance(), "Terrain/Desert", "Particles/Terrain/Desert", 1320f);
            desertResource.Material.AddAttributeFloat("aParviflora", 0, 0, 4);

            // Spawn Parviflora
            {
                int pipeline = desertResource.Material.AddPipeline(Resources.UseGeometryPipeline("ParvifloraField", UpvoidMiner.ModDomain), "Input", "", 0, 4);
                desertResource.Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("Parviflora", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
                desertResource.Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("Parviflora.ShadowDecal", UpvoidMiner.ModDomain), Renderer.Transparent.Mesh);
            }

            // Desert
            addResource(desertResource);

            // Stones
            addResource(new SolidTerrainResource(new BasaltSubstance(), "Terrain/Rock09", "Particles/Terrain/Rock09", 2700f));

            // Wood
            addResource(new SolidTerrainResource(new BirchWoodSubstance(), "Terrain/BirchWood", "Particles/Terrain/BirchWood", 650f));

            // Ores + Metals
            addResource(new SolidTerrainResource(new CharcoalSubstance(), "Terrain/Charcoal", "Particles/Terrain/Charcoal", 1300f));
            addResource(new SolidTerrainResource(new BlackCoalSubstance(), "Terrain/BlackCoal", "Particles/Terrain/BlackCoal", 1300f));
            addResource(new SolidTerrainResource(new CopperSubstance(), "Terrain/Copper", "Particles/Terrain/Copper", 9001f));
            addResource(new SolidTerrainResource(new TinSubstance(), "Terrain/Placeholder", "Particles/Terrain/Placeholder", -1f));
            addResource(new SolidTerrainResource(new BronzeSubstance(), "Terrain/Placeholder", "Particles/Terrain/Placeholder", -1f));
            addResource(new SolidTerrainResource(new IronSubstance(), "Terrain/Iron", "Particles/Terrain/Iron", 7700f));
            addResource(new SolidTerrainResource(new SteelSubstance(), "Terrain/Placeholder", "Particles/Terrain/Placeholder", -1f));
            addResource(new SolidTerrainResource(new GoldSubstance(), "Terrain/Gold", "Particles/Terrain/Gold", 19302f));

            addResource(new SolidTerrainResource(new CopperOreSubstance(), "Terrain/CopperOre", "Particles/Terrain/CopperOre", 4900f));
            addResource(new SolidTerrainResource(new TinOreSubstance(), "Terrain/Placeholder", "Particles/Terrain/Placeholder", -1f));
            addResource(new SolidTerrainResource(new IronOreSubstance(), "Terrain/Placeholder", "Particles/Terrain/Placeholder", -1f));
            addResource(new SolidTerrainResource(new GoldOreSubstance(), "Terrain/OreGold", "Particles/Terrain/OreGold", 4000f));

            // Rares
            addResource(new SolidTerrainResource(new VerdaniumOreSubstance(), "Terrain/VerdaniumOre", "Particles/Terrain/VerdaniumOre", 2900f));
            addResource(new SolidTerrainResource(new AegiriumSubstance(), "Terrain/BlueCrystal", "Particles/Terrain/BlueCrystal", 3500f));
            addResource(new SolidTerrainResource(new FireRockSubstance(), "Terrain/FireRock", "Particles/Terrain/FireRock", 2900f));

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
            return indexToResource.TryGetValue(idx, out res) ? res : null;
        }

        /// <summary>
        /// Gets a terrain resource based on material name
        /// </summary>
        public static TerrainResource FromName(string name)
        {
            TerrainResource res;
            return nameToResource.TryGetValue(name, out res) ? res : null;
        }

        public static IEnumerable<TerrainResource> ListResources()
        {
            return indexToResource.Values;
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

