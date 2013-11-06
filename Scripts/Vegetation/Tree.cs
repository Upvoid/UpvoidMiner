using System;
using System.Collections.Generic;
using Engine.Universe;

namespace UpvoidMiner
{
    /// <summary>
    /// A basic Tree entity.
    /// </summary>
    public class Tree : EntityScript
    {
        /// Foliage are render-only leaves and small branches of a tree.
        public class Foliage
        {
            /// <summary>
            /// All render components of this foliage.
            /// </summary>
            public readonly List<RenderComponent> RenderComps = new List<RenderComponent>();

            /// <summary>
            /// Initializes all components
            /// </summary>
            public void Init(EntityScript e)
            {
                foreach (var c in RenderComps)
                {
                    e.thisEntity.AddComponent(c);
                }
            }
        }

        /// Logs are physics-enabled parts of the tree.
        public class Log
        {
            /// <summary>
            /// Foliage of this log.
            /// </summary>
            public readonly List<Foliage> Foliage = new List<Foliage>();
            
            /// <summary>
            /// All render components of this log.
            /// </summary>
            public readonly List<RenderComponent> RenderComps = new List<RenderComponent>();
            
            /// <summary>
            /// All physics components of this log.
            /// </summary>
            public readonly List<PhysicsComponent> PhysicsComps = new List<PhysicsComponent>();

            /// <summary>
            /// Initializes all components
            /// </summary>
            public void Init(EntityScript e)
            {
                foreach (var leaf in Foliage)
                {
                    leaf.Init(e);
                }

                foreach (var c in RenderComps)
                {
                    e.thisEntity.AddComponent(c);
                }

                foreach (var c in PhysicsComps)
                {
                    e.thisEntity.AddComponent(c);
                }
            }
        }

        /// <summary>
        /// List of logs of this tree.
        /// </summary>
        public readonly List<Log> Logs = new List<Log>();
        /// <summary>
        /// List of non-log foliage of this tree.
        /// </summary>
        public readonly List<Foliage> Leaves = new List<Foliage>();

        /// <summary>
        /// Initializes all components
        /// </summary>
        private void InitComps()
        {
            foreach (var log in Logs)
            {
                log.Init(this);
            }

            foreach (var leaf in Leaves)
            {
                leaf.Init(this);
            }
        }

        public Tree()
        {
        }

        protected override void Init()
        {
            base.Init();

            InitComps();
        }
    }
}

