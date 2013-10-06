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
    public abstract class Item
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
        /// True iff the item has a preview when it is selected.
        /// </summary>
        public virtual bool HasPreview { get; protected set; }

        /// <summary>
        /// True iff the item represents an empty (or negative) amount of the item.
        /// </summary>
        public virtual bool IsEmpty { get { return false; } }

        /// <summary>
        /// If IsUsable is true, this executes the "use" action of the item. Does nothing otherwise.
        /// _worldPos is the world position that the player points at.
        /// </summary>
        public virtual void OnUse(Player player, vec3 _worldPos) { }
        /// <summary>
        /// Is executed when this item is selected by a player.
        /// </summary>
        public virtual void OnSelect() { }
        /// <summary>
        /// Is executed when this item is de-selected by a player.
        /// </summary>
        public virtual void OnDeselect() { }
        /// <summary>
        /// If selected and the user scrolls with the mouse wheel, this function is called with the delta. It should be used to modify 'OnUse' parameter.
        /// </summary>
        public virtual void OnUseParameterChange(float _delta) { }
        /// <summary>
        /// Is executed when this item is selected and a preview ray point was found.
        /// If not _visible, _worldPos is zero, otherwise it is the point where the player looks at.
        /// </summary>
        public virtual void OnPreview(vec3 _worldPos, bool _visible) { }

        /// <summary>
        /// Index for quickaccess, -1 for none.
        /// </summary>
        public int QuickAccessIndex { get; set; }

        public Item(string name, string description, float weight, ItemCategory category)
        {
            Name = name;
            Description = description;
            Weight = weight;
            Category = category;
            QuickAccessIndex = -1;
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
        /// If substract, rhs is removed from this item (no negatives allowed).
        /// If force and subtract, the "can-remove" check will be ignored.
        /// If dryrun, actual merge will not be executed even if possible.
        /// </summary>
        public abstract bool TryMerge(Item rhs, bool substract, bool force, bool dryrun = false);
        /// <summary>
        /// Creates a copy of this item.
        /// </summary>
        public abstract Item Clone();
    }
}

