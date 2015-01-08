using System;
using System.Diagnostics;
using System.Collections.Generic;
using Engine;
using Engine.Physics;
using Engine.Rendering;
using Engine.Resources;
using Engine.Universe;
using UpvoidMiner.UI;
using UpvoidMiner.Items;

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
            // No torches on overhangs, ceiling, etc.
            if (_worldNormal.y < -0.2f)
                return;

            LocalScript.player.Inventory.Items.RemoveItem(new TorchItem(), true);

            mat4 transformation = mat4.Translate(_worldPos + new vec3(0, -0.6f + _worldNormal.y * 0.7f, 0) + 0.3f * _worldNormal);

            // Orient torch by "mount point" normal when terrain is too steep
            if(_worldNormal.y < 0.5f)
            {
                vec3 biTangent = _worldNormal;
                vec3 tangent = -vec3.cross(biTangent, new vec3(0, 1, 0)).Normalized;
                vec3 newNormal = vec3.cross(_worldNormal, tangent).Normalized;

                transformation *= new mat4(tangent, newNormal, biTangent, vec3.Zero);
            }

            

            ItemManager.InstantiateItem(new TorchItem(), transformation, true);
        }



        /// <summary>
        /// Is called when a torch is placed
        /// </summary>
        public override void SetupItemEntity(ItemEntity itemEntity, Entity entity, bool fixedPosition = true)
        {
            // Create the physical representation of the item.
            RigidBody body = new RigidBody(
                fixedPosition ? 0 : 1.0f,
                entity.Transform * mat4.Translate(new vec3(0, 0.0f, 0)),
                new CylinderShape(0.2f, 0.4f)
                );

            if(!fixedPosition)
            {
                body.SetRestitution(0.5f);
                body.SetFriction(1f);
                body.SetDamping(0.2f, 0.4f);
            }
            
            itemEntity.ContainingWorld.Physics.AddRigidBody(body);
            itemEntity.AddPhysicsComponent(new PhysicsComponent(body, mat4.Identity));


            vec3 offsetHandleAndFire = new vec3(0, 0.5f, 0);
            vec3 offsetFireAndLight = new vec3(0, 0.4f, 0);

            // Torch handle - opaque
            itemEntity.AddRenderComponent(new RenderComponent(new MeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("::Torch", UpvoidMiner.ModDomain),
                Resources.UseMesh("::Assets/Torch", UpvoidMiner.ModDomain),
                mat4.Identity), mat4.Translate(-offsetHandleAndFire) * mat4.Scale(1f)));

            // Torch handle - shadow
            itemEntity.AddRenderComponent(new RenderComponent(new MeshRenderJob(
                Renderer.Shadow.Mesh,
                Resources.UseMaterial("::Shadow", UpvoidMiner.ModDomain),
                Resources.UseMesh("::Assets/Torch", UpvoidMiner.ModDomain),
                mat4.Identity), mat4.Translate(-offsetHandleAndFire) * mat4.Scale(1f)));

            // This is unfinished work. TODO(ks): FIXME
            /*const bool torchMount = false;
            if (torchMount)
            {
                mat4 mountTrans = mat4.RotateX(70.0);
                // Mount
                itemEntity.AddRenderComponent(new RenderComponent(new MeshRenderJob(
                    Renderer.Opaque.Mesh,
                    Resources.UseMaterial("Terrain/Wood", UpvoidMiner.ModDomain),
                    Resources.UseMesh("::Debug/Cylinder", UpvoidMiner.ModDomain),
                    mat4.Identity), mat4.Translate(-0.25f * new vec3(entity.Transform.col2).Normalized) * mountTrans * mat4.Scale(new vec3(.05f, .2f, .05f))));
            }*/
            

            // Torch fire - additive transparent
            MeshRenderJob torchFire;
            itemEntity.AddRenderComponent(new RenderComponent(torchFire = new MeshRenderJob(
                Renderer.Additive.Mesh,
                Resources.UseMaterial("TorchFire", UpvoidMiner.ModDomain),
                Resources.UseMesh("TorchFire", UpvoidMiner.ModDomain),
                mat4.Identity), mat4.Identity));

            float randomValue = (float)torchRandom.NextDouble();
            torchFire.SetParameter("uRandom", randomValue);

            // Torch light
            RenderComponent lightComp = new RenderComponent(new MeshRenderJob(
                Renderer.Lights.Mesh,
                Resources.UseMaterial("::Light", UpvoidMiner.ModDomain),
                Resources.UseMesh("::Debug/Sphere", UpvoidMiner.ModDomain),
                mat4.Identity), mat4.Translate(offsetFireAndLight) * mat4.Scale(1.0f));
            itemEntity.AddRenderComponent(lightComp);

            // This is a light. Register it (for flickering etc)
            itemEntity.RegisterLightRenderComponent(lightComp, 6.0f + 1.0f * randomValue, 0.3f + 0.1f * randomValue, 5.5f + randomValue * 5.0f);

            
            /*
            MaterialResource material;
            var mat = Substance.QueryResource();
            if (mat is SolidTerrainResource)
                material = (mat as SolidTerrainResource).RenderMaterial;
            else
                throw new NotImplementedException("Unknown terrain resource");

            // Create the graphical representation of the item.
            MeshRenderJob renderJob = new MeshRenderJob(
                Renderer.Opaque.Mesh,
                material,
                mesh,
                mat4.Identity
                );
            itemEntity.AddRenderComponent(new RenderComponent(renderJob, scaling, true));

            MeshRenderJob renderJobShadow = new MeshRenderJob(
                Renderer.Shadow.Mesh,
                Resources.UseMaterial("::Shadow", UpvoidMiner.ModDomain),
                mesh,
                mat4.Identity
                );
            itemEntity.AddRenderComponent(new RenderComponent(renderJobShadow, scaling, true));
            */
        }
    }
}

