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
using Engine.Universe;
using Engine.Rendering;
using Engine.Resources;
using Engine.Scripting;

namespace UpvoidMiner
{
    /// <summary>
    /// A simple solid terrain resource without decorations, e.g. stone or wood.
    /// </summary>
    public class SolidTerrainResource : TerrainResource
    {
        /// <summary>
        /// Material used for rendering the solid terrain.
        /// </summary>
        public readonly MaterialResource RenderMaterial;
        /// <summary>
        /// Material used for particle effect when digging.
        /// </summary>
        public readonly MaterialResource DigParticleMaterial;

        public SolidTerrainResource(string name, string renderMaterial, string particleMaterial, float massDensity, bool defaultPipeline = true) :
            base(name, massDensity)
        {
            RenderMaterial = Resources.UseMaterial(renderMaterial, UpvoidMiner.ModDomain);
            DigParticleMaterial = Resources.UseMaterial(particleMaterial, UpvoidMiner.ModDomain);

            if (Scripting.IsHost)
            {
                // Add a default pipeline. (Solid material with zPre and Shadow pass)
                if (defaultPipeline)
                {
                    { // LoD 0-4
                        int pipe = Material.AddPipeline(Resources.UseGeometryPipeline("ColoredRock", UpvoidMiner.ModDomain), "Input", "Input", 0, 4);
                        Material.AddDefaultShadowAndZPre(pipe);
                        Material.AddMeshMaterial(pipe, "Output", RenderMaterial, Renderer.Opaque.Mesh);
                    }

                    { // LoD 5-max
                        int pipe = Material.AddPipeline(Resources.UseGeometryPipeline("ColoredRockLow", UpvoidMiner.ModDomain), "Input", "Input", 5);
                        Material.AddDefaultShadowAndZPre(pipe);
                        Material.AddMeshMaterial(pipe, "Output", RenderMaterial, Renderer.Opaque.Mesh);
                    }
                }
            }
        }
    }
}

