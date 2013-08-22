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

		// Define an area the user can NOT dig in. Note that the expression defines everything but the are we are allowed to dig in. 
		const float halfCubeSideLength = 10.0f;
		CsgExpression inverseCube = new CsgExpression(1, "-(max(max(abs(x), abs(y)), abs(z)) - " + halfCubeSideLength.ToString() + ")");

		// Create a pointer to an area we are allowed to dig in.
		MeshRenderJob diggingConstraints = null;

		public Player(GenericCamera _camera)
		{
			camera = _camera;
            Input.OnPressInput += HandlePressInput;
		}

        void HandlePressInput (object sender, InputPressArgs e)
        {
            // We don't have tools or items yet, so we hard-code digging on left mouse click here.
			if((e.Key == InputKey.MouseLeft || e.Key == InputKey.MouseMiddle) && e.PressType == InputPressArgs.KeyPressType.Down) {

                // Send a ray query to find the position on the terrain we are looking at.
                ContainingWorld.Physics.RayQuery(thisEntity.Position + camera.ForwardDirection * 0.5f, thisEntity.Position + camera.ForwardDirection * 20f, delegate(bool _hit, vec3 _position, vec3 _normal, RigidBody _body, bool _hasTerrainCollision) {
                    // Receiving the async ray query result here
                    if(_hit)
                    {
						// Set the actual shape to be dug or built.
						CsgExpression sphereShape = new CsgExpression(1, "-1.5 + sqrt(distance2(vec3(x,y,z), vec3"+_position.ToString()+"))");

						// Concatenate this sphere shape and the diff-operation that we do not allow digging in.
						CsgOpConcat concatenator = new CsgOpConcat();
						concatenator.AddNode(sphereShape);
						concatenator.AddNode(new CsgOpDiff(inverseCube));

						// Distinguish between middle mouse button (build) and left mouse button (dig).
						if(e.Key == InputKey.MouseMiddle)
						{
							// Shape is a union, so we add something.
							CsgOpUnion digShape = new CsgOpUnion(concatenator);

	                        // Apply that union operation to the terrain -> build.
							ContainingWorld.Terrain.ModifyTerrain(new BoundingSphere(_position, 4), digShape);
						}
						else
						{
							// Shape is a diff, so we remove something.
							CsgOpDiff digShape = new CsgOpDiff(concatenator);

							// Apply that diff operation to the terrain -> dig.
							ContainingWorld.Terrain.ModifyTerrain(new BoundingSphere(_position, 4), digShape);
						}
                    }
                });
            }
        }

		protected override void Init()
		{
			// For now, attach this entity to a simple sphere physics object.
			physicsComponent = new PhysicsComponent(thisEntity,
                                 ContainingWorld.Physics.CreateAndAddRigidBody(0f, mat4.Identity, new CapsuleShape(0.3f, 1.5f)),
			                     mat4.Translate(new vec3(0, 1.5f, 0)));

			// Create a character controller that allows us to walk around.
			controller = new CharacterController(physicsComponent.RigidBody, camera, ContainingWorld, thisEntity.Position);

			// Make the area we are allowed to dig in visible
			diggingConstraints = new MeshRenderJob(
				Renderer.Opaque.Mesh, 
				Resources.UseMaterial("DiggingConstraints", LocalScript.ModDomain), 
				Resources.UseMesh("::Debug/Box", LocalScript.ModDomain),
				mat4.Scale(0.999f * halfCubeSideLength)); // avoid z-fighting

			// Add this RenderJob to the world's jobs
			ContainingWorld.AddRenderJob(diggingConstraints);
		}

	}
}

