/*
 *    Copyright (C) by Upvoid Studios
 *
 *    This program is free software: you can redistribute it and/or modify
 *    it under the terms of the GNU General Public License as published by
 *    the Free Software Foundation, either version 3 of the License, or
 *    (at your option) any later version.
 *
 *    This program is distributed in the hope that it will be useful,
 *    but WITHOUT ANY WARRANTY; without even the implied warranty of
 *    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *    GNU General Public License for more details.
 *
 *    You should have received a copy of the GNU General Public License
 *    along with this program.  If not, see <http://www.gnu.org/licenses/>
 */

using System;
using Engine;
using Engine.Universe;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Rendering;
using Engine.Physics;
using Engine.Input;

namespace UpvoidMiner
{
	/// <summary>
	/// Contains the game logic and the internal state of the player character.
	/// </summary>
	public class Player: EntityScript
	{
		/// <summary>
		/// The physical representation of the player. For now, this is a simple uncontrollable sphere.
		/// </summary>
		private PhysicsComponent physicsComponent;

		/// <summary>
		/// This is the camera that is used to show the perspective of the player.
		/// </summary>
		GenericCamera camera;

		/// <summary>
		/// This takes control of the rigid body attached to this entity and lets us walk around.
		/// </summary>
		CharacterController controller;

        DiggingController digging;

		// Define an area cube the user can NOT dig in.
		float halfCubeSideLength = 10f;

		// Position where this cube is located.
		vec3 currentAreaPosition = new vec3(0, 0, 0);

		// Create a pointer to a renderjob visualizing the area we are allowed to dig in.
		MeshRenderJob diggingConstraints = null;

		public Player(GenericCamera _camera)
		{
			camera = _camera;
            Input.OnPressInput += HandlePressInput;
		}

        protected override void Init()
		{
			// For now, attach this entity to a simple sphere physics object.
			physicsComponent = new PhysicsComponent(thisEntity,
                                 ContainingWorld.Physics.CreateAndAddRigidBody(70f, mat4.Identity, new CapsuleShape(0.3f, 1.5f)),
			                     mat4.Translate(new vec3(0, 1.5f, 0)));

            physicsComponent.RigidBody.SetTransformation(mat4.Translate(new vec3(0, 30f, 0)));

            // Create a character controller that allows us to walk around.
            controller = new CharacterController(physicsComponent.RigidBody, camera, ContainingWorld);

            // This digging controller will perform digging and handle digging constraints for us.
            digging = new DiggingController(ContainingWorld);

            // Make the area we are allowed to dig in visible
            diggingConstraints = new MeshRenderJob(
                Renderer.Opaque.Mesh, 
                Resources.UseMaterial("DiggingConstraints", LocalScript.ModDomain), 
                Resources.UseMesh("::Debug/Box", LocalScript.ModDomain),
                mat4.Scale(0.999f * halfCubeSideLength)); // avoid z-fighting

            // Add this RenderJob to the world's jobs
            ContainingWorld.AddRenderJob(diggingConstraints);
		}

        void HandlePressInput (object sender, InputPressArgs e)
        {

            // Scale the area using + and - keys.
            // Translate it using up down left right (x, z)
            // and PageUp PageDown (y).
            if(e.PressType == InputPressArgs.KeyPressType.Down) {
                if(e.Key == InputKey.Plus) {
                    halfCubeSideLength += 0.5f;
                } else if(e.Key == InputKey.Minus) {
                    halfCubeSideLength -= 0.5f;
                } else if(e.Key == InputKey.Up) {
                    currentAreaPosition.z += 0.5f;
                } else if(e.Key == InputKey.Down) {
                    currentAreaPosition.z -= 0.5f;
                } else if(e.Key == InputKey.Left) {
                    currentAreaPosition.x += 0.5f;
                } else if(e.Key == InputKey.Right) {
                    currentAreaPosition.x -= 0.5f;
                } else if(e.Key == InputKey.PageUp) {
                    currentAreaPosition.y += 0.5f;
                } else if(e.Key == InputKey.PageDown) {
                    currentAreaPosition.y -= 0.5f;
                } else if(e.Key == InputKey.O) {
                    CsgExpression cube = new CsgExpression(1, "(max(max(abs(x - " + currentAreaPosition.x.ToString() + "), abs(y - " + currentAreaPosition.y.ToString() + ")), abs(z - " + currentAreaPosition.z.ToString() + ")) - " + halfCubeSideLength.ToString() + ")");
                    digging.SetConstraint(cube, new BoundingSphere(currentAreaPosition, halfCubeSideLength * 2f), DiggingController.ConstraintMode.OutsideAllowed);
                } else if(e.Key == InputKey.I) {
                    CsgExpression cube = new CsgExpression(1, "(max(max(abs(x - " + currentAreaPosition.x.ToString() + "), abs(y - " + currentAreaPosition.y.ToString() + ")), abs(z - " + currentAreaPosition.z.ToString() + ")) - " + halfCubeSideLength.ToString() + ")");
                    digging.SetConstraint(cube, new BoundingSphere(currentAreaPosition, halfCubeSideLength * 2f), DiggingController.ConstraintMode.InsideAllowed);
                }
            }

            // Set the new modelmatrix.
            diggingConstraints.ModelMatrix = mat4.Translate(currentAreaPosition) * mat4.Scale(0.999f * halfCubeSideLength);

            // We don't have tools or items yet, so we hard-code digging on left mouse click here.
            if((e.Key == InputKey.MouseLeft || e.Key == InputKey.MouseMiddle || e.Key == InputKey.C || e.Key == InputKey.V) && e.PressType == InputPressArgs.KeyPressType.Down) {

                // Send a ray query to find the position on the terrain we are looking at.
                ContainingWorld.Physics.RayQuery(camera.Position + camera.ForwardDirection * 0.5f, camera.Position + camera.ForwardDirection * 200f, delegate(bool _hit, vec3 _position, vec3 _normal, RigidBody _body, bool _hasTerrainCollision) {
                    // Receiving the async ray query result here
                    if(_hit)
                    {
                        // There is currently a bug in the physics system that returns NaNs in some cases.
                        if(!_position.IsFinite)
                            return;

                        if (e.Key == InputKey.MouseLeft)
                        {
                            digging.DigSphere(_position, 1.5f);
                        } else if (e.Key == InputKey.MouseMiddle) {
                            digging.DigSphere(_position, 1.5f, 1, DiggingController.DigMode.Add);
                        }

                    }
                });
            }
        }

	}
}

