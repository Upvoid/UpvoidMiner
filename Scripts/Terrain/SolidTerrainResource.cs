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

        public SolidTerrainResource(string name, string renderMaterial) :
            base(name)
        {
            RenderMaterial = Resources.UseMaterial(renderMaterial, HostScript.ModDomain);

            int pipe = Material.AddDefaultPipeline();
            Material.AddDefaultShadowAndZPre(pipe);
            Material.AddMeshMaterial(pipe, "Output", RenderMaterial, Renderer.Opaque.Mesh);
        }
    }
}

