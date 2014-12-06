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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Engine;
using Engine.Audio;
using Engine.Universe;
using Engine.Resources;
using Engine.Rendering;
using Engine.Physics;
using System.Collections.Generic;
using UpvoidMiner.UI;
using Engine.Statistics;

namespace UpvoidMiner
{
    public class DiggingController
    {
        private static Random random = new Random();

        /// <summary>
        /// Contains all relevant settings for the digging tools. Used to save settings per tool and to pass them to the GUI
        /// </summary>
        [Serializable]
        public class DiggingSettings
        {
            public DigMode Mode = DigMode.Subtract;
            public DigShape Shape = DigShape.Sphere;
            public DigAlignment Alignment = DigAlignment.Axis;
            public DigPosition Position = DigPosition.Ground;
            public AddMode AddMode = AddMode.AirOnly;
            //public vec3 Scale = new vec3(1f);
        }

        /// <summary>
        /// Singleton for this controller.
        /// </summary>
        private static DiggingController instance;

        [Serializable]
        public enum DigMode
        {
            Subtract = 1,
            Add
        }

        [Serializable]
        public enum DigShape
        {
            Sphere = 1,
            Box,
            Cylinder,
            Cone
        }

        [Serializable]
        public enum DigPivot
        {
            Top = 1,
            Center,
            Bottom
        }

        [Serializable]
        public enum PhysicsMode
        {
            Thrown = 1,
            Dynamic,
            Static
        }

        /// <summary>
        /// Describes how the digging shape should be rotated
        /// </summary>
        [Serializable]
        public enum DigAlignment
        {
            /// <summary>
            /// Align the shape along the coordinate system (no rotation) 
            /// </summary>
            Axis = 1,
            /// <summary>
            /// Align the shape along the viewing direction of the player
            /// </summary>
            View,
            /// <summary>
            /// Align the shape along the terrain surface orientation
            /// </summary>
            Terrain,
            /// <summary>
            /// Align the shape along the coordinate system (no rotation) and snap to grid
            /// </summary>
            GridAligned
        }

        /// <summary>
        /// Describes where the digging shape should be placed
        /// </summary>
        [Serializable]
        public enum DigPosition
        {
            /// <summary>
            /// Place the shape on the ground where the center of the screen is pointing at (or nowhere if that position is too far away or non-existent)
            /// </summary>
            Ground = 1,
            FixedDistance
        }

        /// <summary>
        /// Describes where material should be added
        /// </summary>
        [Serializable]
        public enum AddMode
        {
            /// <summary>
            /// Fill the whole digging shape with material
            /// </summary>
            Overwrite = 1,
            /// <summary>
            /// Fill only the parts inside the digging shape that are air
            /// </summary>
            AirOnly,
            /// <summary>
            /// Only replace the parts inside the digging shape that are non-air ("Paint mode")
            /// </summary>
            NonAirOnly
        }

        /// <summary>
        /// Backref to world.
        /// </summary>
        World world;
        /// <summary>
        /// Backref to player.
        /// </summary>
        Player player;
        /// <summary>
        /// Cached CSG Sphere.
        /// </summary>
        CsgExpression sphereNode;
        /// <summary>
        /// Cached CSG Cube.
        /// </summary>
        CsgExpression boxNode;
        /// <summary>
        /// Cached CSG Cylinder.
        /// </summary>
        CsgExpression cylinderNode;
        /// <summary>
        /// Cached CSG Cone.
        /// </summary>
        CsgExpression coneNode;
        /// <summary>
        /// Cached Player safety margin.
        /// </summary>
        CsgExpression playerNode;
        /// <summary>
        /// Time the last stat callback was executed
        /// </summary>
        DateTime timeDirtSound = DateTime.Now;
        DateTime timeStoneSound = DateTime.Now;
        /// <summary>
        /// Cached sound resources for digging dirt and stone
        /// </summary>
        private static SoundResource[] dirtSoundResource;
        private static SoundResource[] stoneSoundResource;
        private static vec3 diggingPosition = vec3.Zero;

        /// <summary>
        /// Particle system for 3D stones due to digging.
        /// </summary>
        class StoneParticles
        {
            public SolidTerrainResource resource;
            public CpuParticleSystem particlesStones;

