using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using Engine.Universe;
using Engine.Rendering;
using Engine.Resources;

namespace UpvoidMiner
{
    /// <summary>
    /// Multiple drones of the same type and 'color' form a drone constraint.
    /// </summary>
    public class DroneConstraint
    {
        /// <summary>
        /// List of participating drones.
        /// </summary>
        private List<Drone> drones = new List<Drone>();

        /// <summary>
        /// List of renderjobs for boundary indicators.
        /// </summary>
        private List<MeshRenderJob> boundaryIndicators = new List<MeshRenderJob>();
        private List<MeshRenderJob> boundaryIndicatorsDistort = new List<MeshRenderJob>();

        /// <summary>
        /// List of expressions used for realizing boundaries
        /// </summary>
        private List<CsgExpression> constraintExpression = new List<CsgExpression>();

        /// <summary>
        /// First drone is reference drone.
        /// </summary>
        public Drone ReferenceDrone
        {
            get
            {
                Debug.Assert(drones.Count > 0);
                return drones[0];
            }
        }

        public DroneConstraint(Drone _firstDrone)
        {
            drones.Add(_firstDrone);
        }

        /// <summary>
        /// Determines whether the drone is addable this istance.
        /// </summary>
        public bool IsAddable(Drone _drone)
        {
            Drone refDrone = ReferenceDrone;

            // Matching drone type.
            if (refDrone.Type != _drone.Type)
                return false;

            return true;
        }

        /// <summary>
        /// Adds the drone to this constraint.
        /// Drone must be addable (check with IsAddable).
        /// </summary>
        public void AddDrone(Drone _drone)
        {
            Debug.Assert(IsAddable(_drone));

            drones.Add(_drone);
        }

        /// <summary>
        /// Configures a renderjob for a vertical constraint between two drones.
        /// </summary>
        private void configureVerticalConstraint(Drone first, Drone second, MeshRenderJob job1, MeshRenderJob job2, CsgExpression expr)
        {
            vec3 startPos = first.CurrentPosition;
            vec3 endPos = second.CurrentPosition;

            vec3 up = new vec3(0, 14, 0);
            vec3 dir = endPos - startPos;

            // This transforms x from start to end and y from -up to up.
            mat4 transform = new mat4(
                dir,
                up,
                new vec3(0,0,1),
                new vec3()
                );

            job1.ModelMatrix = job2.ModelMatrix = mat4.Translate(startPos) * transform * mat4.Scale(.5f) * mat4.Translate(new vec3(1, 0, 0));

            // Configure expression.
            expr.SetParameterVec3("planePos", startPos);
            expr.SetParameterVec3("planeNormal", vec3.cross(dir, up));
        }

        /// <summary>
        /// Updates this constraint, i.e. renderjobs.
        /// </summary>
        public void Update(float _elapsedSeconds)
        {
            Drone refDrone = ReferenceDrone;

            switch (refDrone.Type)
            {
                case DroneType.Chain:
                    for (int i = 0; i < drones.Count - 1; ++i)
                    {
                        Drone first = drones[i];
                        Drone second = drones[i+1];

                        bool addJob = false;
                        if ( boundaryIndicators.Count <= i )
                        {
                            boundaryIndicators.Add(new MeshRenderJob(Renderer.Transparent.Mesh, 
                                                                     Resources.UseMaterial("Miner/DroneConstraintVertical", LocalScript.ModDomain),
                                                                     Resources.UseMesh("::Debug/Quad", LocalScript.ModDomain),
                                                                     mat4.Identity));
                            boundaryIndicatorsDistort.Add(new MeshRenderJob(Renderer.Distortion.Mesh, 
                                                                     Resources.UseMaterial("Miner/DroneConstraintVerticalDistort", LocalScript.ModDomain),
                                                                     Resources.UseMesh("::Debug/Quad", LocalScript.ModDomain),
                                                                     mat4.Identity));
                            constraintExpression.Add(new CsgExpression(1, "dot(planeNormal, (planePos - vec3(x, y, z))) * invert", LocalScript.ModDomain, "planeNormal:vec3, planePos:vec3, invert:float"));
                            addJob = true;
                        }
                        
                        MeshRenderJob job1 = boundaryIndicators[i];
                        MeshRenderJob job2 = boundaryIndicatorsDistort[i];

                        configureVerticalConstraint(first, second, job1, job2, constraintExpression[i]);

                        if ( addJob )
                        {
                            LocalScript.world.AddRenderJob(job1);
                            LocalScript.world.AddRenderJob(job2);
                        }
                    }
                    break;
                default: Debug.Fail("Not implemented/Invalid");
                    break;
            }
        }

        /// <summary>
        /// Checks if a vertical constraint requires inversion
        /// </summary>
        private void checkVerticalConstraintInversion(vec3 refPos, Drone first, Drone second, ref float proInvert, ref float conInvert)
        {
            vec3 start = first.CurrentPosition;
            vec3 end = second.CurrentPosition;

            vec3 up = new vec3(0, 1, 0);
            vec3 dir = end - start;
            vec3 normal = vec3.cross(dir, up);

            vec3 d = refPos - start;
            bool inside = vec3.dot(d, normal) > 0;
            
            float dis1 = vec2.distance(new vec2(start.x, start.z), new vec2(refPos.x, refPos.z));
            float dis2 = vec2.distance(new vec2(end.x, end.z), new vec2(end.x, end.z));
            float confidence = 1f / (1f + dis1) + 1f / (1f + dis2);

            if (inside)
                conInvert += confidence;
            else
                proInvert += confidence;
        }

        /// <summary>
        /// Adds all csg constraints to a diff node.
        /// </summary>
        public void AddCsgConstraints(CsgOpDiff diffNode, vec3 refPos)
        {
            /// Check if csg constraints require inversion.
            float proInvert = 0;
            float conInvert = 0;
            
            Drone refDrone = ReferenceDrone;
            switch (refDrone.Type)
            {
                case DroneType.Chain:
                    for (int i = 0; i < drones.Count - 1; ++i)
                    {
                        Drone first = drones[i];
                        Drone second = drones[i+1];

                        checkVerticalConstraintInversion(refPos, first, second, ref proInvert, ref conInvert);
                    }
                    break;
                default: Debug.Fail("Not implemented/Invalid");
                    break;
            }

            foreach (var expr in constraintExpression)
            {
                expr.SetParameterFloat("invert", proInvert > conInvert ? 1f : -1f);
                diffNode.AddNode(expr);
            }
        }
    }
}

