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
            
            TerrainResource dirt = TerrainResource.FromName("Dirt");
            TerrainResource wood = TerrainResource.FromName("Wood");
            TerrainResource iron = TerrainResource.FromName("Iron");
            TerrainResource coal = TerrainResource.FromName("Coal");
            TerrainResource gold = TerrainResource.FromName("Gold");
            TerrainResource copper = TerrainResource.FromName("Copper");
            TerrainResource aoicrystal = TerrainResource.FromName("AoiCrystal");

            /*craftingRules.Add(new ExplicitCraftingRule(new MaterialItem(dirt, MaterialShape.Cube, new vec3(1)),
                                                       new [] { new ResourceItem(dirt, 1f) } ,
                                                       new [] { new ResourceItem(dirt, .5f) } ));*/

            // Tools
            
            craftingRules.Add(new ExplicitCraftingRule(new ToolItem(ToolType.Pickaxe),
                                                       (new [] { new ResourceItem(iron, 1f), new ResourceItem(wood, 1f) }),
                                                       (new [] { new ResourceItem(iron, .1f), new ResourceItem(wood, .2f) })));
            craftingRules.Add(new ExplicitCraftingRule(new ToolItem(ToolType.Axe),
                                                       (new [] { new ResourceItem(iron, 1f), new ResourceItem(wood, 1f) }),
                                                       (new [] { new ResourceItem(iron, .1f), new ResourceItem(wood, .2f) })));
            craftingRules.Add(new ExplicitCraftingRule(new ToolItem(ToolType.Shovel),
                                                       (new [] { new ResourceItem(iron, 1f), new ResourceItem(wood, 1f) }),
                                                       (new [] { new ResourceItem(iron, .1f), new ResourceItem(wood, .2f) })));
            craftingRules.Add(new ExplicitCraftingRule(new ToolItem(ToolType.SteamHammer),
                                                       (new [] { new ResourceItem(iron, 4f), new ResourceItem(wood, 2f) }),
                                                       (new [] { new ResourceItem(iron, .2f), new ResourceItem(wood, .3f) })));

            // MaterialItems
            List<TerrainResource> craftMaterials = new List<TerrainResource>();
            craftMaterials.Add(dirt);
            craftMaterials.Add(wood);
            craftMaterials.Add(coal);
            craftMaterials.Add(iron);
            craftMaterials.Add(gold);
            craftMaterials.Add(aoicrystal);
            craftMaterials.Add(copper);
            for (int i = 1; i <= 14; ++i)
                craftMaterials.Add(TerrainResource.FromName("Stone." + i.ToString("00")));

            foreach (var mat in craftMaterials) 
            {
                craftingRules.Add(new MaterialCraftingRule(mat, MaterialShape.Cube, new vec3(1)));
                craftingRules.Add(new MaterialCraftingRule(mat, MaterialShape.Cylinder, new vec3(1)));
                craftingRules.Add(new MaterialCraftingRule(mat, MaterialShape.Sphere, new vec3(1)));
            }
        }
    }
}

