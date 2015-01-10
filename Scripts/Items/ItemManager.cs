using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Engine.Physics;

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
        /// Get all items and their entities
        /// </summary>
        public static IEnumerable<KeyValuePair<Item, ItemEntity>> AllItemsEntities { get { return Item2Entity; } }

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
            LocalScript.world.Physics.WakeUpRigidbodies(new BoundingSphere(entity.thisEntity.Position, 3f));
            LocalScript.world.RemoveEntity(entity.thisEntity);
        }

        /// <summary>
        /// Updates all items
        /// </summary>
        public static void Update(float _elapsedSeconds)
        {
            // Find the three closest torches
            vec3 refPos = LocalScript.camera != null ? LocalScript.camera.Position : vec3.Zero;

            // vec4 contains: position (x,y,z) and distance to player (w)
            List<vec4> torchPositions = new List<vec4>();
            torchPositions.Add(new vec4(0, 0, 0, float.MaxValue));
            torchPositions.Add(new vec4(0, 0, 0, float.MaxValue));
            torchPositions.Add(new vec4(0, 0, 0, float.MaxValue));

            foreach (var entity in Item2Entity.Values)
            {
                entity.UpdatePhysics();
                entity.Update(_elapsedSeconds);

                if(entity.RepresentedItem is TorchItem)
                {
                    vec3 torchPos = new vec3(entity.thisEntity.Transform.col3);
                    float dis = vec3.distance(refPos, torchPos);

                    if (dis < torchPositions[0].w)
                    {
                        // Closest torch (of those known)
                        torchPositions[2] = torchPositions[1]; // Move down the line...
                        torchPositions[1] = torchPositions[0]; // Move down the line...
                        torchPositions[0] = new vec4(torchPos, dis);
                    }
                    else if (dis < torchPositions[1].w)
                    {
                        // Second-closest torch (of those known)
                        torchPositions[2] = torchPositions[1]; // Move down the line...
                        torchPositions[1] = new vec4(torchPos, dis);
                    }
                    else if (dis < torchPositions[2].w)
                    {
                        // Third-closest torch (of those known)
                        torchPositions[2] = new vec4(torchPos, dis);
                    }
                    // Otherwise: Further away than at least three other torches
                }
            }

            // Update the torch fire sound
            TorchItem.UpdateTorchSound(torchPositions);
        }
    }
}
