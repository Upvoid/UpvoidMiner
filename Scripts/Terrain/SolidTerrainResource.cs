using System;
using Engine.Universe;
using Engine.Rendering;
using Engine.Resources;

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

        public SolidTerrainResource(string name, string renderMaterial, string particleMaterial, bool defaultPipeline = true) :
            base(name)
        {
            RenderMaterial = Resources.UseMaterial(renderMaterial, HostScript.ModDomain);
            DigParticleMaterial = Resources.UseMaterial(particleMaterial, HostScript.ModDomain);

            // Add a default pipeline. (Solid material with zPre and Shadow pass)
            if ( defaultPipeline )
            {
                int pipe = Material.AddDefaultPipeline();
                Material.AddDefaultShadowAndZPre(pipe);
                Material.AddMeshMaterial(pipe, "Output", RenderMaterial, Renderer.Opaque.Mesh);
            }
        }
    }
}

