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

        public ItemEntity(Item representedItem)
        {
            RepresentedItem = representedItem;
        }

        /// <summary>
        /// Adds a physics component to this entity.
        /// </summary>
        public void AddPhysicsComponent(PhysicsComponent comp)
        {
            physicsComponents.Add(comp);
        }
        /// <summary>
        /// Adds a render component to this entity.
        /// </summary>
        public void AddRenderComponent(RenderComponent comp)
        {
            renderComponents.Add(comp);
        }

        protected override void Init()
        {
            RepresentedItem.SetupItemEntity(this, thisEntity);

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

            // For now, simply hide this entity (entity deletion is not yet possible)
            foreach (RenderComponent comp in renderComponents)
                comp.Visible = false;
            foreach (PhysicsComponent comp in physicsComponents)
                comp.Transform = mat4.Scale(0f);
        }
    }
}

