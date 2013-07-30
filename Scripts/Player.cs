using System;
using Engine;
using Engine.Model;
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

		protected override void Init()
		{
			// For now, attach this entity to a simple sphere physics object.
			physicsComponent = new PhysicsComponent(Entity.FromId(ID), 
                                 ContainingWorld.Physics.CreateAndAddRigidBody(70.0f, mat4.Identity, new SphereShape(2.0f)),
			                     mat4.Identity);
		}
	}
}

