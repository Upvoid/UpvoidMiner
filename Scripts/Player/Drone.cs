using System;
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
        private const float DroneScale = 2f;

        /// <summary>
        /// Total lifetime of the drone.
        /// </summary>
        private float lifetime = 0f;

        // Render Components.
        private RenderComponent renderComponentBodyOpaque;
        private RenderComponent renderComponentBodyShadow;
        private RenderComponent renderComponentWing1Opaque;
        private RenderComponent renderComponentWing1Shadow;
        private RenderComponent renderComponentWing2Opaque;
        private RenderComponent renderComponentWing2Shadow;
        private RenderComponent renderComponentIndicator;

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
            renderComponentBodyOpaque = new RenderComponent(thisEntity,
                                                            mat4.Scale(DroneScale),
                                                            new MeshRenderJob(Renderer.Opaque.Mesh, Resources.UseMaterial("Miner/Torso", HostScript.ModDomain), Resources.UseMesh("Miner/DroneBody", HostScript.ModDomain), mat4.Identity),
                                                            true);
            renderComponentBodyShadow = new RenderComponent(thisEntity,
                                                            mat4.Scale(DroneScale),
                                                            new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", HostScript.ModDomain), Resources.UseMesh("Miner/DroneBody", HostScript.ModDomain), mat4.Identity),
                                                            true);

            renderComponentWing1Opaque = new RenderComponent(thisEntity,
                                                             mat4.Scale(DroneScale),
                                                            new MeshRenderJob(Renderer.Opaque.Mesh, Resources.UseMaterial("Miner/Torso", HostScript.ModDomain), Resources.UseMesh("Miner/DroneWing1", HostScript.ModDomain), mat4.Identity),
                                                            true);
            renderComponentWing1Shadow = new RenderComponent(thisEntity,
                                                             mat4.Scale(DroneScale),
                                                            new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", HostScript.ModDomain), Resources.UseMesh("Miner/DroneWing1", HostScript.ModDomain), mat4.Identity),
                                                            true);

            renderComponentWing2Opaque = new RenderComponent(thisEntity,
                                                             mat4.Scale(DroneScale),
                                                            new MeshRenderJob(Renderer.Opaque.Mesh, Resources.UseMaterial("Miner/Torso", HostScript.ModDomain), Resources.UseMesh("Miner/DroneWing2", HostScript.ModDomain), mat4.Identity),
                                                             true);
            renderComponentWing2Shadow = new RenderComponent(thisEntity,
                                                             mat4.Scale(DroneScale),
                                                             new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", HostScript.ModDomain), Resources.UseMesh("Miner/DroneWing2", HostScript.ModDomain), mat4.Identity),
                                                             true);

            renderComponentIndicator = new RenderComponent(thisEntity,
                                                           mat4.Scale(new vec3(.03f,7,.03f)),
                                                           new MeshRenderJob(Renderer.Transparent.Mesh, Resources.UseMaterial("Miner/DroneIndicator", HostScript.ModDomain), Resources.UseMesh("Miner/DroneIndicator", HostScript.ModDomain), mat4.Identity),
                                                           true);
        }
    }
}

