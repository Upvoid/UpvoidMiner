using System;
using Engine;
using Engine.Rendering;
using Engine.Resources;
using Engine.Universe;
using Engine.Physics;

namespace UpvoidMiner
{
    /// <summary>
    /// An entity representing an item lying around in the world.
    /// It "presents" the contained item graphically and physically. Responds to Interaction triggers with an AddItem trigger (so interaction means picking up the item).
    /// </summary>
    public class ItemEntity: EntityScript
    {
        public Item PresentedItem;

        protected PhysicsComponent physicsComponent;
        protected RenderComponent renderComponent;
        protected RenderComponent renderComponentShadow;

        TriggerId AddItemTrigger;

        public ItemEntity(Item presentedItem)
        {
            PresentedItem = presentedItem;
        }

        protected override void Init()
        {
            // Create the physical representation of the item.
            RigidBody body = ContainingWorld.Physics.CreateAndAddRigidBody(
                50f,
                mat4.Translate(thisEntity.Position),
                new SphereShape(PresentedItem.EntityRadius)
            );

            physicsComponent = new PhysicsComponent(thisEntity, body, mat4.Identity);

            // Create the graphical representation of the item.
            MeshRenderJob renderJob = new MeshRenderJob(
                Renderer.Opaque.Mesh,
                PresentedItem.EntityMaterial,
                PresentedItem.EntityMesh,
                mat4.Identity
                );
            renderComponent = new RenderComponent(thisEntity, mat4.Identity, renderJob, true);

            MeshRenderJob renderJobShadow = new MeshRenderJob(
                Renderer.Shadow.Mesh, 
                Resources.UseMaterial("::Shadow", HostScript.ModDomain), 
                PresentedItem.EntityMesh,
                mat4.Identity
                );
            renderComponentShadow = new RenderComponent(thisEntity, mat4.Identity, renderJobShadow, true);


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
            interactionMsg.Sender[AddItemTrigger] |= new AddItemMessage(PresentedItem);

            // For now, simply hide this entity (entity deletion is not yet possible)
            renderComponent.Visible = false;
            renderComponentShadow.Visible = false;
            physicsComponent.Transform = mat4.Scale(0f);
        }
    }
}

