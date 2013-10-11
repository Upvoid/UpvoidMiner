using System;
using Engine.Universe;
using Engine.Resources;
using Engine.Rendering;
using System.Text;

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
        private TerrainResource terrainRock04;

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
            terrainRock04 = TerrainResource.FromName("Stone.04");
            
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

            union.AddNode(new CsgExpression(terrainDirt.Index, hillsDef + "y + Hills", HostScript.ModDomain));
            //union.AddNode(new CsgExpression(terrainRock04.Index, hillsDef + "y + Hills", HostScript.ModDomain));

            concat.AddNode(union);
            concat.AddNode(new CsgAutomatonNode(Resources.UseAutomaton("Surface", HostScript.ModDomain), World, 4));
            return concat;
        }
    }
}

