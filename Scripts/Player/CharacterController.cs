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
        /// The rigid body that represents the controlled character to the physics system.
        /// </summary>
        public RigidBody Body { get; protected set; }

		/// <summary>
		/// The current position of the controlled character.
		/// </summary>
        public vec3 Position { 
            get
            {
                return new vec3(Body.GetTransformation().col3); 
            }
        }
        /// <summary>
        /// Gets the transformation matrix of the controlled character.
        /// </summary>
        public mat4 Transformation
        {
            get
            {
                return Body.GetTransformation(); 
            }
        }

        /// <summary>
        /// The height that the controller tries to keep between the body and the ground.
        /// The RigidBody representing the character hovers above the ground to make walking on non-planar ground easier.
        /// </summary>
        /// <value>
        /// Sane values usually lie between 0.2 and 0.5. Default is 0.4.
        /// If it is too low, the body will collide with obstacles that a real person would just step over.
        /// If it is too high, the body will not collide with obstacles that a real person would not simply step over.
        /// </value>
        public float HoverHeight = 0.4f;

        /// <summary>
        /// The total height of the simulated character (from the ground to the top, including HoverHeight).
        /// </summary>
        public float CharacterHeight { get; protected set; }

        /// <summary>
        /// The diameter of the character's body.
        /// </summary>
        public float CharacterDiameter { get; protected set; }

        /// <summary>
        /// The mass of the character's body in kilograms.
        /// </summary>
        public float CharacterMass { get { return Body.Mass; } }

        /// <summary>
        /// The physical impulse (meters per second) that will be applied to the body for a jump.
        /// </summary>
        public float JumpImpulse = 300f;

		/// <summary>
		/// The velocity of the character when walking (meters per second). Default is 1.8 (about 6.5 km/h).
		/// </summary>
        public float WalkSpeed = 1.8f;
        
        /// <summary>
        /// The velocity of the character when strafing (meters per second). Default is 1.0 (3.6 km/h).
        /// </summary>
        public float StrafeSpeed = 1f;
        
        /// <summary>
        /// The velocity of the character when strafing while running (meters per second). Default is 3.0 (11 km/h).
        /// </summary>
        public float StrafeSpeedRunning = 3f;

		/// <summary>
		/// The velocity of the character when running (meters per second). Default is 4 (about 15 km/h).
		/// </summary>
        public float WalkSpeedRunning = 4f;

        /// <summary>
        /// Returns true iff the character is currently walking or running.
        /// </summary>
        public bool IsWalking { get { return walkDirRight != 0 || walkDirForward != 0; } }

		/// <summary>
		/// True iff the character is currently running.
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

        public CharacterController(GenericCamera _camera, World _containingWorld, float _bodyHeight = 1.25f, float _bodyDiameter = 0.55f, float _bodyMass = 70f)
		{
			camera = _camera;
			ContainingWorld = _containingWorld;

            CharacterHeight = _bodyHeight;
            CharacterDiameter = _bodyDiameter;

			// Initialize default values for auto properties
			IsRunning = false;
			WalkSpeed = 1.8f;
			WalkSpeedRunning = 4f;

            // Create a capsule shaped rigid body representing the character in the physics world.
            Body = ContainingWorld.Physics.CreateAndAddRigidBody(_bodyMass, mat4.Identity, new CapsuleShape(CharacterDiameter/2f, CharacterHeight - HoverHeight));

            // Prevent the rigid body from falling to the ground by simply disabling any rotation
            Body.SetAngularFactor(vec3.Zero);

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

			// When touching the ground, we can walk around.
			if(TouchesGround) {
                
                float forwardSpeed = IsRunning ? WalkSpeedRunning : WalkSpeed;
                float strafeSpeed = IsRunning ? StrafeSpeedRunning : StrafeSpeed;

                // Use the forward and right directions of the camera. Remove the y component, and we have our walking direction.
                vec3 moveDir = camera.ForwardDirection * walkDirForward * forwardSpeed + camera.RightDirection * walkDirRight * strafeSpeed;
				moveDir.y = 0;

                vec3 velocity = Body.GetVelocity();
                velocity.y = 0;

                Body.ApplyImpulse( (moveDir - velocity)  * CharacterMass, vec3.Zero);

			}

            // Let the character hover over the ground by applying a custom gravity. We apply the custom gravity when the body is below the desired height plus 0.5 meters.
            // Our custom gravity pushes the body to its desired height and becomes smaller the closer it gets to prevent rubber band effects.
            if(distanceToGround < HoverHeight+0.5f) {

                vec3 velocity = Body.GetVelocity();

                // Never move down when more than 5cm below the desired height.
                if(distanceToGround < HoverHeight-0.05f && velocity.y < 0f) {
                    velocity.y = 0;
                    Body.SetVelocity(velocity);
                }

                float customGravity = HoverHeight - distanceToGround;
                Body.SetGravity(new vec3(0, customGravity, 0));
            }
            else
                Body.SetGravity(new vec3(0, -9.807f, 0));

            ContainingWorld.Physics.RayQuery(Position, Position - new vec3(0, 5f, 0), ReceiveRayqueryResult);
		}

        protected void ReceiveRayqueryResult(bool hasCollision, vec3 hitPosition, vec3 normal, RigidBody body, bool hasTerrainCollision)
        {
            if(hasCollision)
                distanceToGround = Position.y - hitPosition.y - 1f;
            else
                distanceToGround = 5f;

            TouchesGround = Math.Abs(distanceToGround) < 1f;
        }

		/// <summary>
		/// Called on keyboard input. Updates the walking directions of the character.
		/// </summary>
		protected void HandleInput(object sender, InputPressArgs e)
		{
            if (LocalScript.NoclipEnabled)
                return;

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
            } else if(e.Key == InputKey.Space) { //Space lets the player jump
                if(TouchesGround) {
                    Body.ApplyImpulse(new vec3(0, 5f*CharacterMass, 0), vec3.Zero);
                }
            } else if(e.Key == InputKey.Shift) { // Shift controls running
                if(e.PressType == InputPressArgs.KeyPressType.Down)
                    IsRunning = true;
                else
                    IsRunning = false;
            } else if(e.Key == InputKey.Q) {
                Body.SetTransformation(mat4.Translate(new vec3(0, 50f, 0)) * Body.GetTransformation());
                Body.SetVelocity(vec3.Zero);
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

            // This hack stops the player movement immediately when we stop walking
            //TODO: do some actual friction simulation instead
            if(walkDirRight == 0 && walkDirRight == 0 && TouchesGround && e.PressType == InputPressArgs.KeyPressType.Up) {
                Body.SetVelocity(vec3.Zero);
            }
		}
	}
}

