using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;

namespace UpvoidMiner.Items
{
    /// <summary>
    /// Manager for items and item entities
    /// </summary>
    public static class ItemManager
    {
        /// <summary>
        /// Mapping from item to its entity
        /// </summary>
        private static readonly Dictionary<Item, ItemEntity> Item2Entity = new Dictionary<Item, ItemEntity>();

        /// <summary>
        /// Creates an entity for item and places it into the world at a given transformation
        /// </summary>
        public static ItemEntity InstantiateItem(Item item, mat4 transform, bool fixedPosition)
        {
            var itemEntity = new ItemEntity(item, fixedPosition);
            LocalScript.world.AddEntity(itemEntity, transform);
            Item2Entity.Add(item, itemEntity);
            return itemEntity;
        }

        /// <summary>
        /// Removes an item from the world
        /// </summary>
        public static void RemoveItemFromWorld(ItemEntity entity)
        {
            Item2Entity.Remove(entity.RepresentedItem);
            LocalScript.world.RemoveEntity(entity.thisEntity);
        }

        /// <summary>
        /// Updates all items
        /// </summary>
        public static void Update()
        {
            foreach (var entity in Item2Entity.Values)
            {
                entity.UpdatePhysics();
            }
        }
    }
}
