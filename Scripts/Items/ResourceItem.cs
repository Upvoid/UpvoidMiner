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
        private MeshRenderJob previewSphereLimited;
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

        public override void OnUse(Player player, vec3 _worldPos)
        {
            float radius = useRadius;
            float useVolume = 4f / 3f * (float)Math.PI * useRadius * useRadius * useRadius;
            if ( useVolume > Volume )
                radius = (float)Math.Pow(Volume / (4f / 3f * (float)Math.PI), 1 / 3f);

            player.PlaceSphere(Material, _worldPos, radius);
        }

        public override void OnSelect()
        {
            // Create a transparent sphere as 'fill-indicator'.
            previewSphere = new MeshRenderJob(Renderer.Transparent.Mesh, Resources.UseMaterial("Items/ResourcePreview", LocalScript.ModDomain), Resources.UseMesh("::Debug/Sphere", null), mat4.Scale(0f));
            LocalScript.world.AddRenderJob(previewSphere);
            // And a second one in case we are limited by the volume at hand.
            previewSphereLimited = new MeshRenderJob(Renderer.Transparent.Mesh, Resources.UseMaterial("Items/ResourcePreviewLimited", LocalScript.ModDomain), Resources.UseMesh("::Debug/Sphere", null), mat4.Scale(0f));
            LocalScript.world.AddRenderJob(previewSphereLimited);
        }

        public override void OnUseParameterChange(float _delta) 
        {
            // Adjust use-radius between 0.5m and 5m radius
            useRadius = Math.Max(0.5f, Math.Min(5f, useRadius + _delta / 5f));
        }

        public override void OnPreview(vec3 _worldPos, bool _visible)
        {
            // If the indicated volume is greater than the available volume, show limitation sphere.
            float useVolume = 4f / 3f * (float)Math.PI * useRadius * useRadius * useRadius;
            if ( _visible && useVolume > Volume )
            {
                float availableRadius = (float)Math.Pow(Volume / (4f / 3f * (float)Math.PI), 1 / 3f);
                previewSphereLimited.ModelMatrix = mat4.Translate(_worldPos) * mat4.Scale(availableRadius);
            }
            else previewSphereLimited.ModelMatrix = mat4.Scale(0f);

            // Radius of the primary preview is always use-radius.
            previewSphere.ModelMatrix = _visible ? mat4.Translate(_worldPos) * mat4.Scale(useRadius) : mat4.Scale(0f);
        }

        public override void OnDeselect()
        {
            // Remove and delete it on deselect.
            LocalScript.world.RemoveRenderJob(previewSphere);
            LocalScript.world.RemoveRenderJob(previewSphereLimited);
            previewSphere = null;
        }
    }
}

