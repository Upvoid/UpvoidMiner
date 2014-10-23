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
            if(interactionMsg == null)
                return;

            // Interacting with an item means picking it up. Answer by sending the item to the sender.
            interactionMsg.Sender[AddItemTrigger] |= new AddItemMessage(RepresentedItem);

            // And remove this entity.
            ContainingWorld.RemoveEntity(thisEntity);
        }
    }
}

