using System;
using System.Diagnostics;
using Engine;
using Engine.Universe;
using Engine.Rendering;
using Engine.Resources;

namespace UpvoidMiner
{
    /// <summary>
    /// Types of drones.
    /// </summary>
    public enum DroneType
    {
        Chain,
        Loop,
        Circular,
        Plane
    }

    /// <summary>
    /// One of the Player's Drones used for constraining his digging/creation process.
    /// </summary>
    public class Drone : EntityScript
    {
        /// <summary>
        /// Target position of the drone.
        /// </summary>
        public vec3 TargetPosition { get; private set; }
        /// <summary>
        /// Current position of the drone.
        /// </summary>
        public vec3 CurrentPosition { get; private set; }

        /// <summary>
        /// Type of the drone.
        /// </summary>
        public readonly DroneType Type;

        /// <summary>
        /// Owning player.
        /// </summary>
        public readonly Player Owner;
        
        /// <summary>
        /// Speed in m/s.
        /// </summary>
        public const float Speed = 10f;
        /// <summary>
        /// Rotation Speed in circles/s.
        /// </summary>
        private const float RotationSpeed = 3f;
        /// <summary>
        /// Scale of the drone.
        /// </summary>
        private const float DroneScale = 1.3f;

        /// <summary>
        /// Total lifetime of the drone.
        /// </summary>
        private float lifetime = 0f;

        // Render Components.
        private RenderComponent renderComponentWing1Opaque;
        private RenderComponent renderComponentWing1Shadow;
        private RenderComponent renderComponentWing2Opaque;
        private RenderComponent renderComponentWing2Shadow;

        // Trigger for drone collection.
        TriggerId AddItemTrigger;

        /// <summary>
        /// Creates a new drone.
        /// </summary>
        public Drone(vec3 target, Player owner, DroneType type)
        {
            TargetPosition = target;
            Owner = owner;
            CurrentPosition = Owner.Position;
            Type = type;
        }

        /// <summary>
        /// Updates this drone.
        /// </summary>
        public void Update(float elapsedSeconds)
        {
            lifetime += elapsedSeconds;
            vec3 realTarget = TargetPosition + new vec3(0, (float)Math.Cos(lifetime) * .1f, 0);

            if ((realTarget - CurrentPosition).Length > .001)
            {
                float dis = elapsedSeconds * Speed;
                if ((realTarget - CurrentPosition).Length < dis)
                    CurrentPosition = realTarget;
                else
                    CurrentPosition += (realTarget - CurrentPosition).Normalized * dis;

                thisEntity.Transform = mat4.Translate(CurrentPosition);
            }
            
            renderComponentWing1Opaque.Transform = renderComponentWing1Shadow.Transform = mat4.Scale(DroneScale) * mat4.RotateY(-360 * RotationSpeed * lifetime);
            renderComponentWing2Opaque.Transform = renderComponentWing2Shadow.Transform = mat4.Scale(DroneScale) * mat4.RotateY(360 * RotationSpeed * lifetime);
        }

        protected override void Init()
        {
            // Add Torso mesh.
            thisEntity.AddComponent(new RenderComponent(
                new MeshRenderJob(Renderer.Opaque.Mesh, Resources.UseMaterial("Miner/Torso", UpvoidMiner.ModDomain), Resources.UseMesh("Miner/DroneBody", UpvoidMiner.ModDomain), mat4.Identity),
                mat4.Scale(DroneScale),
                true));
            thisEntity.AddComponent(new RenderComponent(
                new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", UpvoidMiner.ModDomain), Resources.UseMesh("Miner/DroneBody", UpvoidMiner.ModDomain), mat4.Identity),                                                        
                mat4.Scale(DroneScale),
                true));

            thisEntity.AddComponent(renderComponentWing1Opaque = new RenderComponent(
                new MeshRenderJob(Renderer.Opaque.Mesh, Resources.UseMaterial("Miner/Torso", UpvoidMiner.ModDomain), Resources.UseMesh("Miner/DroneWing1", UpvoidMiner.ModDomain), mat4.Identity),
                mat4.Scale(DroneScale),
                true));
            thisEntity.AddComponent(renderComponentWing1Shadow = new RenderComponent(
                new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", UpvoidMiner.ModDomain), Resources.UseMesh("Miner/DroneWing1", UpvoidMiner.ModDomain), mat4.Identity),
                mat4.Scale(DroneScale),
                true));

            thisEntity.AddComponent(renderComponentWing2Opaque = new RenderComponent(
                new MeshRenderJob(Renderer.Opaque.Mesh, Resources.UseMaterial("Miner/Torso", UpvoidMiner.ModDomain), Resources.UseMesh("Miner/DroneWing2", UpvoidMiner.ModDomain), mat4.Identity),
                mat4.Scale(DroneScale),
                true));
            thisEntity.AddComponent(renderComponentWing2Shadow = new RenderComponent(
                new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", UpvoidMiner.ModDomain), Resources.UseMesh("Miner/DroneWing2", UpvoidMiner.ModDomain), mat4.Identity),
                mat4.Scale(DroneScale),
                true));

            thisEntity.AddComponent(new RenderComponent(
                new MeshRenderJob(Renderer.Transparent.Mesh, Resources.UseMaterial("Miner/DroneIndicator", UpvoidMiner.ModDomain), Resources.UseMesh("Miner/DroneIndicator", UpvoidMiner.ModDomain), mat4.Identity),
                mat4.Scale(new vec3(.03f,7,.03f)),
                true));

            // Set up the triggers.
            AddItemTrigger = TriggerId.getIdByName("AddItem");
            AddTriggerSlot("Interaction");
        }

		public void Interaction(object msg)
        {
            // Make sure we get the message type we are expecting.
            InteractionMessage interactionMsg = msg as InteractionMessage;
            if(interactionMsg == null)
                return;

            // Interacting with an item means picking it up. Answer by sending the item to the sender.
            interactionMsg.Sender[AddItemTrigger] |= new AddItemMessage(new ToolItem(ToolType.DroneChain));

            // Remove drone from drone constraints.
            Owner.RemoveDrone(this);
            // And remove this entity.
            ContainingWorld.RemoveEntity(thisEntity);
        }
    }
}

