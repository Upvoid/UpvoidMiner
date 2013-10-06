using System;
using Engine.Universe;
using Engine.Rendering;
using Engine.Resources;
using Engine;

namespace UpvoidMiner
{
    /// <summary>
    /// An item that represents a resource based on a terrain material
    /// </summary>
    public class ResourceItem : VolumeItem
    {
        /// <summary>
        /// Renderjob for the preview sphere
        /// </summary>
        private MeshRenderJob previewSphere;
        /// <summary>
        /// Radius of terrain material that is placed if "use"d.
        /// </summary>
        private float useRadius = 1f;

        /// <summary>
        /// The terrain material that this resource represents.
        /// </summary>
        public readonly TerrainMaterial Material;
        
        public ResourceItem(TerrainMaterial material, float volume = 0f) :
            base(material.Name, "The terrain resource " + material.Name, 1.0f, ItemCategory.Resources, volume)
        {
            Material = material;
        }

        /// <summary>
        /// This can be merged with resource items of the same resource
        /// </summary>
        public override bool TryMerge(Item rhs, bool subtract, bool force, bool dryrun = false)
        {
            ResourceItem item = rhs as ResourceItem;
            if ( item == null ) return false;
            if ( item.Material.MaterialIndex != Material.MaterialIndex ) return false;

            return Merge(item, subtract, force, dryrun);
        }

        /// <summary>
        /// Creates a copy of this item.
        /// </summary>
        public override Item Clone()
        {
            return new ResourceItem(Material, Volume);
        }

        /// <summary>
        /// Yes, we have a preview for resources.
        /// </summary>
        public override bool HasPreview { get { return true; } }

        public override void OnSelect()
        {
            previewSphere = new MeshRenderJob(Renderer.Transparent.Mesh, Resources.UseMaterial("Items/ResourcePreview", LocalScript.ModDomain), Resources.UseMesh("::Debug/Sphere", null), mat4.Scale(0f));
            //previewSphere = new MeshRenderJob(Renderer.Opaque.Mesh, Resources.UseMaterial("::Lime", LocalScript.ModDomain), Resources.UseMesh("::Debug/Sphere", null), mat4.Scale(0f));
            LocalScript.world.AddRenderJob(previewSphere);
            Console.Write("Select " + Material.Name);
        }

        public override void OnPreview(vec3 _worldPos, bool _visible)
        {
            previewSphere.ModelMatrix = _visible ? mat4.Translate(_worldPos) * mat4.Scale(useRadius) : mat4.Scale(0f);
        }

        public override void OnDeselect()
        {
            Console.Write("DeSelect " + Material.Name);
            LocalScript.world.RemoveRenderJob(previewSphere);
            previewSphere = null;
        }
    }
}

