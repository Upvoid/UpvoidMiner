using System;
using System.Collections.Generic;
using Engine;
using Engine.Resources;
using Engine.Universe;
using Engine.Physics;
using Engine.Rendering;

namespace UpvoidMiner
{
    [Serializable]
    /// <summary>
    /// Base class for items.
    /// Can be derive for custom items or instantiated for generic items.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// The item's name. Does not have to be unique.
        /// </summary>
        public virtual string Name { get; protected set; }
        
        /// <summary>
        /// A short description of the item.
        /// </summary>
        public virtual string Description { get; protected set; }

        /// <summary>
        /// A textual description of the stack size. Empty string equals "one".
        /// </summary>
        public virtual string StackDescription { get { return "";} }

        /// <summary>
        /// The physical weight of the item in kilograms.
        /// </summary>
        public virtual float Weight { get; protected set; }

        /// <summary>
        /// Category that this item belongs to.
        /// </summary>
        public virtual ItemCategory Category { get; protected set; }

        /// <summary>
        /// True iff the item is usable.
        /// Examples for usable items are potions and weapons.
        /// </summary>
        public virtual bool IsUsable { get; protected set; }

        /// <summary>
        /// If IsUsable is true, this executes the "use" action of the item. Does nothing otherwise.
        /// </summary>
        public virtual void Use() {}

        /// <summary>
        /// Index for quickaccess, -1 for none.
        /// </summary>
        public int QuickAccessIndex { get; set; }

        public Item(string name, string description, float weight, bool isUsable, ItemCategory category)
        {
            Name = name;
            Description = description;
            Weight = weight;
            IsUsable = isUsable;
            Category = category;
        }

        /// <summary>
        /// Is called when an entity is created for this item (e.g. if dropped).
        /// This function is supposed to add renderjobs and physicscomponents.
        /// Don't forget to add components to the item entity!
        /// </summary>
        public virtual void SetupItemEntity(ItemEntity itemEntity, Entity entity)
        {
            // Create the physical representation of the item.
            RigidBody body = itemEntity.ContainingWorld.Physics.CreateAndAddRigidBody(
                50f,
                mat4.Translate(entity.Position),
                new BoxShape(new vec3(1))
                );
            
            itemEntity.AddPhysicsComponent(new PhysicsComponent(entity, body, mat4.Identity));
            
            // Create the graphical representation of the item.
            MeshRenderJob renderJob = new MeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("Items/Dummy", HostScript.ModDomain),
                Resources.UseMesh("::Debug/Box", HostScript.ModDomain),
                mat4.Identity
                );
            itemEntity.AddRenderComponent(new RenderComponent(entity, mat4.Identity, renderJob, true));
            
            MeshRenderJob renderJobShadow = new MeshRenderJob(
                Renderer.Shadow.Mesh, 
                Resources.UseMaterial("::Shadow", HostScript.ModDomain), 
                Resources.UseMesh("::Debug/Box", HostScript.ModDomain),
                mat4.Identity
                );
            itemEntity.AddRenderComponent(new RenderComponent(entity, mat4.Identity, renderJobShadow, true));

        }

        /// <summary>
        /// Tries to merge this item with the given item.
        /// Returns true, if merge was successful.
        /// </summary>
        public virtual bool TryMerge(Item rhs)
        {
            return false;
        }
    }
}

