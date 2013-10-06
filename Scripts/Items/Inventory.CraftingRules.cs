using System;
using System.Diagnostics;
using System.Collections.Generic;
using Engine;
using Engine.Universe;

namespace UpvoidMiner
{
    // Part of the Inventory: Crafting rules
    public partial class Inventory
    {
        /// <summary>
        /// Initializes all crafting rules
        /// </summary>
        public void InitCraftingRules()
        {
            Debug.Assert(craftingRules.Count == 0, "Expected empty ruleset");

            TerrainMaterial dirt = player.ContainingWorld.Terrain.QueryMaterialFromName("Dirt");
            //TerrainMaterial stone03 = player.ContainingWorld.Terrain.QueryMaterialFromName("Stone.03"); 

            /*craftingRules.Add(new ExplicitCraftingRule(new MaterialItem(dirt, MaterialShape.Cube, new vec3(1)),
                                                       new [] { new ResourceItem(dirt, 1f) } ,
                                                       new [] { new ResourceItem(dirt, .5f) } ));*/

            // MaterialItems
            List<TerrainMaterial> craftMaterials = new List<TerrainMaterial>();
            craftMaterials.Add(dirt);
            for (int i = 1; i <= 14; ++i)
                craftMaterials.Add(player.ContainingWorld.Terrain.QueryMaterialFromName("Stone." + i.ToString("00")));

            foreach (var mat in craftMaterials) 
            {
                craftingRules.Add(new MaterialCraftingRule(mat, MaterialShape.Cube, new vec3(1)));
                craftingRules.Add(new MaterialCraftingRule(mat, MaterialShape.Cylinder, new vec3(1)));
                craftingRules.Add(new MaterialCraftingRule(mat, MaterialShape.Sphere, new vec3(1)));
            }
        }
    }
}

