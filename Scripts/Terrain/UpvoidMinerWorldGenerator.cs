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
using Engine.Statistics;
using System.Text;
using Engine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Engine.Nodes;

namespace UpvoidMiner
{
    /// <summary>
    /// Implements a world generator to create a basic world with some vegetation.
    /// This class is only available on Host-side.
    /// </summary>
    public class UpvoidMinerWorldGenerator
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
        /// Backref to the world
        /// </summary>
        private World world;
        /// <summary>
        /// Dirt terrain resource.
        /// </summary>
        private TerrainResource terrainDirt;
        private TerrainResource terrainRock;
        private TerrainResource terrainCopperOre;
        private TerrainResource terrainDesert;
        private TerrainResource terrainOreGold;

        [Serializable]
        public class EntitySave
        {

            [Serializable]
            public class TreeSave
            {
                public float x, y, z;
                public int seed;
                public Tree.TreeType type;
            }

            public List<TreeSave> trees = new List<TreeSave>();
        }

        public static EntitySave entitySave = new EntitySave();
        public static List<Tree> trees = new List<Tree>();


        public static void setTreeLodSettings(float minFadeDistance, float maxFadeDistance, float fadeTime)
        {
            foreach (Tree t in trees)
            {
                // Configure LoD for all Leaves-Renderjobs
                foreach (RenderComponent r in t.RjLeaves0)
                {
                    r.ConfigureLod(minFadeDistance, maxFadeDistance, fadeTime);
                }

                // Configure LoD for all Trunk-Renderjobs
                foreach (RenderComponent r in t.RjTrunk)
                {
                    r.ConfigureLod(minFadeDistance, maxFadeDistance, fadeTime);
                }
            }
        }


        // Updates all trees and returns position of closest one
        public static vec3 UpdateTrees(vec3 refPos)
        {
            using (new ProfileAction("UpdateTrees", UpvoidMiner.Mod))
            {

                float minDist = float.MaxValue;
                vec3 closestTree = new vec3(float.MaxValue);

                foreach (Tree t in trees)
                {
                    float dis = vec3.distance(refPos, t.Position);

                    // Keep distance to closest tree
                    if (dis < minDist)
                    {
                        minDist = dis;
                        closestTree = t.Position;
                    }

                }

                // Return distance to closest tree
                return closestTree;
            }
        }

        /// <summary>
        /// Initializes the terrain materials and settings.
        /// </summary>
        public static void init(World world)
        {
            Instance = new UpvoidMinerWorldGenerator();
            Instance.world = world;

            // Register all terrain resources (and thus terrain materials).
            TerrainResource.RegisterResources(world.Terrain);

            // Get handle to dirt for generation.
            Instance.terrainDirt = TerrainResource.FromName("Dirt");
            Instance.terrainRock = TerrainResource.FromName("Basalt");
            Instance.terrainCopperOre = TerrainResource.FromName("CopperOre");
            Instance.terrainDesert = TerrainResource.FromName("Sand");
            Instance.terrainOreGold = TerrainResource.FromName("OreGold");

            UpvoidMiner.SavePathEntities = UpvoidMiner.SavePathBase + "/Entities";
            UpvoidMiner.SavePathWorldItems = UpvoidMiner.SavePathBase + "/WorldItems";

            // load entities
            LoadEntities();

            // init terrain
            world.SetTerrainGenerator(new TerrainGenerator());
            world.TerrainGenerator.SetCsgNode(Instance.createTerrain());
        }

