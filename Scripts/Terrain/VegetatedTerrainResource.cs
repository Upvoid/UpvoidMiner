using System;
using Engine.Rendering;
using Engine.Resources;
using Engine.Scripting;

namespace UpvoidMiner
{
    /// <summary>
    /// A solid terrain resource that is vegetated by trees/gras/flowers.
    /// </summary>
    public class VegetatedTerrainResource : SolidTerrainResource
    {
        public VegetatedTerrainResource(string name, string renderMaterial, string particleMaterial) :
            base(name, renderMaterial, particleMaterial, false)
        {
            // For now: just use the setup from our default dirt:
            
            if (Scripting.IsHost)
            {
                // Add Gras attribute for LoD 4 (= MinLoD).
                Material.AddAttributeFloat("aGrass", 0, 0, 4);

                // Lod 0-4
                // Color modulated geometry pipeline for more variance.
                {
                    int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("ColoredTerrain", UpvoidMiner.ModDomain), "Input", "Decimate", 0, 4);
                    Material.AddDefaultShadowAndZPre(pipeline);
                    Material.AddMeshMaterial(pipeline, "Output", RenderMaterial, Renderer.Opaque.Mesh);
                }

                // Lod 5-max
                // Color modulated geometry pipeline for more variance.
                {
                    int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("ColoredTerrainLow", UpvoidMiner.ModDomain), "Input", "Decimate", 5);
                    Material.AddDefaultShadowAndZPre(pipeline);
                    Material.AddMeshMaterial(pipeline, "Output", RenderMaterial, Renderer.Opaque.Mesh);
                }

            
                // Spawn Grass
                {
                    int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("GrassField", UpvoidMiner.ModDomain), "Input", "", 0, 4);
                    Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("Grass01", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
                    Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("Grass01.Transparent", UpvoidMiner.ModDomain), Renderer.Transparent.Mesh);
                }
                /*
            // Spawn Grass
            {
                int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("GrassField2", UpvoidMiner.ModDomain), "Input", "", 0, 4);
                Material.AddMeshMaterial(pipeline, "Spawns", Resources.UseMaterial("Grass01", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
            }
            
            // Spawn Herbs
            {
                int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("HerbField", UpvoidMiner.ModDomain), "Input", "", 0, 4);
                Material.AddMeshMaterial(pipeline, "Spawns", Resources.UseMaterial("Herbs17", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
            }
            
            // Spawn Herbs
            {
                int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("HerbField2", UpvoidMiner.ModDomain), "Input", "", 0, 4);
                Material.AddMeshMaterial(pipeline, "Spawns", Resources.UseMaterial("Herbs18", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
            }*/
            
                // Add the geometry for the terrain LoDs 5-8. Add some tree impostors to make the ground look nicer.
                {
                    //int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("PineImpostorField", UpvoidMiner.ModDomain), "Input", "", 5, 8);
                    //Material.AddMeshMaterial(pipeline, "PineSpawns", Resources.UseMaterial("PineImpostor", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
                }

                // For terrain LoDs 0-4, spawn "real" tree models instead of the impostors.
                {
                    //int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("PineField", UpvoidMiner.ModDomain), "Input", "", 0, 4);
                    //Material.AddMeshMaterial(pipeline, "PineSpawns", Resources.UseMaterial("PineLeaves", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
                }
            }
        }
    }
}

