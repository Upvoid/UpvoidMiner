using System;
using Engine;
using Engine.Resources;
using Engine.Universe;
using Engine.Rendering;

namespace UpvoidMiner
{
    /// <summary>
    /// A class that can spawn different types of trees.
    /// </summary>
    public class TreeGenerator
    {
        private static Random random = new Random();

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

            return log;
        }

        /// <summary>
        /// Creates a birch tree
        /// </summary>
        /// <param name="height">Height in m.</param>
        /// <param name="width">radius in m.</param>
        public static Tree Birch(float height, float radius)
        {
            Tree t = new Tree();
            
            SeedPointMeshRenderJob foliageJob = new SeedPointMeshRenderJob(
                Renderer.Opaque.Mesh,
                Resources.UseMaterial("BirchLeaves", UpvoidMiner.ModDomain),
                Resources.UseMesh("Vegetation/Leaves", UpvoidMiner.ModDomain),
                mat4.Identity);
            SeedPointMeshRenderJob foliageJob2 = new SeedPointMeshRenderJob(
                Renderer.Transparent.Mesh,
                Resources.UseMaterial("BirchLeaves.Transparent", UpvoidMiner.ModDomain),
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
                }
            }
            
            foliageJob.FinalizeSeeds();
            foliageJob2.FinalizeSeeds();
            Tree.Foliage foliage = new Tree.Foliage();
            foliage.RenderComps.Add(new RenderComponent(foliageJob, mat4.Identity));
            foliage.RenderComps.Add(new RenderComponent(foliageJob2, mat4.Identity));
            t.Leaves.Add(foliage);

            return t;
        }
    }
}