            public StoneParticles(SolidTerrainResource res, World world)
            {
                resource = res;

                particlesStones = new CpuParticleSystem(2, 0.05);

                // aPosition [m]
                // TIMESTEP  [s]
                // aVelocity [m/s]
                // g [m/s²]


                particlesStones.AddAttributeVec3("aPosition");
                particlesStones.AddAttributeVec3("aVelocity");
                particlesStones.AddAttributeFloat("aSize");
                particlesStones.AddAttributeFloat("aCurrentLifetime");
                particlesStones.AddAttributeFloat("aMaxLifetime");
                particlesStones.AddAttributeVec3("aTangent");
                particlesStones.AddAttributeVec3("aBiTangent");
                particlesStones.AddAttributeVec3("aAngularVelocity");

                CpuParticleModifier mody = new CpuParticleModifier();
                particlesStones.AddModifier(mody);

                string modyAttributesInOut = "aPosition:vec3;aVelocity:vec3;aCurrentLifetime:float";
                string modyExpression =
                    "t = particle::TIMESTEP;" +
                    "l = particle::aCurrentLifetime + t;" +
                    "v = particle::aVelocity + t * vec(0, -9.81, 0);" +
                    "p = particle::aPosition + t * v;" +
                    "vec(p, v, l)";

                mody.AddFiller(new CpuParticleExpressionFiller(modyAttributesInOut, modyAttributesInOut, modyExpression, null));

                string lifeAttributes = "aCurrentLifetime:float;aMaxLifetime:float";
                string deathExpression = "ite(particle::aCurrentLifetime - particle::aMaxLifetime, 1, 0)";

                particlesStones.AddDeathCondition(new CpuParticleDeathCondition(lifeAttributes, deathExpression, null));

                LocalScript.ParticleEntity.AddComponent(new CpuParticleComponent(particlesStones, mat4.Identity));

                LocalScript.ParticleEntity.AddComponent(new RenderComponent(
                    (new CpuParticleRenderJob(particlesStones,
                                              Renderer.Opaque.CpuParticles,
                                              res.DigParticleMaterial,
                                              Resources.UseMesh("::Particles/Rock", null),
                                              mat4.Identity)),
                    mat4.Identity,
                    true));

                LocalScript.ParticleEntity.AddComponent(new RenderComponent(
                    (new CpuParticleRenderJob(particlesStones,
                                              Renderer.Shadow.CpuParticles,
                                              Resources.UseMaterial("Particles/Shadow/Mesh", UpvoidMiner.ModDomain),
                                              Resources.UseMesh("::Particles/Rock", null),
                                              mat4.Identity)),
                    mat4.Identity,
                    true));
            }
        };

        private Dictionary<int, StoneParticles> stoneParticles = new Dictionary<int, StoneParticles>();

        public DiggingController(World world, Player player)
        {
            Debug.Assert(instance == null, "Singleton is violated");
            instance = this;
            this.world = world;
            this.player = player;

            string digParas = "digRadius:float, digPosition:vec3, digDirX:vec3, digDirY:vec3, digDirZ:vec3";

            string sphereExpression = "-digRadius + distance(vec3(x,y,z), digPosition)";
            sphereNode = new CsgExpression(1, sphereExpression, UpvoidMiner.ModDomain, digParas);

            string boxExpression = @"p = vec3(x,y,z) - digPosition;
                dx = abs(dot(p, digDirX));
                dy = abs(dot(p, digDirY));
                dz = abs(dot(p, digDirZ));
                -digRadius + max(dx, max(dy, dz))";
            boxNode = new CsgExpression(1, boxExpression, UpvoidMiner.ModDomain, digParas);

            string cylinderExpression = @"p = vec3(x,y,z) - digPosition;
                dx = dot(p, digDirX);
                dy = abs(dot(p, digDirY));
                dz = dot(p, digDirZ);
                -digRadius + max(dy, length(vec2(dx, dz)))";
            cylinderNode = new CsgExpression(1, cylinderExpression, UpvoidMiner.ModDomain, digParas);

            string coneExpression = @"p = vec3(x,y,z) - digPosition;
                dx = dot(p, digDirX);
                dy = dot(p, digDirY);
                dz = dot(p, digDirZ);
                -digRadius + max(-dy, 2*length(vec2(dx, dz)) + dy)";
            coneNode = new CsgExpression(1, coneExpression, UpvoidMiner.ModDomain, digParas);

            string playerExpression = @"p = vec3(x,y,z) - playerPosition;
                max(abs(p.y) - (playerHeight/2), length(p.xz) - playerRadius)";
            playerNode = new CsgExpression(1, playerExpression, UpvoidMiner.ModDomain, "playerRadius:float, playerHeight:float, playerPosition:vec3");



            dirtSoundResource = new SoundResource[6];
            stoneSoundResource = new SoundResource[5];

            // Load dirt digging sound resources
            for (int i = 1; i <= 6; ++i)
            {
                dirtSoundResource[i - 1] = Resources.UseSound("Mods/Upvoid/Resources.SFX/1.0.0::Digging/Dirt/Dirt" + i.ToString("00"), UpvoidMiner.ModDomain);

            }

            // Load stone digging sound resources
            for (int i = 1; i <= 5; ++i)
            {
                stoneSoundResource[i - 1] = Resources.UseSound("Mods/Upvoid/Resources.SFX/1.0.0::Digging/Stone/Stone" + i.ToString("00"), UpvoidMiner.ModDomain);
            }
        }

