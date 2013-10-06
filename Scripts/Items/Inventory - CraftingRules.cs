using System;
using System.Diagnostics;
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

            craftingRules.Add(new ExplicitCraftingRule(new MaterialItem(dirt, MaterialShape.Cube, new vec3(1)),
                                                       new [] { new ResourceItem(dirt, 1f) } ,
                                                       new [] { new ResourceItem(dirt, .5f) } ));
        }
    }
}

