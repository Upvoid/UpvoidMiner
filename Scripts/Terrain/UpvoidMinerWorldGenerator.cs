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
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

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

        [Serializable]
        public class EntitySave
        {
            [Serializable]
            public class TreeSave
            {
                public float x, y, z;
                public int seed;
            }

            public List<TreeSave> trees = new List<TreeSave>();
        }
        public static EntitySave entitySave = new EntitySave();

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

            // load entities
            LoadEntities();
            
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
                union.AddNode(new CsgExpression(terrainRock.Index, hillsDef + "y + Hills + (5 + perlins(x / 5, z / 6, y / 7) * 3 + perlins(z / 45, y / 46, x / 47) * 13)", UpvoidMiner.ModDomain));
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
            concat.AddNode(new CsgCollapseNode());

            return concat;
        }
        
        public static void SaveEntities()
        {
            Directory.CreateDirectory(new FileInfo(UpvoidMiner.SavePathEntities).Directory.FullName);
            File.WriteAllText(UpvoidMiner.SavePathEntities, JsonConvert.SerializeObject(entitySave, Formatting.Indented));
        }
        public static void LoadEntities()
        {
            if (File.Exists(UpvoidMiner.SavePathEntities))
            {
                entitySave = JsonConvert.DeserializeObject<EntitySave>(File.ReadAllText(UpvoidMiner.SavePathEntities));
                foreach (var item in entitySave.trees)
                    AddTree(new vec3(item.x, item.y, item.z), new Random(item.seed));
            }
        }

        public static void TreeCallback(IntPtr _pos)
        {
            vec3 pos = (vec3)Marshal.PtrToStructure(_pos, typeof(vec3));

            // at least 2m distance
            vec2 pos2D = new vec2(pos.x, pos.z);
            foreach (var tree in entitySave.trees)
                if (vec2.distance(pos2D, new vec2(tree.x, tree.z)) < 2f)
                    return;

            int seed = random.Next();
            entitySave.trees.Add(new EntitySave.TreeSave { x = pos.x, y = pos.y, z = pos.z, seed = seed });
            AddTree(pos, new Random(seed));
        }

        private static void AddTree(vec3 pos, Random random)
        {
            World world = Instance.World;

            vec3 up = new vec3((float)random.NextDouble() * .05f - .025f, 1, (float)random.NextDouble() * .05f - .025f).Normalized;
            vec3 randXZ = ((float)random.NextDouble() * 2.0f - 1.0f) * vec3.UnitX + ((float)random.NextDouble() * 2.0f - 1.0f) * vec3.UnitZ;
            vec3 left = vec3.cross(up, randXZ.Normalized).Normalized;
            vec3 front = vec3.cross(left, up);

            // Scale the trees randomly
            left.x *= 0.8f + 0.4f * (float)random.NextDouble();
            up.y *= 0.7f + 0.6f * (float)random.NextDouble();
            front.z *= 0.8f + 0.4f * (float)random.NextDouble();

            mat4 transform = new mat4(left, up, front, pos);

            //world.AddEntity(TreeGenerator.Birch(8 + (float)random.NextDouble() * 10f, .3f + (float)random.NextDouble() * .1f, random), transform);

            bool type0 = random.NextDouble() > 0.5;

            MeshRenderJob leavesOpaque = new MeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("TreeLeaves01", UpvoidMiner.ModDomain),
                Resources.UseMesh(type0 ? "Vegetation/Tree01/Leaves_high" : "Vegetation/Tree03/Leaves_high", UpvoidMiner.ModDomain),
                transform);

            MeshRenderJob leavesZPre = new MeshRenderJob(
                Renderer.zPre.Mesh,
                Resources.UseMaterial("TreeLeaves01.zPre", UpvoidMiner.ModDomain),
                Resources.UseMesh(type0 ? "Vegetation/Tree01/Leaves_high" : "Vegetation/Tree03/Leaves_high", UpvoidMiner.ModDomain),
                transform);

            MeshRenderJob leavesShadow = new MeshRenderJob(
                Renderer.Shadow.Mesh,
                Resources.UseMaterial("TreeLeaves01.Shadow", UpvoidMiner.ModDomain),
                Resources.UseMesh(type0 ? "Vegetation/Tree01/Leaves_high" : "Vegetation/Tree03/Leaves_high", UpvoidMiner.ModDomain),
                transform);

            MeshRenderJob trunkOpaque = new MeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("TreeTrunk", UpvoidMiner.ModDomain),
                Resources.UseMesh(type0 ? "Vegetation/Tree01/Trunk" : "Vegetation/Tree03/Trunk", UpvoidMiner.ModDomain),
                transform);

            MeshRenderJob trunkShadow = new MeshRenderJob(
                Renderer.Shadow.Mesh,
                Resources.UseMaterial("::Shadow", UpvoidMiner.ModDomain),
                Resources.UseMesh(type0 ? "Vegetation/Tree01/Trunk" : "Vegetation/Tree03/Trunk", UpvoidMiner.ModDomain),
                transform);


            // Add some color variance to trees
            vec4 colorModulation = new vec4(0.7f + (float)random.NextDouble() * 0.5f, 0.7f + (float)random.NextDouble() * 0.5f, 1, 1);
            leavesOpaque.SetColor("uColorModulation", colorModulation);


            world.AddRenderJob(leavesOpaque);
            world.AddRenderJob(leavesZPre);
            world.AddRenderJob(leavesShadow);
            world.AddRenderJob(trunkOpaque);
            world.AddRenderJob(trunkShadow);

        }
    }
}

