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
		private RenderComponent rcTorso, rcTorsoShadow;
        private RenderComponent rcTorsoSteam;
        private CpuParticleSystemBase psTorsoSteam;
        private CpuParticleComponent pcTorsoSteam;
        private mat4 torsoSteamOffset = mat4.Translate(new vec3(.13090f, .53312f, -.14736f));
        /// <summary>
        /// Relative torso transformation.
        /// </summary>
        private mat4 torsoTransform = mat4.Scale(2f) * mat4.Translate(new vec3(0, -.5f, 0));

        /// <summary>
        /// The direction in which this player is facing.
        /// Is not the same as the camera, but follows it.
        /// </summary>
        private vec3 Direction = new vec3(1,0,0);

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
        public readonly Inventory Inventory;

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
            Inventory = new Inventory(this);
		}

        public void Update(float elapsedSeconds)
        {
            // Update drones.
            foreach (var drone in Drones)
                drone.Update(elapsedSeconds);
            foreach (var dc in DroneConstraints)
                dc.Update(elapsedSeconds);

            if (!LocalScript.NoclipEnabled)
            {
                // Update direction.
                float mix = (float)Math.Pow(0.02, elapsedSeconds);
                vec3 camDir = camera.ForwardDirection;
                Direction.x = Direction.x * mix + camDir.x * (1 - mix);
                Direction.z = Direction.z * mix + camDir.z * (1 - mix);
                Direction = Direction.Normalized;

                // Update player model.
                vec3 up = new vec3(0, 1, 0);
                vec3 left = vec3.cross(up, Direction);
                rcTorso.Transform = rcTorsoShadow.Transform =
                   new mat4(left, up, Direction, new vec3()) * torsoTransform;
            }

            mat4 steamTransform = thisEntity.Transform * rcTorso.Transform * torsoSteamOffset; 
            vec3 steamOrigin = new vec3(steamTransform * new vec4(0,0,0,1));
            vec3 steamVeloMin = new vec3(steamTransform * new vec4(.13f, 0.05f, 0, 0));
            vec3 steamVeloMax = new vec3(steamTransform * new vec4(.16f, 0.07f, 0, 0));
            psTorsoSteam.SetSpawner2D(.03f, new BoundingSphere(steamOrigin, .01f), 
                                      steamVeloMin, steamVeloMax,
                                      new vec4(new vec3(.9f), .8f), new vec4(new vec3(.99f), .9f),
                                      2.0f, 3.4f,
                                      .1f, .2f,
                                      0, 360,
                                      -.2f, .2f);
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
			                     mat4.Translate(new vec3(0, 0.2f, 0)));

            physicsComponent.RigidBody.SetTransformation(mat4.Translate(new vec3(0, 30f, 0)));

			// Add Torso mesh.
			rcTorso = new RenderComponent(thisEntity, torsoTransform,
                                          new MeshRenderJob(Renderer.Opaque.Mesh, Resources.UseMaterial("Miner/Torso", HostScript.ModDomain), Resources.UseMesh("Miner/Torso", HostScript.ModDomain), mat4.Identity),
			                              true);
			rcTorsoShadow = new RenderComponent(thisEntity, torsoTransform,
			                                    new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", HostScript.ModDomain), Resources.UseMesh("Miner/Torso", HostScript.ModDomain), mat4.Identity),
			                                    true);
            psTorsoSteam = CpuParticleSystem.Create2D(new vec3(), ContainingWorld);
            pcTorsoSteam = new CpuParticleComponent(LocalScript.ParticleEntity, psTorsoSteam, mat4.Identity);
            rcTorsoSteam = new RenderComponent(LocalScript.ParticleEntity, mat4.Identity,
                                               new CpuParticleRenderJob(psTorsoSteam, Renderer.Transparent.CpuParticles, Resources.UseMaterial("Particles/Smoke", HostScript.ModDomain), Resources.UseMesh("::Debug/Quad", null), mat4.Identity),
                                               true);

            // This digging controller will perform digging and handle digging constraints for us.
            digging = new DiggingController(ContainingWorld, this);

            gui = new PlayerGui(this);

            AddTriggerSlot("AddItem");

            Inventory.InitCraftingRules();
            generateInitialItems();
		}

        /// <summary>
        /// Populates the inventory with a list of items that we start with.
        /// </summary>
        void generateInitialItems()
        {
            TerrainMaterial dirt = ContainingWorld.Terrain.QueryMaterialFromName("Dirt");
            TerrainMaterial stone06 = ContainingWorld.Terrain.QueryMaterialFromName("Stone.06"); 

            Inventory.AddResource(dirt, 10);
            Inventory.AddItem(new ResourceItem(dirt, 3f));
            Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Sphere, new vec3(1)));
            Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Sphere, new vec3(1), 2));
            Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Cylinder, new vec3(1,2,2)));
            Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Sphere, new vec3(2)));
            Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Cube, new vec3(2)));
            Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Cylinder, new vec3(1,2,2)));
            Inventory.AddItem(new MaterialItem(dirt, MaterialShape.Sphere, new vec3(1)));
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
            
            ContainingWorld.AddEntity(d, mat4.Translate(d.CurrentPosition));
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
                        /// Subtract a few cm toward camera to increase stability near constraints.
                        _position -= camera.ForwardDirection * .04f;

                        if (e.Key == InputKey.MouseLeft)
                        {
							digging.DigSphere(_position, diggingSphereRadius);
                        } else if (e.Key == InputKey.MouseMiddle) {
                            // TODO: proper terrain material index.
                            digging.DigSphere(_position, diggingSphereRadius, ContainingWorld.Terrain.QueryMaterialFromName("Wood").MaterialIndex, DiggingController.DigMode.Add);
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
            Inventory.AddItem(addItemMsg.PickedItem);

        }

	}
}

