using System;
using Engine;
using Engine.Universe;
using Engine.Physics;
using Engine.Input;
using Engine.Scripting;
using Engine.Webserver;

namespace UpvoidMiner
{
	/// <summary>
	/// A simple character controller.
	/// It takes control of a rigid body and lets the user steer it around using the WASD keys.
	/// A given camera is used to get the walking direction.
	/// </summary>
	public class CharacterController
	{
		/// <summary>
		/// The current velocity of the controlled rigid body, in meters per second. Note that for the physics system, the body is static. We move it manually for maximum control.
		/// </summary>
		public vec3 Velocity { get; protected set; }

		/// <summary>
		/// The current position of the controlled character.
		/// </summary>
		public vec3 Position { get; protected set; }

		/// <summary>
		/// The velocity of the character when walking, in meters per seconds. Default is 1.8 (about 6.5 km/h).
		/// </summary>
		public float WalkSpeed { get; set; }

		/// <summary>
		/// The velocity of the character when running, in meters per seconds. Default is 4 (about 15 km/h).
		/// </summary>
		public float WalkSpeedRunning { get; set; }

		/// <summary>
		/// True iff the character is currently running
		/// </summary>
		public bool IsRunning { get; protected set; }

		/// <summary>
		/// True iff the character is closer than 40cm to the ground. Usually, it hovers 30cm above.
		/// </summary>
		public bool TouchesGround { get; protected set; }

		/// <summary>
		/// The world that contains the controlled rigid body.
		/// </summary>
		public World ContainingWorld { get; protected set; }

		/// <summary>
		/// The rigid body that represents the controlled character to the physics system.
		/// </summary>
		public RigidBody ControlledBody { get; protected set; }

		/// <summary>
		/// This camera is used to determine the directions we are walking. Forward means the direction the camera is currently pointing.
		/// </summary>
		GenericCamera camera;

		/// <summary>
		/// Forward/neutral/backward encoded in -1/0/1
		/// </summary>
		int walkDirForward = 0;

		/// <summary>
		/// Left/neutral/right encoded in -1/0/1
		/// </summary>
		int walkDirRight = 0;

		/// <summary>
		/// The last known distance to the ground.
		/// </summary>
		float distanceToGround = 0;

		/// <summary>
		/// The amount of seconds since we had ground below us.
		/// </summary>
		float secondsSinceGroundWasBelow = 0f;

		/// <summary>
		/// True iff there is ground below us.
		/// </summary>
		bool noGroundBelow = false;


		public CharacterController(RigidBody _controlledBody, GenericCamera _camera, World _containingWorld, vec3 _position)
		{
			camera = _camera;
			ContainingWorld = _containingWorld;
			ControlledBody = _controlledBody;
			Position = _position;

			// Initialize default values
			Velocity = new vec3(0, 0, 0);
			IsRunning = false;
			WalkSpeed = 1.8f;
			WalkSpeedRunning = 4f;

			// Register the required callbacks.
			// This update function is called 20 - 60 times per second to update the character position.
			Scripting.RegisterUpdateFunction(Update, 1 / 60f, 1 / 20f);

			// This event handler is used to catch the keyboard input that steers the character.
			Input.OnPressInput += HandleInput;
		}

