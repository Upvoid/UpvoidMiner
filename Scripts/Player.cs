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

		public Player(GenericCamera _camera)
		{
			camera = _camera;
            Input.OnPressInput += HandlePressInput;
		}

        void HandlePressInput (object sender, InputPressArgs e)
        {
            // We don't have tools or items yet, so we hard-code digging on left mouse click here.
            if(e.Key == InputKey.MouseLeft && e.PressType == InputPressArgs.KeyPressType.Up) {

                // Send a ray query to find the position on the terrain we are looking at.
                ContainingWorld.Physics.RayQuery(thisEntity.Position + camera.ForwardDirection * 0.5f, thisEntity.Position + camera.ForwardDirection * 20f, delegate(bool _hit, vec3 _position, vec3 _normal, RigidBody _body) {
                    // Receiving the async ray query result here
                    if(_hit)
                    {
                        // Create a CsgNode that defines the digging shape as a diff operation.
                        CsgNode digShape = new CsgOpDiff(new CsgExpression(1, "-1.5 + sqrt(distance2(vec3(x,y,z), vec3"+_position.ToString()+"))"));

                        // Apply that diff operation to the terrain.
                        ContainingWorld.Terrain.ModifyTerrain(new BoundingSphere(_position, 2), digShape);
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
		}

	}
}