        /// <summary>
        /// Generic digging function
        /// </summary>
        /// <param name="shape">Shape of the terrain modification.</param>
        /// <param name="shapeBoundary">bounding sphere for the modification (as tight as possible has better performance)</param>
        /// <param name="digMode">Dig composition mode</param>
        /// <param name="materialFilter">if non-null: a whitelist of materials that are allowed to change (air should be configured with the next parameter)</param>
        /// <param name="allowAirChange">If set to <c>true</c> allow air change.</param>
        public void Dig(CsgNode shape, BoundingSphere shapeBoundary, DigMode digMode, IEnumerable<int> materialFilter, bool allowAirChange = true)
        {
            CsgNode digShape = null;

            // Keep track of where we are digging right now
            diggingPosition = shapeBoundary.Center;

            // constraintDiffNode performs the constraint as a CSG operation
            // by cutting away anything of thge digging shape not inside the allowed area.
            CsgOpDiff constraintDiffNode = new CsgOpDiff();
            // Assemble difference operation by applying all drone constraints.
            player.AddDroneConstraints(constraintDiffNode, shapeBoundary.Center);

            // When placing material, add a safety margin around the player to prevent it from physically glitching trough the terrain
            if (digMode == DigMode.Add)
            {
                playerNode.SetParameterFloat("playerRadius", player.Character.CharacterDiameter * 0.5f + 0.35f);
                playerNode.SetParameterVec3("playerPosition", player.Character.Position);
                playerNode.SetParameterFloat("playerHeight", player.Character.BodyHeight + 0.3f);
                constraintDiffNode.AddNode(playerNode);
            }

            // We apply the constraint by substracting it from the given shape.
            CsgOpConcat constraintedShape = new CsgOpConcat();
            constraintedShape.AddNode(shape);
            constraintedShape.AddNode(constraintDiffNode);
            digShape = constraintedShape;

            CsgNode digNode = null;
            // Depending on the digging mode, we either add or substract the digging shape from the terrain.
            if (digMode == DigMode.Subtract)
            {
                digNode = new CsgOpDiff(digShape);
            }
            else
            {
                digNode = new CsgOpUnion(digShape);
            }

            // Filter for tools
            CsgNode filterNode = digNode;
            if (materialFilter != null)
            {
                CsgFilterNode filter = new CsgFilterNode(true, digNode);
                foreach (int mat in materialFilter)
                    filter.AddMaterial(mat);
                if (allowAirChange)
                    filter.AddMaterial(0); // Air must be white-listed, too!
                filterNode = filter;
            }
            else if (!allowAirChange) // special case: no filter but no air? black-list air
            {
                CsgFilterNode filter = new CsgFilterNode(false, digNode);
                filter.AddMaterial(0);
                filterNode = filter;
            }

            // Float elimination
            CsgOpConcat collapser = new CsgOpConcat(filterNode);

            // No collapsing terrain for now
            //collapser.AddNode(new CsgCollapseNode());

            // Callback for statistical purposes.
            CsgStatCallback finalNode = new CsgStatCallback(collapser, 4, 4);
            finalNode.AddSimpleVolumeCallback("UpvoidMiner", UpvoidMiner.ModDomain, "UpvoidMiner.DiggingController", "StatCallback");
            if (Settings.settings.DigParticles)
                finalNode.AddVolumeChangePointCallback("UpvoidMiner", UpvoidMiner.ModDomain, "UpvoidMiner.DiggingController", "PointCallback");

            world.Terrain.ModifyTerrain(shapeBoundary, finalNode);
        }

