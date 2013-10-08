using System;
using Engine.Rendering;
using Engine.Resources;

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
            
            // Add Gras attribute for LoD 4 (= MinLoD).
            Material.AddAttributeFloat("aGras", 0, 0, 4);

            // Color modulated geometry pipeline for more variance.
            {
                int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("ColoredTerrain", HostScript.ModDomain), "Input", "Input");
                Material.AddDefaultShadowAndZPre(pipeline);
                Material.AddMeshMaterial(pipeline, "Output", RenderMaterial, Renderer.Opaque.Mesh);
            }
            
            // Spawn Grass
            {
                int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("GrassField", HostScript.ModDomain), "Input", "", 0, 4);
                Material.AddMeshMaterial(pipeline, "Spawns", Resources.UseMaterial("Grass01", HostScript.ModDomain), Renderer.Opaque.Mesh);
            }
            /*
            // Spawn Grass
            {
                int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("GrassField2", HostScript.ModDomain), "Input", "", 0, 4);
                Material.AddMeshMaterial(pipeline, "Spawns", Resources.UseMaterial("Grass01", HostScript.ModDomain), Renderer.Opaque.Mesh);
            }
            
            // Spawn Herbs
            {
                int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("HerbField", HostScript.ModDomain), "Input", "", 0, 4);
                Material.AddMeshMaterial(pipeline, "Spawns", Resources.UseMaterial("Herbs17", HostScript.ModDomain), Renderer.Opaque.Mesh);
            }
            
            // Spawn Herbs
            {
                int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("HerbField2", HostScript.ModDomain), "Input", "", 0, 4);
                Material.AddMeshMaterial(pipeline, "Spawns", Resources.UseMaterial("Herbs18", HostScript.ModDomain), Renderer.Opaque.Mesh);
            }*/
            
            // Add the geometry for the terrain LoDs 5-8. Add some tree impostors to make the ground look nicer.
            {
                int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("PineImpostorField", HostScript.ModDomain), "Input", "", 5, 8);
                Material.AddMeshMaterial(pipeline, "PineSpawns", Resources.UseMaterial("PineImpostor", HostScript.ModDomain), Renderer.Opaque.Mesh);
            }

            // For terrain LoDs 0-4, spawn "real" tree models instead of the impostors.
            {
                int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("PineField", HostScript.ModDomain), "Input", "", 0, 4);
                Material.AddMeshMaterial(pipeline, "PineSpawns", Resources.UseMaterial("PineLeaves", HostScript.ModDomain), Renderer.Opaque.Mesh);
            }
        }
    }
}