        /// <summary>
        /// Creates the CSG node network for the terrain generation.
        /// </returns>
        public CsgNode createTerrain()
        {
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
                hillsDefines.Append("Hills = perlins(x / 30, y / 30, z / 30) * .25 + perlins(x / 10, y / 10, z / 10) * .05;");
                hillsDefines.Append("Hills = Hills * 20 * perlins(x / 300, y / 300, z / 300);");
                string hillsDef = hillsDefines.ToString();

                var texture = new TextureSampleNode(Resources.UseTextureData("Heightmap", UpvoidMiner.ModDomain),
                                                    TextureSampleNode.CoordinateMode.Relative,
                                                    TextureSampleNode.InterpolationMode.Linear,
                                                    TextureSampleNode.WrapUVWMode.Repeat,
                                                    0f);

                string heightmapRocks = "Heightmap = -225 * (texture(x/5000, z/5000).x - 0.5);";
                string heightmapDirt = "Heightmap = -225 * (texture(x/5000, z/5000).y - 0.5);";

                var dirtNodeNetwork = NodeNetworkHelper.CreateExprWithTextureSampling(hillsDef + heightmapDirt + "y + Heightmap + Hills", UpvoidMiner.ModDomain, "texture", texture);
                union.AddNode(new CsgExpression(terrainDirt.Index, dirtNodeNetwork));
                var rockNodeNetwork = NodeNetworkHelper.CreateExprWithTextureSampling(hillsDef + heightmapRocks + "y + Heightmap + 4 + Hills*0.2", UpvoidMiner.ModDomain, "texture", texture);
                union.AddNode(new CsgExpression(terrainRock.Index, rockNodeNetwork));

/*
                string oreDef = @"perlins(x,y,z) $= ::Perlin;
                                    Ore = 1.5 + perlins(x / 20, y / 4, z / 20) + perlins(x / 4, y / 0.5, z / 4);
                                ";
*/

                //var goldNodeNetwork = NodeNetworkHelper.CreateExprWithTextureSampling(hillsDef + oreDef + "Ore + clamp(y+20, -10, 0)*0.1", UpvoidMiner.ModDomain, "texture", texture);
                //union.AddNode(new CsgExpression(terrainOreGold.Index, goldNodeNetwork));

                //union.AddNode(groundTerrain);
                //union.AddNode(new CsgExpression(terrainRock.Index, hillsDef + "y + Hills + (5 + perlins(x / 5, z / 6, y / 7) * 3 + perlins(z / 45, y / 46, x / 47) * 13)", UpvoidMiner.ModDomain));
                concat.AddNode(union);
            }
            {
                StringBuilder copperDefines = new StringBuilder();
                copperDefines.Append("pos = vec3(x, y, z); start = 20; end = -80;");
                copperDefines.Append("center = (start+end)/2; width = (start-end)/2;");
                copperDefines.Append("perlins(x,y,z) $= ::Perlin;");
                copperDefines.Append("Weight = -step(step(y-end)+step(start-y)-1.5);");
                copperDefines.Append("fY = y/20; pX = perlins(fY*1.23+x/20,fY,fY*0.5439+z/20)*20+x;pZ = perlins(fY*2.143+x/20,fY,-fY*0.239+z/20)*20+z;");
                copperDefines.Append("Density = (perlins(pX/10,y/80,pZ/10)+1)/2;");
                copperDefines.Append("Weight + Density + 0.8");
                string copperDef = copperDefines.ToString();
                CsgExpression copperExpression = new CsgExpression(terrainCopperOre.Index,copperDef,UpvoidMiner.ModDomain);
                CsgOpUnion union = new CsgOpUnion();
                union.AddNode(new CsgExpression(terrainRock.Index,"1",null));
                union.AddNode(copperExpression);
                CsgFilterNode filter = new CsgFilterNode(true,union);
                filter.AddMaterial(terrainRock.Index);
                concat.AddNode(filter);
            }
            /*
            {

                CsgOpDiff diff = new CsgOpDiff();

                StringBuilder caveDefines = new StringBuilder();
                // Introduce variables.
                caveDefines.Append("pos = vec3(x, y, z);");
                // Import perlin noise.
                caveDefines.Append("perlins(x,y,z) $= ::Perlin;");
                // Cave structue.
                //hillsDefines.Append("Hills = perlins(x / 300, 0.00001 * y, z / 300) + perlins(x / 100, 0.00001 * y, z / 100) * .5 + perlins(x / 30, 0.00001 * y, z / 30) * .25 + perlins(x / 10, 0.00001 * y, z / 10) * .05;");
                caveDefines.Append("perlinOffsetX = perlins(x, 1, 1);");
                caveDefines.Append("perlinOffsetZ =  perlins(1, 1, 1);");
                caveDefines.Append("CaveDensity = 0.3 + abs(perlins((x+perlinOffsetX) / 30, y / 30, (z+perlinOffsetZ) / 30));");
                caveDefines.Append("Caves = CaveDensity;");
                string caveDef = "y";// caveDefines.ToString();

                diff.AddNode(new CsgExpression(1, "y", UpvoidMiner.ModDomain));
                concat.AddNode(diff);
            }
            */
            concat.AddNode(new CsgAutomatonNode(Resources.UseAutomaton("Trees", UpvoidMiner.ModDomain), world, 4));
            //concat.AddNode(new CsgAutomatonNode(Resources.UseAutomaton("DesertVegetation", UpvoidMiner.ModDomain), world, 4));
            concat.AddNode(new CsgAutomatonNode(Resources.UseAutomaton("Surface", UpvoidMiner.ModDomain), world, 4));


