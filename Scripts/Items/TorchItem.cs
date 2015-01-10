using System;
using System.Diagnostics;
using System.Collections.Generic;
using Engine;
using Engine.Audio;
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
        // We only want three fire sounds (max.) for all the torches (instead of spamming the audio engine with multiple fire sounds)
        private static SoundResource fireSoundRes = null;
        private static Sound[] fireSound = null;

        const float TorchFireVolume = 1.0f;

        public static void UpdateTorchSound(List<vec4> positions)
        {
            if(fireSound == null)
            {
                // Sound not set up yet, nothing to do here, just return silently.
                return;
            }

            if(positions.Count > 3)
            {
                throw new InvalidOperationException("Too many torch positions (" + positions.Count + " total, but only 3 are allowed!). This must not happen.");
            }

            for (int i = 0; i < 3; ++i)
            {
                if(i >= positions.Count)
                {
                    // "Invalidate" sound
                    fireSound[i].Volume = 0.0f;

                    // Go on with next sound
                    continue;
                }

                vec4 curr = positions[i];
                float disToCam = curr.w;

                if(disToCam < 20.0f)
                {
                    fireSound[i].Position = new vec3(curr);
                    fireSound[i].Volume = TorchFireVolume;
                }
                else
                {
                    fireSound[i].Volume = 0.0f;
                }
            }
        }

        private static Random torchRandom = new Random();

        public TorchItem(int stackSize = 1) :
            base("Torch", "A torch that gives light.", 1.0f, ItemCategory.Tools, stackSize)
        {
            Icon = "Torch";

            // Setup the audio stuff, if not done already
            if(fireSoundRes == null)
            {
                fireSoundRes = Resources.UseSound("Mods/Upvoid/Resources.SFX/1.0.0::Miscellaneous/Fire", UpvoidMiner.ModDomain);

                // 3 sounds (max.)
                fireSound = new Sound[3];

                for (int i = 0; i < 3; ++i)
                {
                    // Start with zero volume, we adapt that later
                    fireSound[i] = new Sound(fireSoundRes, vec3.Zero, true, 1.0f, 0.6f, (int)AudioType.SFX, true); // Pitch: 0.6
                    fireSound[i].ReferenceDistance = 2.0f;

                    // Play it all the time. Volume is zero if no torch exists.
                    fireSound[i].Play();
                }
            }
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

            if (!LocalScript.player.GodMode) // Don't remove in godmode
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

            RenderComponent rcTorchHandle;
            RenderComponent rcTorchShadow;
            RenderComponent rcTorchFire;
            RenderComponent rcTorchLight;

            // Torch handle - opaque
            itemEntity.AddRenderComponent(rcTorchHandle = new RenderComponent(new MeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("::Torch", UpvoidMiner.ModDomain),
                Resources.UseMesh("::Assets/Torch", UpvoidMiner.ModDomain),
                mat4.Identity), mat4.Translate(-offsetHandleAndFire) * mat4.Scale(1f)));

            // Torch handle - shadow
            itemEntity.AddRenderComponent(rcTorchShadow = new RenderComponent(new MeshRenderJob(
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
            itemEntity.AddRenderComponent(rcTorchFire = new RenderComponent(torchFire = new MeshRenderJob(
                Renderer.Additive.Mesh,
                Resources.UseMaterial("TorchFire", UpvoidMiner.ModDomain),
                Resources.UseMesh("TorchFire", UpvoidMiner.ModDomain),
                mat4.Identity), mat4.Identity));

            float randomValue = (float)torchRandom.NextDouble();
            torchFire.SetParameter("uRandom", randomValue);

            // Torch light
            rcTorchLight = new RenderComponent(new MeshRenderJob(
                Renderer.Lights.Mesh,
                Resources.UseMaterial("::Light", UpvoidMiner.ModDomain),
                Resources.UseMesh("::Debug/Sphere", UpvoidMiner.ModDomain),
                mat4.Identity), mat4.Translate(offsetFireAndLight) * mat4.Scale(1.0f));
            itemEntity.AddRenderComponent(rcTorchLight);

            // This is a light. Register it (for flickering etc)
            itemEntity.RegisterLightRenderComponent(rcTorchLight, 6.0f + 1.0f * randomValue, 0.3f + 0.1f * randomValue, 5.5f + randomValue * 5.0f);

            // Fade out torches too far away
            float fadeOutMin = 100;
            float fadeOutMax = 105;
            float fadeTime = 1.0f; // 1 second

            rcTorchHandle.ConfigureLod(fadeOutMin, fadeOutMax, fadeTime);
            rcTorchShadow.ConfigureLod(fadeOutMin, fadeOutMax, fadeTime);
            rcTorchFire.ConfigureLod(fadeOutMin, fadeOutMax, fadeTime);
            rcTorchLight.ConfigureLod(fadeOutMin, fadeOutMax, fadeTime);
        }
    }
}

