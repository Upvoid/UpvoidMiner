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
using Engine.Audio;
using Engine.Resources;
using UpvoidMiner.Items;
using UpvoidMiner.UI;

namespace UpvoidMiner
{
    /// <summary>
    /// A basic Tree entity.
    /// </summary>
    public class Tree : EntityScript
    {
        private static Random random = new Random();
        private static SoundResource[] chopSoundResource;
        private static Sound[] chopSound;

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

        /// The amount of wood this tree has.
        float amountOfWood;

        [Serializable]
        public enum TreeType
        {
            Birch,
            Cactus
        };

        TreeType treeType;


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

        public Tree(float _amountOfWood = 0.0f, TreeType _treeType = TreeType.Birch)
        {
            amountOfWood = _amountOfWood;
            treeType = _treeType;
        }

        protected override void Init()
        {
            base.Init();

            // Create sound resources
            if (chopSoundResource == null && chopSound == null)
            {
                chopSoundResource = new SoundResource[5];
                chopSound = new Sound[5];

                // Add dirt digging sounds
                for (int i = 1; i <= 5; ++i)
                {
                    chopSoundResource[i - 1] = Resources.UseSound("Mods/Upvoid/Resources.SFX/1.0.0::Chopping/Wood/Wood" + i.ToString("00"), UpvoidMiner.ModDomain);
                    chopSound[i - 1] = new Sound(chopSoundResource[i - 1], vec3.Zero, false, 1, 1);
                }
            }

            AddTriggerSlot("Hit");

            foreach (var r in RjLeaves0)
                thisEntity.AddComponent(r);
            foreach (var r in RjTrunk)
                thisEntity.AddComponent(r);

            InitComps();
        }

        public void Hit(object message)
        {
            if (!(message is HitMessage))
                return;

            // Tree has been hit (by an axe), so we play a sound...
            Sound woodSound = chopSound[random.Next(0, 4)];
            // +/- 15% pitching
            woodSound.Pitch = 1.0f + (0.3f * (float)random.NextDouble() - 0.15f);
            woodSound.Position = this.Position;
            woodSound.Play();
            
            // ...and we create some wood cylinders the player can pick up
            ContainingWorld.RemoveEntity(thisEntity);
            // remove tree from trees list
            UpvoidMinerWorldGenerator.trees.Remove(this);

            // Gather wood from "real" trees only, not from cacti etc.
            if (treeType != TreeType.Birch)
                return;

            float cylinderHeight = 0.7f;
            int numberOfWoodCylinders = 3 * (int)(amountOfWood + 1.0f);
            for (int i = 0; i < numberOfWoodCylinders; ++i)
            {
                var item = new MaterialItem(TerrainResource.FromName("BirchWood"), MaterialShape.Cylinder, new vec3(0.2f, cylinderHeight, 0.2f));
                var trans = mat4.Translate(Position + new vec3(0, (i + 1.0f) * (cylinderHeight + 0.05f), 0));
                ItemManager.InstantiateItem(item, trans, false);
            }

            // Tutorial
            Tutorials.MsgBasicChoppingTree.Report(1);
        }

    }
}

