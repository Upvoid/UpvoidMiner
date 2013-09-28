using System;

using Engine;
using Engine.Universe;
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

        public enum ConstraintMode
        {
            InsideAllowed,
            OutsideAllowed
        }

        World world;

        CsgNode constraintShape = null;
        BoundingSphere constraintBoundary = new BoundingSphere(vec3.Zero, 0f);
        ConstraintMode constraintMode = ConstraintMode.InsideAllowed;

        CsgExpression sphereNode;

        public DiggingController(World world)
        {
            this.world = world;
            string sphereExpression = "-sphereRadius + distance(vec3(x,y,z), spherePosition)";
            sphereNode = new CsgExpression(1, sphereExpression, HostScript.ModDomain, "sphereRadius:float, spherePosition:vec3");
        }

        public void SetConstraint(CsgNode shape, BoundingSphere boundary, ConstraintMode mode)
        {
            constraintShape = shape;
            constraintBoundary = boundary;
            constraintMode = mode;
        }

        public void Dig(CsgNode shape, BoundingSphere shapeBoundary, DigMode digMode = DigMode.Substract)
        {
            CsgNode digShape = null;
            if(constraintShape == null) {
                // When no constraint is set, we simply use the given shape for digging.
                digShape = shape;
            } else {

                // constraintDiffNode performs the constraint as a CSG operation 
                // by cutting away anything of thge digging shape not inside the allowed area.
                CsgNode constraintDiffNode = null;
                if(constraintMode == ConstraintMode.OutsideAllowed) {
                    // When the constraint is in outside mode, we cut away everything inside the constraint shape
                    constraintDiffNode = new CsgOpDiff(constraintShape);
                } else {
                    // When in outside mode, we want to cut away everything on the outside.
                    // We get the outside by computing (1 - constraintShape), which translates to these CSG nodes.
                    CsgOpConcat invertedConstraint = new CsgOpConcat();
					invertedConstraint.AddNode(new CsgExpression(1, "-1", HostScript.ModDomain));
                    invertedConstraint.AddNode(new CsgOpDiff(constraintShape));

                    constraintDiffNode = new CsgOpDiff(invertedConstraint);
                }

                // We apply the constraint by substracting it from the given shape.
                CsgOpConcat constraintedShape = new CsgOpConcat();
                constraintedShape.AddNode(shape);
                constraintedShape.AddNode(constraintDiffNode);
                digShape = constraintedShape;
            }

            CsgNode digNode = null;
            // Depending on the digging mode, we either add or substract the digging shape from the terrain.
            if(digMode == DigMode.Substract) {
                digNode = new CsgOpDiff(digShape);
            } else {
                digNode = new CsgOpUnion(digShape);
            }

            world.Terrain.ModifyTerrain(shapeBoundary, digNode);
        }

        public void DigSphere(vec3 position, float radius, int terrainMaterialId = 1, DigMode digMode = DigMode.Substract)
        {
            sphereNode.SetParameterFloat("sphereRadius", radius);
            sphereNode.SetParameterVec3("spherePosition", position);

            Dig(sphereNode, new BoundingSphere(position, radius*1.25f), digMode);
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