        public void DigSphere(vec3 worldNormal, vec3 position, float radius, IEnumerable<int> filterMaterials, int terrainMaterialId = 1, DigMode digMode = DigMode.Subtract, bool allowAirChange = true)
        {
            sphereNode.MaterialIndex = terrainMaterialId;
            sphereNode.SetParameterFloat("digRadius", radius);
            sphereNode.SetParameterVec3("digPosition", position);

            vec3 dx, dy, dz;
            player.AlignmentSystem(worldNormal, out dx, out dy, out dz);
            sphereNode.SetParameterVec3("digDirX", dx);
            sphereNode.SetParameterVec3("digDirY", dy);
            sphereNode.SetParameterVec3("digDirZ", dz);

            // radius + 10%
            Dig(sphereNode, new BoundingSphere(position, radius * 1.1f), digMode, filterMaterials, allowAirChange);
        }

        public void DigBox(vec3 worldNormal, vec3 position, float radius, IEnumerable<int> filterMaterials, int terrainMaterialId = 1, DigMode digMode = DigMode.Subtract, bool allowAirChange = true)
        {
            boxNode.MaterialIndex = terrainMaterialId;
            boxNode.SetParameterFloat("digRadius", radius);
            boxNode.SetParameterVec3("digPosition", position);

            vec3 dx, dy, dz;
            player.AlignmentSystem(worldNormal, out dx, out dy, out dz);
            boxNode.SetParameterVec3("digDirX", dx);
            boxNode.SetParameterVec3("digDirY", dy);
            boxNode.SetParameterVec3("digDirZ", dz);

            // radius * sqrt(3) + 10%
            Dig(boxNode, new BoundingSphere(position, radius * 1.733f * 1.1f), digMode, filterMaterials, allowAirChange);
        }

        public void DigCylinder(vec3 worldNormal, vec3 position, float radius, IEnumerable<int> filterMaterials, int terrainMaterialId = 1, DigMode digMode = DigMode.Subtract, bool allowAirChange = true)
        {
            cylinderNode.MaterialIndex = terrainMaterialId;
            cylinderNode.SetParameterFloat("digRadius", radius);
            cylinderNode.SetParameterVec3("digPosition", position);

            vec3 dx, dy, dz;
            player.AlignmentSystem(worldNormal, out dx, out dy, out dz);
            cylinderNode.SetParameterVec3("digDirX", dx);
            cylinderNode.SetParameterVec3("digDirY", dy);
            cylinderNode.SetParameterVec3("digDirZ", dz);

            // radius * sqrt(2) + 10%
            Dig(cylinderNode, new BoundingSphere(position, radius * 1.414f * 1.1f), digMode, filterMaterials, allowAirChange);
        }

        public void DigCone(vec3 worldNormal, vec3 position, float radius, IEnumerable<int> filterMaterials, int terrainMaterialId = 1, DigMode digMode = DigMode.Subtract, bool allowAirChange = true)
        {
            coneNode.MaterialIndex = terrainMaterialId;
            coneNode.SetParameterFloat("digRadius", radius);
            coneNode.SetParameterVec3("digPosition", position);

            vec3 dx, dy, dz;
            player.AlignmentSystem(worldNormal, out dx, out dy, out dz);
            coneNode.SetParameterVec3("digDirX", dx);
            coneNode.SetParameterVec3("digDirY", dy);
            coneNode.SetParameterVec3("digDirZ", dz);

            Dig(coneNode, new BoundingSphere(position, radius * 1.5f), digMode, filterMaterials, allowAirChange);
        }

