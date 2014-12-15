using System;
using System.Diagnostics;
using System.Collections.Generic;
using Engine;
using Engine.Physics;
using Engine.Rendering;
using Engine.Resources;
using Engine.Universe;
using UpvoidMiner.UI;

namespace UpvoidMiner
{
    /// <summary>
    /// An item that is a tool
    /// </summary>
    public class TorchItem : DiscreteItem
    {
        private static Random torchRandom = new Random();

        public TorchItem(int stackSize = 1) :
            base("Torch", "A torch that gives light.", 1.0f, ItemCategory.Tools, stackSize)
        {
            Icon = "Torch";
        }

        /// <summary>
        /// This can be merged with material items of the same resource and shape and size.
        /// </summary>
        public override bool TryMerge(Item rhs, bool subtract, bool force, bool dryrun = false)
        {
            TorchItem item = rhs as TorchItem;
            if (item == null)
                return false;

            return Merge(item, subtract, force, dryrun);
        }

        /// <summary>
        /// Creates a copy of this item.
        /// </summary>
        public override Item Clone()
        {
            return new TorchItem(StackSize);
        }

        /// <summary>
        /// Creates a copy of this item with the given substance.
        /// </summary>
        public override Item Clone(Substance sub)
        {
            return new TorchItem(StackSize);
        }

        /// <summary>
        /// Renderjobs and -components for the preview sphere
        /// </summary>
        private MeshRenderJob torchLightRenderJob;

        private vec3 torchLightPosition;
        private float lightRadiusInitial = 2.0f;
        private float lightRadiusDeviation = 0.5f;

        public float LightRadiusInitial { get { return lightRadiusInitial; } }
        public float LightRadiusDeviation { get { return lightRadiusDeviation; } }


        public override void OnSelect(Player player)
        {
            // no preview 
            return;
        }

        public override void OnDeselect(Player player)
        {
            return;
        }

        public override void OnUseParameterChange(Player player, float _delta)
        {
            return;
        }

        /// <summary>
        /// Some items have a preview for their impact when used, others do not
        /// </summary>
        public override bool HasRayPreview
        {
            get
            {
                return false;
            }
        }


        public override void OnRayPreview(Player _player, RayHit rayHit, CrosshairInfo crosshair)
        {
            var _visible = rayHit != null;
            var _worldPos = rayHit == null ? vec3.Zero : rayHit.Position + rayHit.Normal.Normalized * (0.01f / 7f) /* small security offset */;
            var _worldNormal = rayHit == null ? vec3.UnitY : rayHit.Normal;

            var savPos = _worldPos;

            crosshair.Disabled = rayHit == null;

        }

        public override void OnUse(Player player, vec3 _worldPos, vec3 _worldNormal, Entity _hitEntity)
        {
            bool success = TryMerge(new TorchItem(), true, false);
            if (!success)
                return;

            vec3 normal = _worldNormal;
            float transY = -0.5f + 0.4f * normal.y;
            vec3 pos = _worldPos + _worldNormal * 0.3f;

            torchLightPosition = pos;

            player.ContainingWorld.AddRenderJob(new MeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("::Torch", UpvoidMiner.ModDomain),
                Resources.UseMesh("::Assets/Torch", UpvoidMiner.ModDomain),
                mat4.Translate(_worldPos + new vec3(0, transY, 0)) * mat4.Scale(1f)));

            player.ContainingWorld.AddRenderJob(torchLightRenderJob = new MeshRenderJob(
                Renderer.Lights.Mesh,
                Resources.UseMaterial("::Light", UpvoidMiner.ModDomain),
                Resources.UseMesh("::Debug/Sphere", UpvoidMiner.ModDomain),
                mat4.Translate(torchLightPosition) * mat4.Scale(LightRadiusInitial)));

            MeshRenderJob torchFire;
            player.ContainingWorld.AddRenderJob(torchFire = new MeshRenderJob(
                Renderer.Additive.Mesh,
                Resources.UseMaterial("TorchFire", UpvoidMiner.ModDomain),
                Resources.UseMesh("TorchFire", UpvoidMiner.ModDomain),
                mat4.Translate(_worldPos + new vec3(0, transY + 0.7f, 0))));

            torchFire.SetParameter("uRandom", (float)torchRandom.NextDouble());

            player.Torches.Add(this);
        }

        public void Update(double _elapsedSeconds)
        {
            Debug.Assert(torchLightRenderJob != null);

            torchLightRenderJob.ModelMatrix = mat4.Translate(torchLightPosition) * mat4.Scale(LightRadiusInitial + (float)Math.Sin(_elapsedSeconds) * LightRadiusDeviation);
        }
    }
}

