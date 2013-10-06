using System;
using System.Diagnostics;
using Engine;
using Engine.Universe;
using Engine.Resources;
using Engine.Rendering;
using Engine.Physics;

namespace UpvoidMiner
{
    public class DiggingController
    {
        private static Random random = new Random();

        /// <summary>
        /// Singleton for this controller.
        /// </summary>
        private static DiggingController instance;

        public enum DigMode
        {
            Substract,
            Add
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
        /// Particle system for 3D stones due to digging.
        /// </summary>
        CpuParticleSystemBase particlesStones;
        RenderComponent particlesStonesRC;
        CpuParticleComponent particlesStonesPC;

        public DiggingController(World world, Player player)
        {
            Debug.Assert(instance == null, "Singleton is violated");
            instance = this;
            this.world = world;
            this.player = player;
            string sphereExpression = "-sphereRadius + distance(vec3(x,y,z), spherePosition)";
            sphereNode = new CsgExpression(1, sphereExpression, HostScript.ModDomain, "sphereRadius:float, spherePosition:vec3");

            // Create particle systems.
            particlesStones = CpuParticleSystem.Create3D(new vec3(0, -9.81f, 0), world);
            particlesStonesPC = new CpuParticleComponent(LocalScript.ParticleEntity, particlesStones, mat4.Identity);
            particlesStonesRC = new RenderComponent(LocalScript.ParticleEntity, mat4.Identity, 
                                                    new CpuParticleRenderJob(particlesStones, 
                                                        Renderer.Opaque.CpuParticles, 
                                                        Resources.UseMaterial("::Particle/Rock", null), 
                                                        Resources.UseMesh("::Particles/Rock", null), 
                                                        mat4.Identity), 
                                                    true);
        }

        public void Dig(CsgNode shape, BoundingSphere shapeBoundary, DigMode digMode = DigMode.Substract)
        {
            CsgNode digShape = null;

            // constraintDiffNode performs the constraint as a CSG operation 
            // by cutting away anything of thge digging shape not inside the allowed area.
            CsgOpDiff constraintDiffNode = new CsgOpDiff();
            // Assemble difference operation by applying all drone constraints.
            player.AddDroneConstraints(constraintDiffNode, shapeBoundary.Center);

            // We apply the constraint by substracting it from the given shape.
            CsgOpConcat constraintedShape = new CsgOpConcat();
            constraintedShape.AddNode(shape);
            constraintedShape.AddNode(constraintDiffNode);
            digShape = constraintedShape;

            CsgNode digNode = null;
            // Depending on the digging mode, we either add or substract the digging shape from the terrain.
            if(digMode == DigMode.Substract) {
                digNode = new CsgOpDiff(digShape);
            } else {
                digNode = new CsgOpUnion(digShape);
            }

            // Callback for statistical purposes.
            CsgStatCallback finalNode = new CsgStatCallback(digNode, 4, 4);
            finalNode.AddSimpleVolumeCallback("UpvoidMiner", HostScript.ModDomain, "UpvoidMiner.DiggingController", "StatCallback");
            finalNode.AddVolumeChangePointCallback("UpvoidMiner", HostScript.ModDomain, "UpvoidMiner.DiggingController", "PointCallback");

            world.Terrain.ModifyTerrain(shapeBoundary, finalNode);
        }

        public void DigSphere(vec3 position, float radius, int terrainMaterialId = 1, DigMode digMode = DigMode.Substract)
        {
            sphereNode.MaterialIndex = terrainMaterialId;
            sphereNode.SetParameterFloat("sphereRadius", radius);
            sphereNode.SetParameterVec3("spherePosition", position);

            Dig(sphereNode, new BoundingSphere(position, radius*1.25f), digMode);
        }

        /// <summary>
        /// This callback is called once per changed material in a chunk and reports the amount of volume changed (in m^3).
        /// </summary>
        public static void StatCallback(int mat, float volume, int lod)
        {
            if ( mat != 0 )
            {
                // Resolve terrain material.
                TerrainMaterial material = instance.world.Terrain.QueryMaterialFromIndex(mat);
                Debug.Assert(material != null, "Invalid terrain material");

                // Add proper amount of material to player inventory.
                // If the material changed by a negative volume we want to collect a positive amount.
                instance.player.Inventory.AddResource(material, -volume);
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
            if ( matPrev != 0 && matNow == 0 )
                instance.particlesStones.AddParticle3D(new vec3(x, y, z) + RandomDir() * (float)random.NextDouble() * .3f,
                                                       RandomDir() * (float)random.NextDouble() * .4f,
                                                       new vec4(1),
                                                       .2f + (float)random.NextDouble() * .3f,
                                                       .2f + (float)random.NextDouble() * .3f,
                                                       RandomDir(),
                                                       RandomDir(),
                                                       new vec3(0));
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

