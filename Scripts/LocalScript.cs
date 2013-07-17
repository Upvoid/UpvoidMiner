using System;
using Engine;
using Engine.Input;
using Engine.Model;
using Engine.Rendering;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Common.Cameras;

namespace UpvoidMiner
{
	/// <summary>
	/// Main class for the local scripting domain.
	/// </summary>
	public class LocalScript
	{
		public static GenericCamera Camera;
		public static FreeCameraControl CameraControl;
		public static Module Mod;
		public static ResourceDomain ModDomain;

		/// <summary>
		/// Updates the camera position.
		/// </summary>
		private static void Update(float _elapsedSeconds)
		{
			CameraControl.Update(_elapsedSeconds);
		}

		/// <summary>
		/// Initializes the local part of the mod.
		/// </summary>
		public static void Startup(IntPtr _unmanagedModule)
		{
			// Create a simple camera that allows free movement.
			Camera = new GenericCamera();
			Camera.FarClippingPlane = 750.0;
			CameraControl = new FreeCameraControl(-10f, Camera);

			// Get the world (created by the host script)
			World world = Universe.GetWorldByName("UpvoidMinerWorld");

			// Place the camera in the world
			world.AttachCamera(Camera);
			if(Rendering.ActiveMainPipeline != null)
				Rendering.ActiveMainPipeline.SetCamera(Camera);

			GC.KeepAlive(Camera);

			// Configure the camera to receive user input
			Input.RootGroup.AddListener(CameraControl);

			// Registers the update callback that updates the camera position.
			Scripting.RegisterUpdateFunction(Update, 1 / 60.0f, 3 / 60.0f);
		}
	}
}
