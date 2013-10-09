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
using Engine.Input;
using Engine.Universe;
using Engine.Rendering;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Webserver;
using Common.Cameras;

using System.Runtime.InteropServices;

namespace UpvoidMiner
{
	/// <summary>
	/// Main class for the local scripting domain.
	/// </summary>
	public class LocalScript
	{
		/// <summary>
		/// Resource domain
		/// </summary>
		public static ResourceDomain ModDomain;
		/// <summary>
		/// Mod
		/// </summary>
		public static Module Mod;
		
		/// <summary>
		/// The main world. We will use this to create new entities or query information about the environment.
		/// </summary>
        public static World world;

        /// <summary>
        /// A global entity that is located in the origin and can be used to spawn particles.
        /// This is more or less a workaround so that particles behave more plausible.
        /// </summary>
        public static AnonymousEntity ParticleEntity;

		/// <summary>
		/// The main camera that renders to the screen.
		/// </summary>
		static GenericCamera camera;

		/// <summary>
		/// A camera controller for free camera movement. Used when noclipEnabled is true.
		/// </summary>
		static FreeCameraControl cameraControl;

		/// <summary>
		/// The player entity. Not to confuse with the Player EntityScript.
		/// </summary>
		static Entity playerEntity = null;

        static Player player;

		/// <summary>
		/// Set this to true to enable free camera movement.
		/// </summary>
        public static bool NoclipEnabled { get; private set; }

		/// <summary>
		/// This is called by the engine at mod startup and initializes the local part of the UpvoidMiner mod.
		/// </summary>
		public static void Startup(IntPtr _unmanagedModule)
		{
			// Get and save the resource domain of the mod, needed for loading resources.
			Mod = Module.FromHandle(_unmanagedModule);
			ModDomain = Mod.ResourceDomain;

			// Create a simple camera that allows free movement.
			camera = new GenericCamera();
			camera.FarClippingPlane = 1750.0;
			cameraControl = new FreeCameraControl(-10f, camera);

			// Get the world (created by the host script).
			world = Universe.GetWorldByName("UpvoidMinerWorld");

			// Place the camera in the world.
			world.AttachCamera(camera);
			if(Rendering.ActiveMainPipeline != null)
				Rendering.ActiveMainPipeline.SetCamera(camera);

            // Create particle entity.
            ParticleEntity = new AnonymousEntity(mat4.Identity);
            world.AddEntity(ParticleEntity);

			// Create the Player EntityScript and add it to the world.
			// For now, place him 30 meters above the ground and let him drop to the ground.
            player = new Player(camera);
			playerEntity = world.AddEntity(player, mat4.Translate(new vec3(0, 50f, 0)));

            // Create an active region around the player spawn
            // Active regions help the engine to decide which parts of a world are important (to generate, render, etc.)
            // In near future it will be updated when the player moves out of it
            world.AddActiveRegion(new ivec3(), 100f, 400f, 40f, 40f);


            Gui.NavigateTo("http://localhost:8080/Mods/Upvoid/UpvoidMiner/0.0.1/IngameGui.html");

			// Configure the camera to receive user input.
			Input.RootGroup.AddListener(cameraControl);

			// Register for input press events.
			Input.OnPressInput += HandlePressInput;

			// Registers the update callback that updates the camera position.
			Scripting.RegisterUpdateFunction(Update, 1 / 60f, 3 / 60f);
		}

		/// <summary>
		/// Performs some basic input handling.
		/// </summary>
		private static void HandlePressInput(object sender, InputPressArgs e)
		{
			// For now, gameplay and debug actions are bound to static keys.

			// N toggles noclip.
			if(e.PressType == InputPressArgs.KeyPressType.Up && e.Key == InputKey.N)
				NoclipEnabled = !NoclipEnabled;
		}

		/// <summary>
		/// Updates the camera position.
		/// </summary>
		public static void Update(float _elapsedSeconds)
        {
			cameraControl.Update(_elapsedSeconds);

			// Set the camera to the player position if free camera movement is disabled.
			if(!NoclipEnabled && playerEntity != null) {
                if(!playerEntity.Position.IsFinite)
                    return;
                
                // Also add 10cm of forward.xz direction for a "head offset"
                vec3 forward = camera.ForwardDirection;
                forward.y = 0;
                camera.Position = playerEntity.Position + forward.Normalized * 0.1f;
			}

            player.Update(_elapsedSeconds);
		}

		/// <summary>
		/// MonoDevelop's debugger requires an executable program, so here is a dummy Main method.
		/// </summary>
		private static void Main() { throw new Exception("I'm a mod, don't execute me like that!"); }
	}
}