        /// <summary>
        /// This callback is called once per changed material in a chunk and reports the amount of volume changed (in m^3).
        /// </summary>
        public static void StatCallback(int mat, float volume, int lod)
        {
            if (mat != 0)
            {

                // Resolve terrain material.
                TerrainResource material = TerrainResource.FromIndex(mat);
                Substance substance = material.Substance;
                Debug.Assert(material != null, "Invalid terrain material");


                DateTime currentTime = DateTime.Now;
                const int soundCooldownMs = 100;

                // Depending on whether we dig dirt or stone, play a random digging sound

                Sound digSound = null;
                if (substance is LooseSubstance && currentTime > instance.timeDirtSound + TimeSpan.FromMilliseconds(soundCooldownMs))
                {
                    // Dirt material
                    instance.timeDirtSound = currentTime;
                    digSound = new Sound(dirtSoundResource[random.Next(0, 5)], vec3.Zero, false, 1, 1, (int)AudioType.SFX, true);
                }
                else if (substance is RockSubstance && currentTime > instance.timeStoneSound + TimeSpan.FromMilliseconds(soundCooldownMs))
                {
                    // Any material beginning with "Stone"
                    instance.timeStoneSound = currentTime;
                    digSound = new Sound(stoneSoundResource[random.Next(0, 4)], vec3.Zero, false, 1, 1, (int)AudioType.SFX, true);
                }
                else if (substance is PlantSubstance && currentTime > instance.timeDirtSound + TimeSpan.FromMilliseconds(soundCooldownMs))
                {
                    // Fallback, i.e. all other materials
                    instance.timeDirtSound = currentTime;
                    digSound = new Sound(dirtSoundResource[random.Next(0, 5)], vec3.Zero, false, 1, 1, (int)AudioType.SFX, true);
                }
                else if (substance is MetalSubstance && currentTime > instance.timeStoneSound + TimeSpan.FromMilliseconds(soundCooldownMs))
                {
                    // Any material beginning with "Stone"
                    instance.timeStoneSound = currentTime;
                    digSound = new Sound(stoneSoundResource[random.Next(0, 4)], vec3.Zero, false, 1, 1, (int)AudioType.SFX, true);
                }

                // +/- 15% pitching
                if (digSound != null)
                {
                    digSound.Pitch = 1.0f + (0.3f * (float)random.NextDouble() - 0.15f);
                    digSound.Position = diggingPosition;
                    digSound.Play();
                }


                // Add proper amount of material to player inventory.
                // If the material changed by a negative volume we want to collect a positive amount.
                // only do if non-god
                if (!instance.player.GodMode)
                    instance.player.Inventory.AddResource(substance, -volume);

                // Tutorial
                if (volume < 0)
                {
                    if (instance.player.Inventory.Selection is ToolItem &&
                        (instance.player.Inventory.Selection as ToolItem).ToolType == ToolType.Shovel &&
                        material.Name == "Dirt")
                        Tutorials.MsgBasicDiggingDirt.Report(-volume);

                    if (instance.player.Inventory.Selection is ToolItem &&
                        (instance.player.Inventory.Selection as ToolItem).ToolType == ToolType.Pickaxe &&
                        material.Name.StartsWith("Stone"))
                        Tutorials.MsgBasicDiggingStone.Report(-volume);

                    if (instance.player.Inventory.Selection is ToolItem &&
                        (instance.player.Inventory.Selection as ToolItem).ToolType == ToolType.GodsShovel)
                        Tutorials.MsgBasicDiggingGod.Report(-volume);

                    if (instance.player.Inventory.Selection is ToolItem &&
                        ((instance.player.Inventory.Selection as ToolItem).DigRadiusShovel < 0.6 ||
                        (instance.player.Inventory.Selection as ToolItem).DigRadiusPickaxe < 0.6))
                        Tutorials.MsgAdvancedDiggingSmall.Report(-volume);

                    if (instance.player.Inventory.Selection is ToolItem &&
                        instance.player.CurrentDiggingShape != DigShape.Sphere)
                        Tutorials.MsgAdvancedDiggingNonSphere.Report(-volume);

                    if (instance.player.Inventory.Selection is ToolItem &&
                        instance.player.CurrentDiggingShape == DigShape.Box &&
                        instance.player.CurrentDiggingPivot == DigPivot.Bottom)
                        Tutorials.MsgAdvancedDiggingBottom.Report(-volume);

                    if (instance.player.Inventory.Selection is ToolItem &&
                        instance.player.CurrentDiggingShape == DigShape.Cylinder &&
                        instance.player.CurrentDiggingAlignment == DigAlignment.View &&
                        instance.player.CurrentDiggingPivot == DigPivot.Center)
                        Tutorials.MsgAdvancedDiggingView.Report(-volume);

                    if (instance.player.Inventory.Selection is ToolItem &&
                        instance.player.CurrentDiggingShape == DigShape.Box &&
                        instance.player.CurrentDiggingAlignment == DigAlignment.GridAligned &&
                        instance.player.DiggingAlignmentAxisRotation == 45 / 5)
                        Tutorials.MsgAdvancedDiggingAngle.Report(-volume);

                    if (instance.player.Inventory.Selection is ToolItem &&
                        instance.player.CurrentDiggingShape == DigShape.Sphere &&
                        instance.player.CurrentDiggingAlignment == DigAlignment.GridAligned &&
                        instance.player.DiggingGridSize == 2 * 2)
                        Tutorials.MsgAdvancedDiggingGridSize.Report(-volume);

                    if (instance.player.Inventory.Selection is ResourceItem &&
                        instance.player.CurrentDiggingAddMode == AddMode.NonAirOnly &&
                        material.Name == "Dirt")
                        Tutorials.MsgAdvancedBuildingReplaceMaterial.Report(-volume);
                }
                else if (volume > 0)
                {
                    if (material.Name.StartsWith("Stone"))
                        Tutorials.MsgBasicBuildingStone.Report(volume);

                    if (material.Name == "Dirt")
                        Tutorials.MsgBasicBuildingDirt.Report(volume);

                    if (instance.player.CurrentDiggingAlignment == DigAlignment.Terrain &&
                        instance.player.CurrentDiggingShape == DigShape.Cylinder)
                        Tutorials.MsgAdvancedBuildingTerrainAligned.Report(volume);

                    if (instance.player.CurrentDiggingAddMode == AddMode.Overwrite)
                        Tutorials.MsgAdvancedBuildingReplaceAll.Report(volume);

                    if (material.Name == "Dirt" &&
                        instance.player.DroneConstraints.Any(dc => dc.Drones.Count >= 2))
                        Tutorials.MsgAdvancedBuildingPlaceConstrained.Report(volume);
                }
            }
        }

