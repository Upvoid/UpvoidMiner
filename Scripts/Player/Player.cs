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
using System.Collections.Generic;
using System.Diagnostics;
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
		/// The render component for the torso.
		/// </summary>
		private RenderComponent renderComponentTorso;
		/// <summary>
		/// The render component for the torso (shadow pass).
		/// </summary>
		private RenderComponent renderComponentTorsoShadow;

		/// <summary>
		/// This is the camera that is used to show the perspective of the player.
		/// </summary>
		GenericCamera camera;

		/// <summary>
		/// This takes control of the rigid body attached to this entity and lets us walk around.
		/// </summary>
		CharacterController character;

        DiggingController digging;

        PlayerGui gui;

		// Radius of digging/building sphere
		float diggingSphereRadius = 1.0f;

        /// <summary>
        /// A list of items representing the inventory of the player.
        /// </summary>
        public List<Item> inventory = new List<Item>();

        /// <summary>
        /// List of drones of the player.
        /// </summary>
        public List<Drone> Drones = new List<Drone>();
        /// <summary>
        /// List of drone constraints that are currently active.
        /// </summary>
        public List<DroneConstraint> DroneConstraints = new List<DroneConstraint>();

        /// <summary>
        /// Position of the player.
        /// </summary>
        public vec3 Position
        {
            get { return character.Position; }
        }

		public Player(GenericCamera _camera)
		{
			camera = _camera;
            Input.OnPressInput += HandlePressInput;
			Input.OnAxisInput += HandleAxisInput;
		}

        public void Update(float elapsedSeconds)
        {
            foreach (var drone in Drones)
                drone.Update(elapsedSeconds);
            foreach (var dc in DroneConstraints)
                dc.Update(elapsedSeconds);
        }

        /// <summary>
        /// Adds all active drone constraints to a Csg Diff Node.
        /// </summary>
        public void AddDroneConstraints(CsgOpDiff diffNode, vec3 refPos)
        {
            foreach (var dc in DroneConstraints)
                dc.AddCsgConstraints(diffNode, refPos);
        }

        protected override void Init()
		{
            // Create a character controller that allows us to walk around.
            character = new CharacterController(camera, ContainingWorld);

			// For now, attach this entity to a simple sphere physics object.
			physicsComponent = new PhysicsComponent(thisEntity,
                                 character.Body,
			                     mat4.Translate(new vec3(0, 0.6f, 0)));

            physicsComponent.RigidBody.SetTransformation(mat4.Translate(new vec3(0, 30f, 0)));

			// Add Torso mesh.
			renderComponentTorso = new RenderComponent(thisEntity,
			                                           mat4.Scale(2f) * mat4.Translate(new vec3(0, -.5f, 0)),
			                                           new MeshRenderJob(Renderer.Opaque.Mesh, Resources.UseMaterial("Miner/Torso", HostScript.ModDomain), Resources.UseMesh("Miner/Torso", HostScript.ModDomain), mat4.Identity),
			                                           true);
			renderComponentTorsoShadow = new RenderComponent(thisEntity,
			                                           renderComponentTorso.Transform,
			                                           new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", HostScript.ModDomain), Resources.UseMesh("Miner/Torso", HostScript.ModDomain), mat4.Identity),
			                                           true);

            // This digging controller will perform digging and handle digging constraints for us.
            digging = new DiggingController(ContainingWorld, this);

            gui = new PlayerGui(this);

            AddTriggerSlot("AddItem");
		}

		void HandleAxisInput (object sender, InputAxisArgs e)
		{
			if(e.Axis == AxisType.MouseWheelY) 
			{
				diggingSphereRadius = Math.Max(1.0f, diggingSphereRadius + e.RelativeChange);
			}
		}

        void AddDrone(vec3 position)
        {
            Drone d = new Drone(position + new vec3(0, 1, 0), this, DroneType.Chain);
            Drones.Add(d);
            
            bool foundConstraint = false;
            foreach (var dc in DroneConstraints)
            {
                if ( dc.IsAddable(d) )
                {
                    dc.AddDrone(d);
                    foundConstraint = true;
                    break;
                }
            }
            if ( !foundConstraint )
                DroneConstraints.Add(new DroneConstraint(d));
            
            LocalScript.world.AddEntity(d, mat4.Translate(d.CurrentPosition));
        }

        void HandlePressInput (object sender, InputPressArgs e)
        {

            // Scale the area using + and - keys.
            // Translate it using up down left right (x, z)
            // and PageUp PageDown (y).
            if(e.PressType == InputPressArgs.KeyPressType.Down) {

				if ( e.Key == InputKey.F8 )
					Renderer.Opaque.Mesh.DebugWireframe = !Renderer.Opaque.Mesh.DebugWireframe;

            }

            // We don't have tools or items yet, so we hard-code digging on left mouse click here.
            if((e.Key == InputKey.MouseLeft || e.Key == InputKey.MouseMiddle || e.Key == InputKey.C || e.Key == InputKey.V) && e.PressType == InputPressArgs.KeyPressType.Down) {

                // Send a ray query to find the position on the terrain we are looking at.
                ContainingWorld.Physics.RayQuery(camera.Position + camera.ForwardDirection * 0.5f, camera.Position + camera.ForwardDirection * 200f, delegate(bool _hit, vec3 _position, vec3 _normal, RigidBody _body, bool _hasTerrainCollision) {
                    // Receiving the async ray query result here
                    if(_hit)
                    {
                        if (e.Key == InputKey.MouseLeft)
                        {
							digging.DigSphere(_position, diggingSphereRadius);
                        } else if (e.Key == InputKey.MouseMiddle) {
							digging.DigSphere(_position, diggingSphereRadius, 1, DiggingController.DigMode.Add);
                        }

                    }
                });
            }
            
            if(e.Key == InputKey.E && e.PressType == InputPressArgs.KeyPressType.Down) {
                ContainingWorld.Physics.RayQuery(camera.Position + camera.ForwardDirection * 0.5f, camera.Position + camera.ForwardDirection * 200f, delegate(bool _hit, vec3 _position, vec3 _normal, RigidBody _body, bool _hasTerrainCollision) {
                    // Receiving the async ray query result here
                    if(_body != null)
                    {
                        Entity entity = _body.AttachedEntity;
                        if(entity != null)
                        {
                            TriggerId trigger = TriggerId.getIdByName("Interaction");
                            entity[trigger] |= new InteractionMessage(thisEntity);
                        }
                    }
                });
            }
            
            if(e.Key == InputKey.T && e.PressType == InputPressArgs.KeyPressType.Down) {
                ContainingWorld.Physics.RayQuery(camera.Position + camera.ForwardDirection * 0.5f, camera.Position + camera.ForwardDirection * 200f, delegate(bool _hit, vec3 _position, vec3 _normal, RigidBody _body, bool _hasTerrainCollision) {
                    // Receiving the async ray query result here
                    if(_hit)
                    {
                        AddDrone(_position);
                    }
                });
            }

        }

        /// <summary>
        /// This trigger slot is for sending an item to the receiving entity, which usually will add it to its inventory.
        /// This is triggered as a response to the Interaction trigger by items, but can be used whenever you want to give an item to a character.
        /// </summary>
        /// <param name="msg">Expected to be a of type PickupResponseMessage.</param>
        public void AddItem(object msg)
        {
            // Make sure we get the message type we are expecting.
            AddItemMessage addItemMsg = msg as AddItemMessage;
            if(addItemMsg == null)
                return;

            // Add the received item to the inventory.
            inventory.Add(addItemMsg.PickedItem);

        }

	}
}

