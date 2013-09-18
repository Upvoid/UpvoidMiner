using System;

using Engine.Resources;

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
        /// The item's name. Has to be unique.
        /// </summary>
        public virtual string Name { get; protected set; }

        /// <summary>
        /// A short description of the item.
        /// </summary>
        public virtual string Description { get; protected set; }

        /// <summary>
        /// The physical weight of the item in kilograms.
        /// </summary>
        public virtual float Weight { get; protected set; }

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
        /// When displaying the item in the world, this mesh will be used.
        /// </summary>
        [NonSerialized]
        public MeshResource EntityMesh;
        /// <summary>
        /// When displaying the item in the world, this material will be used.
        /// </summary>
        [NonSerialized]
        public MaterialResource EntityMaterial;

        /// <summary>
        /// Items in the world are currently represented as spheres in the physics world. This is the radius of that sphere.
        /// </summary>
        public float EntityRadius;

        public Item(string name, string description, float weight, bool isUsable, MaterialResource entityMaterial, MeshResource entityMesh, float entityRadius)
        {
            Name = name;
            Description = description;
            Weight = weight;
            IsUsable = isUsable;
            EntityMaterial = entityMaterial;
            EntityMesh = entityMesh;
            EntityRadius = entityRadius;
        }
    }
}

