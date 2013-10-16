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
        }

        /// <summary>
        /// List of logs of this tree.
        /// </summary>
        public readonly List<Log> Logs = new List<Log>();
        /// <summary>
        /// List of non-log foliage of this tree.
        /// </summary>
        public readonly List<Foliage> Leaves = new List<Foliage>();

        public Tree()
        {
        }
    }
}

