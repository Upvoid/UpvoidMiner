using System;
using Engine.Universe;
using Engine.Resources;
using Engine.Rendering;

namespace UpvoidMiner
{
    /// <summary>
    /// Implements a world generator to create a basic world with some vegetation.
    /// This class is only available on Host-side.
    /// </summary>
    public class UpvoidMinerWorldGenerator : SimpleWorldGenerator
    {
        /// <summary>
        /// Dirt terrain resource.
        /// </summary>
        private TerrainResource terrainDirt;

        /// <summary>
        /// Initializes the terrain materials and settings.
        /// </summary>
        public override bool init()
        {
            World world = World;
            TerrainEngine terr = world.Terrain;

            // Register all terrain resources (and thus terrain materials).
            TerrainResource.RegisterResources(terr);

            // Get handle to dirt for generation.
            terrainDirt = TerrainResource.FromName("Dirt");
            
            return base.init();
        }
        
        
        /// <summary>
        /// Creates the CSG node network for the terrain generation.
        /// </returns>
        public override CsgNode createTerrain()
        {
            // Load and return a CsgNode based on the "Hills" expression resource. This will create some generic perlin-based hills.
            CsgOpConcat concat = new CsgOpConcat();
            
            CsgOpUnion union = new CsgOpUnion();
            
            ExpressionResource expression = Resources.UseExpression("Hills", HostScript.ModDomain);
            union.AddNode(new CsgExpression(terrainDirt.Index, expression));

            concat.AddNode(union);
            concat.AddNode(new CsgAutomatonNode(Resources.UseAutomaton("Surface", HostScript.ModDomain), World, 4));
            return concat;
        }
    }
}

