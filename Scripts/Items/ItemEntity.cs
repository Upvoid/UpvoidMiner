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
using Engine;
using Engine.Rendering;
using Engine.Resources;
using Engine.Universe;
using Engine.Physics;
using System.Collections.Generic;
using UpvoidMiner.Items;
using UpvoidMiner.UI;

namespace UpvoidMiner
{
    /// <summary>
    /// An entity representing an item lying around in the world.
    /// It "presents" the contained item graphically and physically. Responds to Interaction triggers with an AddItem trigger (so interaction means picking up the item).
    /// </summary>
    public class ItemEntity : EntityScript
    {
        public Item RepresentedItem;

        protected List<PhysicsComponent> physicsComponents = new List<PhysicsComponent>();
        protected List<RenderComponent> renderComponents = new List<RenderComponent>();

        public float Mass { get; private set; }

        TriggerId AddItemTrigger;

        public bool FixedPosition { get; protected set; }

        public ItemEntity(Item representedItem, bool fixedPosition)
        {
            RepresentedItem = representedItem;
            FixedPosition = fixedPosition;
        }

        /// <summary>
        /// Applies an impulse to all physics components.
        /// </summary>
        public void ApplyImpulse(vec3 impulse, vec3 relPos)
        {
            foreach (var pc in physicsComponents)
            {
                pc.RigidBody.ApplyImpulse(impulse, relPos);
            }
        }

        /// <summary>
        /// Adds a physics component to this entity.
        /// </summary>
        public void AddPhysicsComponent(PhysicsComponent comp)
        {
            physicsComponents.Add(comp);
            thisEntity.AddComponent(comp);
            Mass += comp.RigidBody.Mass;
        }
        /// <summary>
        /// Adds a render component to this entity.
        /// </summary>
        public void AddRenderComponent(RenderComponent comp)
        {
            renderComponents.Add(comp);
            thisEntity.AddComponent(comp);
        }

        protected override void Init()
        {
            RepresentedItem.SetupItemEntity(this, thisEntity, FixedPosition);

            // Set up the triggers.
            AddItemTrigger = TriggerId.getIdByName("AddItem");
            AddTriggerSlot("Interaction");
        }

        public void Interaction(object msg)
        {
            // Make sure we get the message type we are expecting.
            InteractionMessage interactionMsg = msg as InteractionMessage;
            if (interactionMsg == null)
                return;

            // Interacting with an item means picking it up. Answer by sending the item to the sender.
            interactionMsg.Sender[AddItemTrigger] |= new AddItemMessage(RepresentedItem);

            // And remove this entity.
            ItemManager.RemoveItemFromWorld(this);

            // Tutorial
            Tutorials.MsgBasicCraftingCollect.Report(1);
            if (RepresentedItem is MaterialItem && (RepresentedItem as MaterialItem).Substance is WoodSubstance)
                Tutorials.MsgBasicChoppingCollect.Report(1);
        }

        /// <summary>
        /// Last known physics pos
        /// </summary>
        private vec3 lastPhysicsPos = vec3.Zero;

        /// <summary>
        /// Updates physics state by teleporting item upwards if below ground
        /// </summary>
        public void UpdatePhysics()
        {
            if (FixedPosition)
                return; // no update on fixed

            foreach (var pc in physicsComponents)
            {
                var body = pc.RigidBody;

                // Security: if in non-air chunk, teleport to next all-air one
                mat4 transformation = body.GetTransformation();
                vec3 pos = new vec3(transformation.col3);
                if (vec3.distance(lastPhysicsPos, pos) < 0.01)
                    return; // early abort

                lastPhysicsPos = pos;
                WorldTreeNode node = ContainingWorld.QueryWorldTreeNode(pos);
                if (node != null && node.IsMinLod)
                {
                    HermiteData volumeData = node.CurrentVolume;
                    if (volumeData != null)
                    {
                        if (!volumeData.HasAir)
                        {
                            // we are definitely in a non-air chunk here
                            // teleport one node size above
                            body.SetTransformation(mat4.Translate(new vec3(0, node.Size, 0)) * body.GetTransformation());
                        }
                        else if (!volumeData.HasAirAt(pos))
                        {
                            // we are in a mixed chunk, advance pos until air
                            float offset = 0f;
                            do
                            {
                                pos.y += 0.5f;
                                offset += 0.5f;
                            } while (!volumeData.HasAirAt(pos) || !volumeData.HasAirAt(pos + new vec3(0, 1.5f, 0)));

                            // another 1.5m to ensure good ground
                            offset += 1.5f;

                            body.SetTransformation(mat4.Translate(new vec3(0, offset, 0)) * body.GetTransformation());
                        }
                    }
                }

                break; // only one RB supported
            }
        }
    }
}