		/// <summary>
		/// Called by the scripting system in regular timesteps. Updates the position of the character.
		/// </summary>
		/// <param name="_elapsedSeconds">The elapsed seconds since the last call.</param>
		protected void Update(float _elapsedSeconds)
		{
			// Store the velocity to enable writing to single coordinates.
			vec3 velocity = Velocity;

			// Move the rigid body along the velocity.
			Position += velocity * _elapsedSeconds;

			// Update the rigid body position
			ControlledBody.SetTransformation(mat4.Translate(Position));

			// When touching the ground, we can walk around.
			if(TouchesGround) {
				// Use the forward and right directions of the camera. Remove the y component, and we have our walking direction.
				vec3 moveDir = camera.ForwardDirection * walkDirForward + camera.RightDirection * walkDirRight;
				moveDir.y = 0;

				// Normalize and multiply with the walk or running speed to get the designated velocity.
				moveDir = moveDir.Normalized * (IsRunning ? WalkSpeedRunning : WalkSpeed);
				velocity.x = moveDir.x;
				velocity.z = moveDir.z;
			}

			// No ground below us? Then we may be stuck in the earth.
			if(noGroundBelow) {
				secondsSinceGroundWasBelow += _elapsedSeconds;

				// Ignore short times where no ground is beneath.
				// However, when we don't find any ground below us for longer than one second,
				// we assume that we are underground and move upward to fix this.
				// This currently can also occur when we walk into areas where no terrain is generated yet.
				if(secondsSinceGroundWasBelow > 1f)
					velocity = new vec3(0, 1f, 0);

			} else {
				// We have ground below us, so set this timer to zero.
				secondsSinceGroundWasBelow = 0;

				// Approach our optimal position (hovering 30cm over the ground).
				// When we are more than 10cm away from our goal position, we apply an acceleration that gives us a fake gravity at the same time.
				// When closer than 10cm to the goal position, we move linearly towards it to prevent oscillation around it.
				if(Math.Abs(distanceToGround - 0.3f) > 0.1f)
					velocity.y += -10f * Math.Max(Math.Min(distanceToGround - 0.3f, 1f), -1f) * _elapsedSeconds;
				else
					velocity.y = 0.3f - distanceToGround;
			}

			// Perform a ray query to find the ground below us. The ray starts at our position and ends 1km below us.
			ContainingWorld.Physics.RayQuery(Position, Position + new vec3(0, -100f, 0),
            	delegate(bool _hit, vec3 _hitPosition, vec3 _normal, RigidBody _body, bool _hasTerrainCollision)
	            {
					// Receiving the async ray result here.

					// Nothing found below us? Might be stuck in the terrain.
					noGroundBelow = !_hit;

					// Set the distance to the ground to the end of the ray if no collision was found.
					if(!_hit) {
						distanceToGround = 100f;
					} else {
						distanceToGround = Position.y - _hitPosition.y;
					}

					// Usually, we hover at 30cm over the ground. We are by definition touching the ground if we are closer to it than 40cm.
					if(Math.Abs(distanceToGround) < 0.4f)
						TouchesGround = true;

					// When we got very close to the ground, make sure we stop there.
				if(distanceToGround < 0.05f && velocity.y < 0)
					velocity.y = 0;

				}
			);

			// Write the new velocity back to the property
			Velocity = velocity;

		}

		/// <summary>
		/// Called on keyboard input. Updates the walking directions of the character.
		/// </summary>
		protected void HandleInput(object sender, InputPressArgs e)
		{
			// Let the default WASD-keys control the walking directions.
			if(e.Key == InputKey.W) {
				if(e.PressType == InputPressArgs.KeyPressType.Down)
					walkDirForward++;
				else
					walkDirForward--;
			} else if(e.Key == InputKey.S) {
				if(e.PressType == InputPressArgs.KeyPressType.Down)
					walkDirForward--;
				else
					walkDirForward++;
			} else if(e.Key == InputKey.D) {
				if(e.PressType == InputPressArgs.KeyPressType.Down)
					walkDirRight++;
				else
					walkDirRight--;
			} else if(e.Key == InputKey.A) {
				if(e.PressType == InputPressArgs.KeyPressType.Down)
					walkDirRight--;
				else
					walkDirRight++;
			} else if(e.Key == InputKey.Shift) { // Shift controls running
				if(e.PressType == InputPressArgs.KeyPressType.Down)
					IsRunning = true;
				else
					IsRunning = false;
			}

			// Clamp the walking directions to [-1, 1]. The values could get out of bound, for example, when we receive two down events without an up event in between.
			if(walkDirForward < -1)
				walkDirForward = -1;
			if(walkDirForward > 1)
				walkDirForward = 1;
			if(walkDirRight < -1)
				walkDirRight = -1;
			if(walkDirRight > 1)
				walkDirRight = 1;
		}
	}
}

