using System;
using Engine;
using Engine.Model;
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

		public static void Main() {}

		/// <summary>
		/// Initializes the terrain materials and settings.
		/// </summary>
		public override bool init()
		{
			World world = World;
			TerrainEngine terr = world.Terrain;

			// For now, register a single ground material.
			MatGround = terr.RegisterMaterial("Ground");

			// Add the geometry for the terrain LoDs >= 9.
			int pipeline = MatGround.AddDefaultPipeline(9);
			MatGround.AddDefaultShadowAndZPre(pipeline);
			MatGround.AddMeshMaterial(pipeline, "Output", Resources.UseMaterial("::Terrain/GrassyMountains", HostScript.ModDomain), Renderer.Opaque.Mesh);

			// Add the geometry for the terrain LoDs 5-8. Add some tree impostors to make the ground look nicer.
			pipeline = MatGround.AddPipeline(Resources.UseGeometryPipeline("PineImpostorField", HostScript.ModDomain), "Input", 5, 8);
			MatGround.AddDefaultShadowAndZPre(pipeline, "Input");
			MatGround.AddMeshMaterial(pipeline, "Input", Resources.UseMaterial("::Terrain/GrassyMountains", HostScript.ModDomain), Renderer.Opaque.Mesh);
			MatGround.AddMeshMaterial(pipeline, "PineSpawns", Resources.UseMaterial("PineImpostor", HostScript.ModDomain), Renderer.Opaque.Mesh);

			// For terrain LoDs 0-4, spawn "real" tree models instead of the impostors.
			pipeline = MatGround.AddPipeline(Resources.UseGeometryPipeline("PineField", HostScript.ModDomain), "Input", 0, 4);
			MatGround.AddDefaultShadowAndZPre(pipeline, "Input");
			MatGround.AddMeshMaterial(pipeline, "Input", Resources.UseMaterial("::Terrain/GrassyMountains", HostScript.ModDomain), Renderer.Opaque.Mesh);
			MatGround.AddMeshMaterial(pipeline, "PineSpawns", Resources.UseMaterial("PineLeaves", HostScript.ModDomain), Renderer.Opaque.Mesh);

			return base.init();
		}

		/// <summary>
		/// Creates the CSG node network for the terrain generation.
		/// </returns>
		public override CsgNode createTerrain()
		{
			// Load and return a CsgNode based on the "Hills" expression resource. This will create some generic perlin-based hills.
			CsgOpUnion union = new CsgOpUnion();
			ExpressionResource expression = Resources.UseExpression("Hills", HostScript.ModDomain);
			union.AddNode(new CsgExpression(MatGround.MaterialIndex, expression));
			return union;
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
			Universe.CreateWorld("UpvoidMinerWorld", new UpvoidMinerWorldGenerator());
		}
	}
}
