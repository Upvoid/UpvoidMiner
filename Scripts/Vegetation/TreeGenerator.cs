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
using Engine;
using Engine.Resources;
using Engine.Universe;
using Engine.Rendering;
using Engine.Physics;

namespace UpvoidMiner
{
    /// <summary>
    /// A class that can spawn different types of trees.
    /// </summary>
    public class TreeGenerator
    {
        public static Tree OldTree(Random random, mat4 transform1, mat4 transform2, World world)
        {
            bool type0 = random.NextDouble() > 0.5;

            MeshRenderJob leavesOpaque = new MeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("TreeLeaves01", UpvoidMiner.ModDomain),
                Resources.UseMesh(type0 ? "Vegetation/Tree01/Leaves_high" : "Vegetation/Tree03/Leaves_high", UpvoidMiner.ModDomain),
                transform2);

            MeshRenderJob leavesZPre = new MeshRenderJob(
                Renderer.zPre.Mesh,
                Resources.UseMaterial("TreeLeaves01.zPre", UpvoidMiner.ModDomain),
                Resources.UseMesh(type0 ? "Vegetation/Tree01/Leaves_high" : "Vegetation/Tree03/Leaves_high", UpvoidMiner.ModDomain),
                transform2);

            MeshRenderJob leavesShadow = new MeshRenderJob(
                Renderer.Shadow.Mesh,
                Resources.UseMaterial("TreeLeaves01.Shadow", UpvoidMiner.ModDomain),
                Resources.UseMesh(type0 ? "Vegetation/Tree01/Leaves_high" : "Vegetation/Tree03/Leaves_high", UpvoidMiner.ModDomain),
                transform2);

            MeshRenderJob trunkOpaque = new MeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("TreeTrunk", UpvoidMiner.ModDomain),
                Resources.UseMesh(type0 ? "Vegetation/Tree01/Trunk" : "Vegetation/Tree03/Trunk", UpvoidMiner.ModDomain),
                transform2);

            MeshRenderJob trunkShadow = new MeshRenderJob(
                Renderer.Shadow.Mesh,
                Resources.UseMaterial("::Shadow", UpvoidMiner.ModDomain),
                Resources.UseMesh(type0 ? "Vegetation/Tree01/Trunk" : "Vegetation/Tree03/Trunk", UpvoidMiner.ModDomain),
                transform2);


            // Add some color variance to trees
            vec4 colorModulation = new vec4(0.7f + (float)random.NextDouble() * 0.5f, 0.7f + (float)random.NextDouble() * 0.5f, 1, 1);
            leavesOpaque.SetColor("uColorModulation", colorModulation);

            Tree t = new Tree();
            Tree.Log l = new Tree.Log();
            RigidBody b = world.Physics.CreateAndAddRigidBody(0f, transform1 * mat4.Translate(new vec3(0,5,0)), new CylinderShape(.5f, 10));
            l.PhysicsComps.Add(new PhysicsComponent(b, mat4.Translate(new vec3(0,-5,0))));

            l.RenderComps.Add(new RenderComponent(leavesOpaque, transform2));
            l.RenderComps.Add(new RenderComponent(leavesZPre, transform2));
            l.RenderComps.Add(new RenderComponent(leavesShadow, transform2));
            l.RenderComps.Add(new RenderComponent(trunkOpaque, transform2));
            l.RenderComps.Add(new RenderComponent(trunkShadow, transform2));

            t.Logs.Add(l);

