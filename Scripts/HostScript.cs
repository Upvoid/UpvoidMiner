/*
 *    Copyright (C) by Upvoid Studios
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <http://www.gnu.org/licenses/>
 */

using System;
using Engine;
using Engine.Universe;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Rendering;
using Engine.Physics;
using Engine.Input;
using System.Collections.Generic;
using System.Runtime.InteropServices;


namespace UpvoidMiner
{
    /// <summary>
    /// Implements a world generator to create a basic world with some vegetation.
    /// </summary>
    public class UpvoidMinerWorldGenerator : SimpleWorldGenerator
	{
		TerrainMaterial MatGround;
		TerrainMaterial MatStone;

		/// <summary>
		/// Initializes the terrain materials and settings.
		/// </summary>
		public override bool init()
		{
			World world = World;
			TerrainEngine terr = world.Terrain;

			{
				// For now, register a single ground material.
				MatGround = terr.RegisterMaterial("Ground");

				// Add the geometry for the terrain LoDs >= 0.
				int pipeline = MatGround.AddDefaultPipeline(0);
				MatGround.AddDefaultShadowAndZPre(pipeline);
				MatGround.AddMeshMaterial(pipeline, "Output", Resources.UseMaterial("GrassyMountains2", HostScript.ModDomain), Renderer.Opaque.Mesh);

				// Spawn Grass
				pipeline = MatGround.AddPipeline(Resources.UseGeometryPipeline("GrassField", HostScript.ModDomain), "Input", "Input", 0, 4);
				MatGround.AddMeshMaterial(pipeline, "Spawns", Resources.UseMaterial("Grass01", HostScript.ModDomain), Renderer.Opaque.Mesh);

				// Spawn Grass
				pipeline = MatGround.AddPipeline(Resources.UseGeometryPipeline("GrassField2", HostScript.ModDomain), "Input", "Input", 0, 4);
				MatGround.AddMeshMaterial(pipeline, "Spawns", Resources.UseMaterial("Grass01", HostScript.ModDomain), Renderer.Opaque.Mesh);

				// Spawn Herbs
				pipeline = MatGround.AddPipeline(Resources.UseGeometryPipeline("HerbField", HostScript.ModDomain), "Input", "Input", 0, 4);
				MatGround.AddMeshMaterial(pipeline, "Spawns", Resources.UseMaterial("Herbs17", HostScript.ModDomain), Renderer.Opaque.Mesh);

				// Spawn Herbs
				pipeline = MatGround.AddPipeline(Resources.UseGeometryPipeline("HerbField2", HostScript.ModDomain), "Input", "Input", 0, 4);
				MatGround.AddMeshMaterial(pipeline, "Spawns", Resources.UseMaterial("Herbs18", HostScript.ModDomain), Renderer.Opaque.Mesh);

				// Add the geometry for the terrain LoDs 5-8. Add some tree impostors to make the ground look nicer.
				pipeline = MatGround.AddPipeline(Resources.UseGeometryPipeline("PineImpostorField", HostScript.ModDomain), "Input", "", 5, 8);
				MatGround.AddMeshMaterial(pipeline, "PineSpawns", Resources.UseMaterial("PineImpostor", HostScript.ModDomain), Renderer.Opaque.Mesh);

				// For terrain LoDs 0-4, spawn "real" tree models instead of the impostors.
				pipeline = MatGround.AddPipeline(Resources.UseGeometryPipeline("PineField", HostScript.ModDomain), "Input", "", 0, 4);
				MatGround.AddMeshMaterial(pipeline, "PineSpawns", Resources.UseMaterial("PineLeaves", HostScript.ModDomain), Renderer.Opaque.Mesh);
			}

                        // Register some simple stone materials.
                        for (int i = 1; i <= 14; ++i)
                        {
                                MatStone = terr.RegisterMaterial("Stone." + i.ToString("00"));
                                int pipeline = MatStone.AddDefaultPipeline(0);
                                MatStone.AddDefaultShadowAndZPre(pipeline);
                                MatStone.AddMeshMaterial(pipeline, "Output", Resources.UseMaterial("Terrain/Rock" + i.ToString("00"), HostScript.ModDomain), Renderer.Opaque.Mesh);
                        }
                        {
                                MatStone = terr.RegisterMaterial("FireRock");
                                int pipeline = MatStone.AddDefaultPipeline(0);
                                MatStone.AddDefaultShadowAndZPre(pipeline);
                                MatStone.AddMeshMaterial(pipeline, "Output", Resources.UseMaterial("Terrain/FireRock", HostScript.ModDomain), Renderer.Opaque.Mesh);
                        }
                        {
                                MatStone = terr.RegisterMaterial("AlienRock");
                                int pipeline = MatStone.AddDefaultPipeline(0);
                                MatStone.AddDefaultShadowAndZPre(pipeline);
                                MatStone.AddMeshMaterial(pipeline, "Output", Resources.UseMaterial("Terrain/AlienRock", HostScript.ModDomain), Renderer.Opaque.Mesh);
                        }

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
				union.AddNode(new CsgExpression(MatGround.MaterialIndex, expression));
				//union.AddNode(new CsgMeshNode(MatGround.MaterialIndex, Resources.UseMesh("::Debug/Monkey", HostScript.ModDomain), mat4.Translate(new vec3(-30.1f, 10.1f, 0.1f)) * mat4.Scale(10)));

				concat.AddNode(union);
				concat.AddNode(new CsgAutomatonNode(Resources.UseAutomaton("StoneSpawner", HostScript.ModDomain), World, 4));
				return concat;
		}
    }

    /// <summary>
    /// Main class for the host script.
    /// </summary>
    public class HostScript
    {
        public static Module Mod;
        public static ResourceDomain ModDomain;

        /// <summary>
        /// Starts
        /// </summary>
        public static void Startup(IntPtr _unmanagedModule)
        {
            // Get and save the resource domain of the mod, needed for loading resources.
            Mod = Module.FromHandle(_unmanagedModule);
            ModDomain = Mod.ResourceDomain;

            // Create the world. Multiple worlds could be created here, but we only want one.
            // Use the UpvoidMinerWorldGenerator, which will create a simple terrain with some vegetation.
            World world = Universe.CreateWorld("UpvoidMinerWorld", new UpvoidMinerWorldGenerator());

	        for(int i = 0; i<3; ++i) {

	            Item testItem = new Item(
	                "TestItem",
	                "A test item.",
	                1f,
	                false,
	                Resources.UseMaterial("::Rock", ModDomain),
	                Resources.UseMesh("::Debug/Sphere", ModDomain),
	                1f
	            );

	            world.AddEntity(new ItemEntity(testItem), mat4.Translate(new vec3(5f, i*2f, ((i%3)*2f))));
	        }

        }
    }
}