            // No collapsing terrain for now
            //concat.AddNode(new CsgCollapseNode());


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
                var trees = entitySave.trees;
                entitySave.trees = new List<EntitySave.TreeSave>();
                foreach (var item in trees)
                    TreeCreate(new vec3(item.x, item.y, item.z), item.seed, item.type);
            }
        }

        public static void TreeCallback(IntPtr _pos)
        {
            vec3 pos = (vec3)Marshal.PtrToStructure(_pos, typeof(vec3));
            int seed = random.Next();
            TreeCreate(pos, seed, Tree.TreeType.Birch);
        }

        public static void CactusCallback(IntPtr _pos)
        {
            vec3 pos = (vec3)Marshal.PtrToStructure(_pos, typeof(vec3));
            int seed = random.Next();
            TreeCreate(pos, seed, Tree.TreeType.Cactus);
        }

        public static void TreeCreate(vec3 pos, int seed, Tree.TreeType type)
        {
            // at least 6m distance
            vec2 pos2D = new vec2(pos.x, pos.z);
            foreach (var tree in entitySave.trees)
                if (vec2.distance(pos2D, new vec2(tree.x, tree.z)) < 6f)
                    return;

            Random random = new Random(seed);
            entitySave.trees.Add(new EntitySave.TreeSave { x = pos.x, y = pos.y, z = pos.z, seed = seed, type = type });
            AddTree(pos, random, type);
        }

        private static void AddTree(vec3 pos, Random random, Tree.TreeType type)
        {
            World world = Instance.world;

            vec3 up = new vec3((float)random.NextDouble() * .05f - .025f, 1, (float)random.NextDouble() * .05f - .025f).Normalized;
            vec3 randXZ = ((float)random.NextDouble() * 2.0f - 1.0f) * vec3.UnitX + ((float)random.NextDouble() * 2.0f - 1.0f) * vec3.UnitZ;
            vec3 left = vec3.cross(up, randXZ.Normalized).Normalized;
            vec3 front = vec3.cross(left, up);

            // Scale the trees randomly
            left.x *= 0.8f + 0.4f * (float)random.NextDouble();
            up.y *= 0.7f + 0.6f * (float)random.NextDouble();
            front.z *= 0.8f + 0.4f * (float)random.NextDouble();

            mat4 transform1 = mat4.Translate(pos);
            mat4 transform2 = new mat4(left, up, front, vec3.Zero);

            //world.AddEntity(TreeGenerator.Birch(8 + (float)random.NextDouble() * 10f, .3f + (float)random.NextDouble() * .1f, random), transform);

            Tree t = null;
            switch (type)
            {
                case Tree.TreeType.Birch:
                    t = TreeGenerator.OldTree(random, transform1, transform2, world);
                    break;
                case Tree.TreeType.Cactus:
                    t = TreeGenerator.Cactus(random, transform1, transform2, world);
                    break;
            }

            t.Position = pos;
            world.AddEntity(t, transform1);
            trees.Add(t);
        }
    }
}