            return t;
        }

        /// <summary>
        /// Creates a log.
        /// </summary>
        private static Tree.Log CreateLog(Tree t,
                                          vec3 start, vec3 dir, vec3 front, float height, float radius,
                                          MaterialResource material, string meshName)
        {
            Tree.Log log = new Tree.Log();

            vec3 left = vec3.cross(dir, front);
            mat4 transform = new mat4(left, dir, front, start) * mat4.Scale(new vec3(radius, height, radius));
            var mesh = Resources.UseMesh(meshName, UpvoidMiner.ModDomain);
            
            log.RenderComps.Add(new RenderComponent( new MeshRenderJob(Renderer.Opaque.Mesh, material, mesh, mat4.Identity), transform));
            log.RenderComps.Add(new RenderComponent( new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", null), mesh, mat4.Identity), transform));
            log.RenderComps.Add(new RenderComponent( new MeshRenderJob(Renderer.zPre.Mesh, Resources.UseMaterial("::ZPre", null), mesh, mat4.Identity), transform));

            return log;
        }

        /// <summary>
        /// Creates a birch tree
        /// </summary>
        /// <param name="height">Height in m.</param>
        /// <param name="width">radius in m.</param>
        public static Tree Birch(float height, float radius, Random random)
        {
            Tree t = new Tree();
            
            SeedPointMeshRenderJob foliageJob = new SeedPointMeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("SimpleBirchLeaves", UpvoidMiner.ModDomain),
                Resources.UseMesh("Vegetation/Leaves", UpvoidMiner.ModDomain),
                mat4.Identity);
            SeedPointMeshRenderJob foliageJob2 = new SeedPointMeshRenderJob(
                Renderer.Transparent.Mesh,
                Resources.UseMaterial("SimpleBirchLeaves.Transparent", UpvoidMiner.ModDomain),
                Resources.UseMesh("Vegetation/Leaves", UpvoidMiner.ModDomain),
                mat4.Identity);
            SeedPointMeshRenderJob foliageJob3 = new SeedPointMeshRenderJob(
                Renderer.zPre.Mesh,
                Resources.UseMaterial("BirchLeaves.zPre", UpvoidMiner.ModDomain),
                Resources.UseMesh("Vegetation/Leaves", UpvoidMiner.ModDomain),
                mat4.Identity);

            float hsum = 0;
            float unitHeight = radius * 2 * (float)Math.PI * 2 * .6f;
            MaterialResource mat = Resources.UseMaterial("Vegetation/BirchLog", UpvoidMiner.ModDomain);
            while (hsum < height)
            {
                float h = unitHeight * (.8f + (float)random.NextDouble() * .4f);
                t.Logs.Add(CreateLog(t, new vec3(0, hsum, 0), vec3.UnitY, vec3.UnitZ, h, radius, mat, "Vegetation/Trunk-1.0"));

                int leaves = (int)(0 + (hsum / height) * (8 + random.Next(0, 3)));
                for (int i = 0; i < leaves * 4; ++i)
                {
                    vec3 rad = new vec3((float)random.NextDouble() - .5f, 0, (float)random.NextDouble() - .5f).Normalized;

                    vec3 pos = new vec3(0, hsum + (float)random.NextDouble() * h, 0) + rad * radius * .9f;
                    vec3 normal = (rad + new vec3(0,.3f - (float)random.NextDouble() * .6f,0)).Normalized * (1 + (float)random.NextDouble() * (.2f + hsum/height * .3f));
                    vec3 tangent = vec3.cross(normal, new vec3((float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f)).Normalized * (1 + (float)random.NextDouble() * (.2f + hsum/height * .3f));
                    vec3 color = new vec3(.9f + (float)random.NextDouble() * .4f, 1, 1);
                    
                    foliageJob.AddSeed(pos, normal, tangent, color);
                    foliageJob2.AddSeed(pos, normal, tangent, color);
                    foliageJob3.AddSeed(pos, normal, tangent, color);
                }
                
                hsum += h; 
            }

            int branches = random.Next(3, 5);
            for (int i = 0; i < branches; ++i)
            {
                float h = (float)((1 - random.NextDouble() * random.NextDouble()) * hsum) * .8f + .1f;
                vec3 dir = new vec3((float)random.NextDouble() * 2  - 1, .3f + (float)random.NextDouble() * .6f, (float)random.NextDouble() * 2 - 1).Normalized;
                vec3 front = vec3.cross(dir, vec3.UnitY).Normalized;
                vec3 left = vec3.cross(front, dir);
                float r = radius * (0.2f + (float)random.NextDouble() * .4f);
                vec3 basePos = new vec3(0, h, 0);
                float branchLength = unitHeight * (.8f + (float)random.NextDouble() * .4f + .1f) * .7f;
                t.Logs.Add(CreateLog(t, basePos, dir, front, branchLength, r, mat, "Vegetation/Trunk-0.8")); 
                
                int leaves = (int)(4 + random.Next(0, 3));
                for (int j = 0; j < leaves * 4; ++j)
                {
                    vec3 rad = (((float)random.NextDouble() - .5f) * left + ((float)random.NextDouble() - .5f) * front).Normalized;

                    vec3 pos = basePos + dir * branchLength * (float)random.NextDouble() + rad * r * .9f;
                    vec3 normal = (rad + new vec3(0,.3f - (float)random.NextDouble() * .6f,0)).Normalized * (1 + (float)random.NextDouble() * (.2f + hsum/height * .3f));
                    vec3 tangent = vec3.cross(normal, new vec3((float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f, (float)random.NextDouble() - .5f)).Normalized * (1 + (float)random.NextDouble() * (.2f + hsum/height * .3f));
                    vec3 color = new vec3(.9f + (float)random.NextDouble() * .4f, 1, 1);

                    foliageJob.AddSeed(pos, normal, tangent, color);
                    foliageJob2.AddSeed(pos, normal, tangent, color);
                    foliageJob3.AddSeed(pos, normal, tangent, color);
                }
            }
            
            foliageJob.FinalizeSeeds();
            foliageJob2.FinalizeSeeds();
            foliageJob3.FinalizeSeeds();
            Tree.Foliage foliage = new Tree.Foliage();
            foliage.RenderComps.Add(new RenderComponent(foliageJob, mat4.Identity));
            foliage.RenderComps.Add(new RenderComponent(foliageJob2, mat4.Identity));
            foliage.RenderComps.Add(new RenderComponent(foliageJob3, mat4.Identity));
            t.Leaves.Add(foliage);

            return t;
        }
    }
}

