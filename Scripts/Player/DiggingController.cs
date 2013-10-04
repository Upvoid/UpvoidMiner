using System;

using Engine;
using Engine.Universe;
using Engine.Resources;
using Engine.Rendering;
using Engine.Physics;

namespace UpvoidMiner
{
    public class DiggingController
    {
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

        public DiggingController(World world, Player player)
        {
            this.world = world;
            this.player = player;
            string sphereExpression = "-sphereRadius + distance(vec3(x,y,z), spherePosition)";
            sphereNode = new CsgExpression(1, sphereExpression, HostScript.ModDomain, "sphereRadius:float, spherePosition:vec3");
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
            digNode = new CsgStatCallback("UpvoidMiner", HostScript.ModDomain, "UpvoidMiner.DiggingController", "StatCallback", digNode, 4, 4);

            world.Terrain.ModifyTerrain(shapeBoundary, digNode);
        }

        public void DigSphere(vec3 position, float radius, int terrainMaterialId = 1, DigMode digMode = DigMode.Substract)
        {
            sphereNode.SetParameterFloat("sphereRadius", radius);
            sphereNode.SetParameterVec3("spherePosition", position);

            Dig(sphereNode, new BoundingSphere(position, radius*1.25f), digMode);
        }

        public static void StatCallback(int mat, float volume, int lod)
        {
            Console.WriteLine("Mat " + mat + " changed by " + volume + " m^3");
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

