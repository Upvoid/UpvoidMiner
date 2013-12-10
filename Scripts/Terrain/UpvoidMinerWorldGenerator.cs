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
using Engine.Universe;
using Engine.Resources;
using Engine.Rendering;
using System.Text;
using Engine;
using System.Runtime.InteropServices;

namespace UpvoidMiner
{
    /// <summary>
    /// Implements a world generator to create a basic world with some vegetation.
    /// This class is only available on Host-side.
    /// </summary>
    public class UpvoidMinerWorldGenerator : SimpleWorldGenerator
    {
        /// <summary>
        /// Singleton
        /// </summary>
        private static UpvoidMinerWorldGenerator Instance;
        /// <summary>
        /// Random
        /// </summary>
        private static Random random = new Random();

        /// <summary>
        /// Dirt terrain resource.
        /// </summary>
        private TerrainResource terrainDirt;
        private TerrainResource terrainRock;

        /// <summary>
        /// Initializes the terrain materials and settings.
        /// </summary>
        public override bool init()
        {
            Instance = this;
            World world = World;
            TerrainEngine terr = world.Terrain;

            // Register all terrain resources (and thus terrain materials).
            TerrainResource.RegisterResources(terr);

            // Get handle to dirt for generation.
            terrainDirt = TerrainResource.FromName("Dirt");
            terrainRock = TerrainResource.FromName("Stone.09");
            
            return base.init();
        }
        
        
        /// <summary>
        /// Creates the CSG node network for the terrain generation.
        /// </returns>
        public override CsgNode createTerrain()
        {
            // Load and return a CsgNode based on the "Hills" expression resource. This will create some generic perlin-based hills.
            CsgOpConcat concat = new CsgOpConcat();

            {
                CsgOpUnion union = new CsgOpUnion();
                StringBuilder hillsDefines = new StringBuilder();
                // Introduce variables.
                hillsDefines.Append("pos = vec3(x, y, z);");
                // Import perlin noise.
                hillsDefines.Append("perlins(x,y,z) $= ::Perlin;");
                hillsDefines.Append("perlin(v) = perlins(v.x, v.y, v.z);");
                // Hill structue.
                //hillsDefines.Append("Hills = perlins(x / 300, 0.00001 * y, z / 300) + perlins(x / 100, 0.00001 * y, z / 100) * .5 + perlins(x / 30, 0.00001 * y, z / 30) * .25 + perlins(x / 10, 0.00001 * y, z / 10) * .05;");
                hillsDefines.Append("Hills = perlins(x / 300, y / 300, z / 300) + perlins(x / 100, y / 100, z / 100) * .5 + perlins(x / 30, y / 30, z / 30) * .25 + perlins(x / 10, y / 10, z / 10) * .05;");
                hillsDefines.Append("Hills = (Hills + 1).pow2 * 50;");
                string hillsDef = hillsDefines.ToString();

                union.AddNode(new CsgExpression(terrainDirt.Index, hillsDef + "y + Hills", UpvoidMiner.ModDomain));
                union.AddNode(new CsgExpression(terrainRock.Index, hillsDef + "y + Hills + (5 + perlins(x / 20, z / 21, y / 22) * 3)", UpvoidMiner.ModDomain));
                concat.AddNode(union);
            }

            {
                CsgOpDiff diff = new CsgOpDiff();
                StringBuilder caveDefines = new StringBuilder();
                // Introduce variables.
                caveDefines.Append("pos = vec3(x, y, z);");
                // Import perlin noise.
                caveDefines.Append("perlins(x,y,z) $= ::Perlin;");
                caveDefines.Append("perlin(v) = perlins(v.x, v.y, v.z);");
                // Cave structue.
                //hillsDefines.Append("Hills = perlins(x / 300, 0.00001 * y, z / 300) + perlins(x / 100, 0.00001 * y, z / 100) * .5 + perlins(x / 30, 0.00001 * y, z / 30) * .25 + perlins(x / 10, 0.00001 * y, z / 10) * .05;");
                caveDefines.Append("CaveDensity = clamp(perlins(x / 30, y / 30, z / 30) * 4, 0, 1);");
                caveDefines.Append("CaveDensity = CaveDensity * clamp((y + 100) * .2, 0, 1);");
                caveDefines.Append("Caves = (perlins(x / 17, y / 17, z / 17) + perlins(x / 6, y / 6, z / 6) * .5 - .2) + (5 - CaveDensity * 5);");
                string caveDef = caveDefines.ToString();

                diff.AddNode(new CsgExpression(1, caveDef + "Caves", UpvoidMiner.ModDomain));
                concat.AddNode(diff);
            }

            concat.AddNode(new CsgAutomatonNode(Resources.UseAutomaton("Trees", UpvoidMiner.ModDomain), World, 4));
            concat.AddNode(new CsgAutomatonNode(Resources.UseAutomaton("Surface", UpvoidMiner.ModDomain), World, 4));

            return concat;
        }

        public static void TreeCallback(IntPtr _pos)
        {
            vec3 pos = (vec3)Marshal.PtrToStructure(_pos, typeof(vec3));

            World world = Instance.World;

            vec3 up = new vec3((float)random.NextDouble() * .05f - .025f, 1, (float)random.NextDouble() * .05f - .025f).Normalized;
            vec3 left = vec3.cross(up, vec3.UnitZ).Normalized;
            vec3 front = vec3.cross(left, up);

            mat4 transform = new mat4(left, up, front, pos);

            world.AddEntity(TreeGenerator.Birch(8 + (float)random.NextDouble() * 10f, .3f + (float)random.NextDouble() * .1f), transform);
        }
    }
}

