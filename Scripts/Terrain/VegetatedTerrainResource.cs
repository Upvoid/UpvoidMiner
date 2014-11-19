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
        public int GrassPipelineIndex;
        public int GrassMaterialIndex;

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
                    int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("ColoredTerrain", UpvoidMiner.ModDomain), "Input", "Input", 0, 4);
                    Material.AddDefaultShadowAndZPre(pipeline);
                    Material.AddMeshMaterial(pipeline, "Output", Resources.UseMaterial("Terrain/DirtVegetated", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
                }

                // Lod 5-max
                // Color modulated geometry pipeline for more variance.
                {
                    int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("ColoredTerrainLow", UpvoidMiner.ModDomain), "Input", "Input", 5);
                    Material.AddDefaultShadowAndZPre(pipeline);
                    Material.AddMeshMaterial(pipeline, "Output", RenderMaterial, Renderer.Opaque.Mesh);
                }


                // Spawn Grass
                {
                    int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("GrassField", UpvoidMiner.ModDomain), "Input", "", 0, 4);
                    //Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("SimpleGrass", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
                    //Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("SimpleGrass.Transparent", UpvoidMiner.ModDomain), Renderer.Transparent.Mesh);
                    //Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("SimpleGrass.ShadowDecal", UpvoidMiner.ModDomain), Renderer.Transparent.Mesh);
                    //Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("SimpleGrass.zPre", UpvoidMiner.ModDomain), Renderer.zPre.Mesh);

                    var materialIdx = Material.AddMeshMaterial(pipeline, "GrassBlades", Resources.UseMaterial("Vegetation/GrassBlades", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);

                    GrassPipelineIndex = pipeline;
                    GrassMaterialIndex = materialIdx;

                    Material.SetPipelineActive(pipeline, Settings.settings.Grass);
                }

                // Spawn Flowers
                {
                    //int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("Flowers", UpvoidMiner.ModDomain), "Input", "", 0, 4);
                    //Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("Flower01.ShadowDecal", UpvoidMiner.ModDomain), Renderer.Transparent.Mesh);
                    //Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("Flower01", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
                }

                // Spawn more Flowers
                {
                    int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("Flowers2", UpvoidMiner.ModDomain), "Input", "", 0, 4);
                    //Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("Flower02.ShadowDecal", UpvoidMiner.ModDomain), Renderer.Transparent.Mesh);
                    Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("Flower02", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
                }

                // Spawn fern
                {
                    int pipeline = Material.AddPipeline(Resources.UseGeometryPipeline("Fern", UpvoidMiner.ModDomain), "Input", "", 0, 4);
                    Material.AddMeshMaterial(pipeline, "ColoredSpawns", Resources.UseMaterial("Fern", UpvoidMiner.ModDomain), Renderer.Opaque.Mesh);
                }
            }
        }
    }
}

