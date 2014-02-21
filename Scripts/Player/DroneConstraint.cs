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
        /// <summary>
        /// Initializes a new instance of the <see cref="UpvoidMiner.DroneConstraint"/> class.
        /// A drone constraint creates a digging/construction constraint between two nearby drones.
        /// </summary>
        /// <param name="_firstDrone">First drone, initially added to this constraint.</param>
        public DroneConstraint(Drone _firstDrone)
        {
            drones.Add(_firstDrone);
        }

        
        /// <summary>
        /// Reports whether drones are attached to this constraint.
        /// </summary>
        /// <returns><c>true</c>, iff at least one drone is attached, <c>false</c> otherwise.</returns>
        public bool ContainsDrones()
        {
            return drones.Count > 0;
        }


        /// <summary>
        /// Determines whether the drone is addable to this instance.
        /// True iff constraint contains a drone of the same type.
        /// </summary>
        public bool IsAddable(Drone _drone)
        {
            //if (drones.Count < 1)
            //    return false;

            Drone refDrone = ReferenceDrone;

            // Matching drone type.
            if (refDrone.Type != _drone.Type)
                return false;

            return true;
        }

        /// <summary>
        /// Removes a drone from the constraint.
        /// </summary>
        public void RemoveDrone(Drone drone)
        {
            drones.Remove(drone);
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
        private void configureVerticalConstraint(Drone first, Drone second, MeshRenderJob job1, MeshRenderJob job2)
        {
            vec3 startPos = first.CurrentPosition;
            vec3 endPos = second.CurrentPosition;

            vec3 up = new vec3(0, 14 * 2 * 100, 0);
            vec3 dir = endPos - startPos;

            // This transforms x from start to end and y from -up to up.
            mat4 transform = new mat4(
                dir,
                up,
                new vec3(0,0,1),
                new vec3()
                );

            job1.ModelMatrix = job2.ModelMatrix = mat4.Translate(startPos) * transform * mat4.Scale(.5f) * mat4.Translate(new vec3(1, 0, 0));
        }

        /// <summary>
        /// Updates this constraint, i.e. renderjobs.
        /// </summary>
        public void Update(float _elapsedSeconds)
        {
            if (drones.Count > 0)
            {

                Drone refDrone = ReferenceDrone;

                switch (refDrone.Type)
                {
                    case DroneType.Chain:
                        for (int i = 0; i < drones.Count - 1; ++i)
                        {
                            Drone first = drones[i];
                            Drone second = drones[i + 1];

                            // Swap every other drone.
                            if (i % 2 == 1)
                            {
                                Drone tmp = first;
                                first = second;
                                second = tmp;
                            }

                            bool addJob = false;
                            if (boundaryIndicators.Count <= i)
                            {
                                boundaryIndicators.Add(new MeshRenderJob(Renderer.Transparent.Mesh, 
                                                                         Resources.UseMaterial("Miner/DroneConstraintVertical", UpvoidMiner.ModDomain),
                                                                         Resources.UseMesh("::Debug/Quad", UpvoidMiner.ModDomain),
                                                                         mat4.Identity));
                                boundaryIndicatorsDistort.Add(new MeshRenderJob(Renderer.Distortion.Mesh, 
                                                                                Resources.UseMaterial("Miner/DroneConstraintVerticalDistort", UpvoidMiner.ModDomain),
                                                                                Resources.UseMesh("::Debug/Quad", UpvoidMiner.ModDomain),
                                                                                mat4.Identity));

                                // Vertical drones cause a constraint by the intersection of the planes (i.e. the plane between two drones and the two shadow-planes).
                                constraintExpression.Add(new CsgExpression(1, "max((dot(plane1Normal, vec3(x, y, z)) - plane1Dis), max( (dot(plane2Normal, vec3(x, y, z)) - plane2Dis), (dot(plane3Normal, vec3(x, y, z)) - plane3Dis)) )", 
                                                                           UpvoidMiner.ModDomain, 
                                                                           "plane1Normal:vec3, plane1Dis:float, plane2Normal:vec3, plane2Dis:float, plane3Normal:vec3, plane3Dis:float"));
                                addJob = true;
                            }
                            
                            MeshRenderJob job1 = boundaryIndicators[i];
                            MeshRenderJob job2 = boundaryIndicatorsDistort[i];

                            configureVerticalConstraint(first, second, job1, job2);

                            if (addJob)
                            {
                                LocalScript.world.AddRenderJob(job1);
                                LocalScript.world.AddRenderJob(job2);
                            }
                        }

                        
                        break;
                    default:
                        Debug.Fail("Not implemented/Invalid");
                        break;
                }
            }
            // Remove old ones.
            while ( boundaryIndicators.Count > Math.Max(0, drones.Count -1))
            {
                LocalScript.world.RemoveRenderJob(boundaryIndicators[boundaryIndicators.Count - 1]);
                LocalScript.world.RemoveRenderJob(boundaryIndicatorsDistort[boundaryIndicators.Count - 1]);
                boundaryIndicators.RemoveAt(boundaryIndicators.Count - 1);
                boundaryIndicatorsDistort.RemoveAt(boundaryIndicatorsDistort.Count - 1);
                constraintExpression.RemoveAt(constraintExpression.Count - 1);
            }
        }

        /// <summary>
        /// Configure the CSG Expression of a vertical constraint (including 'shadow').
        /// </summary>
        private void configureVerticalConstraintExpression(vec3 refPos, Drone first, Drone second, CsgExpression expr)
        {
            // Calculate primary plane.
            vec3 start = first.CurrentPosition;
            vec3 end = second.CurrentPosition;
            vec3 mid = (start + end) / 2f;

            vec3 up = new vec3(0, 1, 0);
            vec3 dir = end - start;
            vec3 normal = vec3.cross(dir, up);

            vec3 d = refPos - mid;
            bool invert = vec3.dot(d, normal) < 0;
            if ( invert ) normal *= -1f;
            
            expr.SetParameterVec3("plane1Normal", normal);
            expr.SetParameterFloat("plane1Dis", vec3.dot(normal, mid));

            // Caluclate first shadow plane
            vec3 s1Pos = start;
            vec3 s1Normal = vec3.cross(up, s1Pos - refPos);
            if ( invert ) s1Normal *= -1f;
            
            expr.SetParameterVec3("plane2Normal", s1Normal);
            expr.SetParameterFloat("plane2Dis", vec3.dot(s1Normal, s1Pos));

            // Caluclate second shadow plane
            vec3 s2Pos = end;
            vec3 s2Normal = vec3.cross(s2Pos - refPos, up);
            if ( invert ) s2Normal *= -1f;
            
            expr.SetParameterVec3("plane3Normal", s2Normal);
            expr.SetParameterFloat("plane3Dis", vec3.dot(s2Normal, s2Pos));
        }

        /// <summary>
        /// Adds all csg constraints to a diff node.
        /// </summary>
        public void AddCsgConstraints(CsgOpDiff diffNode, vec3 refPos)
        {            

            Drone refDrone = ReferenceDrone;
            switch (refDrone.Type)
            {
                case DroneType.Chain:
                    for (int i = 0; i < drones.Count - 1; ++i)
                    {
                        Drone first = drones[i];
                        Drone second = drones[i+1];
                        CsgExpression expr = constraintExpression[i];

                        configureVerticalConstraintExpression(refPos, first, second, expr);

                        diffNode.AddNode(expr);
                    }
                    break;
                default: Debug.Fail("Not implemented/Invalid");
                    break;
            }
        }
    }
}

