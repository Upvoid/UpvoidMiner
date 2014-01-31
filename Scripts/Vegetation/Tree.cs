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
using System.Collections.Generic;
using Engine.Universe;
using Engine;

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

        public vec3 Position;
        
        public List<RenderComponent> RjTrunk = new List<RenderComponent>();
        public List<RenderComponent> RjLeaves0 = new List<RenderComponent>();

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
            
            foreach (var r in RjLeaves0)
                thisEntity.AddComponent(r);
            foreach (var r in RjTrunk)
                thisEntity.AddComponent(r);

            InitComps();
        }
    }
}