        /// <summary>
        /// Returns a random direction (currently biased towards cube edges).
        /// </summary>
        private static vec3 RandomDir()
        {
            return new vec3(
                (float)random.NextDouble() - .5f,
                (float)random.NextDouble() - .5f,
                (float)random.NextDouble() - .5f
            ).Normalized;
        }

        /// <summary>
        /// This callback is called for every point that changes materials in a digging operation.
        /// </summary>
        public static void PointCallback(float x, float y, float z, int matPrev, int matNow, int lod)
        {
            // If material was changed from non-air to air: add a particle animation.
            if (matPrev != 0 && matNow == 0)
            {
                // Create particle systems on demand.
                if (!instance.stoneParticles.ContainsKey(matPrev))
                {
                    SolidTerrainResource res = TerrainResource.FromIndex(matPrev) as SolidTerrainResource;
                    if (res != null)
                        instance.stoneParticles.Add(matPrev, new StoneParticles(res, LocalScript.world));
                    else
                        instance.stoneParticles.Add(matPrev, null);
                }

                // Add particle.
                StoneParticles particles = instance.stoneParticles[matPrev];
                if (particles != null)
                {
                    vec3 partPos = new vec3(x, y, z) + RandomDir() * (float)random.NextDouble() * .5f;
                    vec3 partVel = 0.5f * RandomDir() * (1.0f + (float)random.NextDouble() * 1.0f);
                    partVel.y = Math.Abs(partVel.y); // upwards direction
                    float partSize = .2f + (float)random.NextDouble() * .25f;
                    float curLife = .0f;
                    float maxLife = 0.7f + (float)random.NextDouble() * .4f;

                    // Random orientation
                    vec3 tangent = RandomDir();
                    vec3 biTangent = vec3.cross(tangent, RandomDir()).Normalized;

                    // Random rotation axis
                    vec3 axisToRotateAbout = RandomDir();

                    // ...And random speed for the rotation (radians per second)
                    float rotSpeed = 2.2f + (float)random.NextDouble() * 3.45f;
                    axisToRotateAbout *= rotSpeed;

                    particles.particlesStones.AddParticle(
                        partPos.x, partPos.y, partPos.z,
                        partVel.x, partVel.y, partVel.z,
                        partSize, curLife, maxLife,
                        tangent.x, tangent.y, tangent.z,
                        biTangent.x, biTangent.y, biTangent.z,
                        axisToRotateAbout.x, axisToRotateAbout.y, axisToRotateAbout.z);
                }
            }
        }
        /*
        public void construct(vec3 position, CsgNode diggingShape, float influencingRadius)
        {
            CsgOpUnion digDiffNode = new CsgOpUnion(diggingShape);
            world.Terrain.ModifyTerrain(new BoundingSphere(position, influencingRadius), digDiffNode);
        }

        public void constructSphere(vec3 position, float radius)
        {
            dig(position, new CsgExpression("-"+radius.ToString()+" + sqrt(distance2(vec3(x,y,z), vec3"+position.ToString()+"))"));
        }
        */
    }
}

