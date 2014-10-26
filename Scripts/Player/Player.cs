// Copyright (C) by Upvoid Studios
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using Engine.Audio;
using Engine.Universe;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Rendering;
using Engine.Physics;
using Engine.Network;
using Engine.Input;
using System.IO;
using Newtonsoft.Json;


namespace UpvoidMiner
{
    /// <summary>
    /// Contains the game logic and the internal state of the player character.
    /// </summary>
    public class Player: EntityScript
    {
        /// <summary>
        /// The render component for the torso.
        /// </summary>
        private RenderComponent rcTorsoShadow;
        //private CpuParticleSystemBase psTorsoSteam;
        private mat4 torsoSteamOffset = mat4.Translate(new vec3(.13090f, .53312f, -.14736f));
        /// <summary>
        /// Relative torso transformation.
        /// </summary>
        private mat4 torsoTransform = mat4.Scale(2f) * mat4.Translate(new vec3(0, -.5f, 0));

        /// <summary>
        /// Minimum and maximum ranges for ray querys for first-person and no-clip mode, resp.
        /// </summary>
        const float minRayQueryDistancePlayer = 0.25f;
        const float maxRayQueryDistancePlayer = 10.0f;
        const float minRayQueryDistanceNoClip = 0.1f;
        const float maxRayQueryDistanceNoClip = 200.0f;

        private const int millisecondsBetweenItemUsages = 500;

        public static vec3 SpawnPosition = new vec3(150, 5, 150);

        /// <summary>
        /// The direction in which this player is facing.
        /// Is not the same as the camera, but follows it.
        /// </summary>
        public vec3 Direction { get; private set; }
        /// <summary>
        /// Azimuth angle in degree (around y-axis).
        /// </summary>
        private float AngleAzimuth = 0;
        /// <summary>
        /// Elevation angle in degree (0 = horizontal)
        /// </summary>
        private float AngleElevation = 0;
        /// <summary>
        /// Gets the camera direction.
        /// </summary>
        public vec3 CameraDirection
        {
            get
            {
                float sinAzi = (float)Math.Sin(AngleAzimuth / 180.0 * Math.PI);
                float cosAzi = (float)Math.Cos(AngleAzimuth / 180.0 * Math.PI);
                float sinEle = (float)Math.Sin(AngleElevation / 180.0 * Math.PI);
                float cosEle = (float)Math.Cos(AngleElevation / 180.0 * Math.PI);
                return new vec3(sinAzi * cosEle, sinEle, cosAzi * cosEle).Normalized;
            }
        }

        /// <summary>
        /// This is the camera that is used to show the perspective of the player.
        /// </summary>
        GenericCamera camera;
        /// <summary>
        /// Component used for synchronizing camera to player
        /// </summary>
        CameraComponent cameraComponent;

        /// <summary>
        /// This takes control of the rigid body attached to this entity and lets us walk around.
        /// </summary>
        CharacterController character;

        public CharacterController Character { get { return character; } }

        /// <summary>
        /// Controller for digging and its constraints.
        /// </summary>
        DiggingController digging;

        /// <summary>
        /// Controller for input handling.
        /// </summary>
        InputController input;

        /// <summary>
        /// GUI for player values.
        /// </summary>
        public PlayerGui Gui { get; set; }

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
		/// True iff the player is physically frozen because the world around him is not yet generated.
		/// </summary>
		/// <value>The position.</value>
		public bool IsFrozen { get { return character.Body.IsFrozen; } }

		/// <summary>
		/// The Value of IsFrozen from the last update frame.
		/// </summary>
		bool WasFrozen = false;

        public bool GodMode { get; protected set; }

        /// <summary>
        /// Position of the player.
        /// </summary>
        public vec3 Position
        {
            get { return character.Position; }
        }
        /// <summary>
        /// Character transformation matrix.
        /// </summary>
        public mat4 Transformation
        {
            get { return character.Transformation; }
        }

		public enum DiggingShape
		{
            Sphere,
            Box,
            Cylinder
		}

		public enum DiggingAlignment
		{
            AxisAligned,
            GridAligned,
            PlayerAligned,
            TerrainAligned
		}

		public DiggingShape CurrentDiggingShape { get; set; }
		public DiggingAlignment CurrentDiggingAlignment { get; set; }

        public Player(GenericCamera _camera, bool _godMode)
        {
            GodMode = _godMode;
            Direction = new vec3(1, 0, 0);
            camera = _camera;
            CurrentDiggingShape = DiggingShape.Sphere;
			CurrentDiggingAlignment = DiggingAlignment.AxisAligned;
            Inventory = new Inventory(this);
        }


        protected override void Init()
        {
            // Create a character controller that allows us to walk around.
            if (ContainingWorld == null)
                throw new InvalidOperationException();

            camera.Position = thisEntity.Position;

            input = new InputController(this);

            character = new CharacterController(camera, ContainingWorld, GodMode);

            // For now, attach this entity to a simple sphere physics object.
            character.Body.SetTransformation(thisEntity.Transform);
            thisEntity.AddComponent(new PhysicsComponent(
                                         character.Body,
                                         mat4.Translate(new vec3(0, (GodMode ? 0f : character.EyeOffset), 0))));

            // Add Torso mesh.
            thisEntity.AddComponent(rcTorsoShadow = new RenderComponent(new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", UpvoidMiner.ModDomain), Resources.UseMesh("Miner/Torso", UpvoidMiner.ModDomain), mat4.Identity),
                                                                        torsoTransform,
                                                                        true));
            /*psTorsoSteam = CpuParticleSystem.Create2D(new vec3(), ContainingWorld);
            LocalScript.ParticleEntity.AddComponent(new CpuParticleComponent(psTorsoSteam, mat4.Identity));*/


            // Add camera component.
            thisEntity.AddComponent(cameraComponent = new CameraComponent(camera, mat4.Identity));

            // This digging controller will perform digging and handle digging constraints for us.
            digging = new DiggingController(ContainingWorld, this);

            Gui = new PlayerGui(this);

            AddTriggerSlot("AddItem");

            Inventory.InitCraftingRules();
            generateInitialItems();

            SetPosition(SpawnPosition);
        }

        public void Update(float elapsedSeconds)
        {
            // Tell AudioEngine where the listener is at the moment
            Audio.SetListenerPosition(camera);

            // Use current item?
            if (isUsingItem && (DateTime.Now - lastItemUse).TotalMilliseconds > millisecondsBetweenItemUsages)
            {
                TriggerItemUse();
                lastItemUse = DateTime.Now;
            }

            // Update drones.
            foreach (var drone in Drones)
                drone.Update(elapsedSeconds);
            foreach (var dc in DroneConstraints)
                dc.Update(elapsedSeconds);

            if (!LocalScript.NoclipEnabled)
            {
                // Update direction.
                vec3 camDir = CameraDirection;
                vec3 camLeft = vec3.cross(vec3.UnitY, camDir).Normalized;
                vec3 camUp = vec3.cross(camDir, camLeft);

                float mix = (float)Math.Pow(0.01, elapsedSeconds);
                vec3 targetDir = camDir;
                vec3 dir = Direction;
                dir.x = dir.x * mix + targetDir.x * (1 - mix);
                dir.z = dir.z * mix + targetDir.z * (1 - mix);
                Direction = dir.Normalized;

                // Update player model.
                vec3 up = new vec3(0, 1, 0);
                vec3 left = vec3.cross(up, Direction);
                mat4 viewMat = new mat4(left, up, Direction, new vec3());
                rcTorsoShadow.Transform =
                   viewMat * torsoTransform;

                // Update camera component.
                cameraComponent.Camera = camera;
                
                // Also add 10cm of forward.xz direction for a "head offset"
                vec3 forward = Direction;
                forward.y = 0;
                cameraComponent.Transform = new mat4(-camLeft, camUp, -camDir, new vec3()) * mat4.Translate(forward * .1f);

                // Re-Center mouse if UI is not open.
                if ( !Gui.IsInventoryOpen )
                {
                    Rendering.MainViewport.SetMouseVisibility(false);
                    Rendering.MainViewport.SetMouseGrab(true);
                }
                else
                {
                    Rendering.MainViewport.SetMouseVisibility(true);
                    Rendering.MainViewport.SetMouseGrab(false);
                }
            }
            else
            {
                cameraComponent.Camera = null;
                Rendering.MainViewport.SetMouseVisibility(true);
                Rendering.MainViewport.SetMouseGrab(false);
            }

            mat4 steamTransform = thisEntity.Transform * rcTorsoShadow.Transform * torsoSteamOffset; 
            vec3 steamOrigin = new vec3(steamTransform * new vec4(0, 0, 0, 1));
            vec3 steamVeloMin = new vec3(steamTransform * new vec4(.13f, 0.05f, 0, 0));
            vec3 steamVeloMax = new vec3(steamTransform * new vec4(.16f, 0.07f, 0, 0));
            /*psTorsoSteam.SetSpawner2D(.03f, new BoundingSphere(steamOrigin, .01f), 
                                      steamVeloMin, steamVeloMax,
                                      new vec4(new vec3(.9f), .8f), new vec4(new vec3(.99f), .9f),
                                      2.0f, 3.4f,
                                      .1f, .2f,
                                      0, 360,
                                      -.2f, .2f);*/

            // Update item preview.
            if (Inventory.Selection != null && Inventory.Selection.HasRayPreview)
            {

                float minRayQueryRange;
                float maxRayQueryRange;
                if (!LocalScript.NoclipEnabled && !GodMode)
                {
                    minRayQueryRange = minRayQueryDistancePlayer;
                    maxRayQueryRange = maxRayQueryDistancePlayer;
                }
                else
                {
                    minRayQueryRange = minRayQueryDistanceNoClip;
                    maxRayQueryRange = maxRayQueryDistanceNoClip;
                }

                // Send a ray query to find the position on the terrain we are looking at.
                ContainingWorld.Physics.RayQuery(camera.Position + camera.ForwardDirection * minRayQueryRange, camera.Position + camera.ForwardDirection * maxRayQueryRange, delegate(bool _hit, vec3 _position, vec3 _normal, RigidBody _body, bool _hasTerrainCollision)
                {
                    Item selection = Inventory.Selection;
                    // Receiving the async ray query result here
                    if (_hit)
                    {
                        /// Subtract a few cm toward camera to increase stability near constraints.
                        _position -= camera.ForwardDirection * .04f;

                        if (selection != null)
                            selection.OnRayPreview(this, _position, _normal, true);
                    }
                    else if (selection != null)
                        selection.OnRayPreview(this, vec3.Zero, vec3.Zero, false);
                });
            }
            if (Inventory.Selection != null && Inventory.Selection.HasUpdatePreview)
                Inventory.Selection.OnUpdatePreview(this, elapsedSeconds);

			// Notify the gui if the player freezing status has changed since the last update frame.
			if (WasFrozen != IsFrozen)
				Gui.OnUpdate();
			WasFrozen = IsFrozen;
        }

        public void Lookaround(vec2 angleDelta)
        {
            AngleAzimuth += angleDelta.x;
            float newAngle = AngleElevation + angleDelta.y;
            if (newAngle < -89.8f) newAngle = -89.8f;
            if (newAngle > 89.8f) newAngle = 89.8f;
            AngleElevation = newAngle;
        }

        public void SetPosition(vec3 position)
        {
            character.Body.SetTransformation(mat4.Translate(position));
        }

        private bool isUsingItem = false;
        private DateTime lastItemUse = DateTime.Now;

        public void TriggerItemUse()
        {
            float minRayQueryRange;
            float maxRayQueryRange;
            if (LocalScript.NoclipEnabled || GodMode)
            {
                minRayQueryRange = minRayQueryDistanceNoClip;
                maxRayQueryRange = maxRayQueryDistanceNoClip;
            }
            else
            {
                minRayQueryRange = minRayQueryDistancePlayer;
                maxRayQueryRange = maxRayQueryDistancePlayer;
            }

            // Send a ray query to find the position on the terrain we are looking at.
            ContainingWorld.Physics.RayQuery(camera.Position + camera.ForwardDirection * minRayQueryRange, camera.Position + camera.ForwardDirection * maxRayQueryRange, delegate(bool _hit, vec3 _position, vec3 _normal, RigidBody _body, bool _hasTerrainCollision)
            {
                // Receiving the async ray query result here
                if (_hit)
                {
                    Entity _hitEntity = null;
                    if (_body != null && _body.RefComponent != null)
                    {
                        _hitEntity = _body.RefComponent.Entity;
                    }

                    /// Subtract a few cm toward camera to increase stability near constraints.
                    _position -= camera.ForwardDirection * .04f;

                    // Use currently selected item.
                    Item selection = Inventory.Selection;
                    if (selection != null)
                        selection.OnUse(this, _position, _normal, _hitEntity);
                }
            });
        }

        public void StartItemUse()
        {
            isUsingItem = true;
            TriggerItemUse();
            lastItemUse = DateTime.Now;
        }

        public void StopItemUse()
        {
            isUsingItem = false;
        }

        public void TriggerInteraction()
        {
            float minRayQueryRange;
            float maxRayQueryRange;
            if (LocalScript.NoclipEnabled || GodMode)
            {
                minRayQueryRange = minRayQueryDistanceNoClip;
                maxRayQueryRange = maxRayQueryDistanceNoClip;
            }
            else
            {
                minRayQueryRange = minRayQueryDistancePlayer;
                maxRayQueryRange = maxRayQueryDistancePlayer;
            }

            ContainingWorld.Physics.RayQuery(camera.Position + camera.ForwardDirection * minRayQueryRange, camera.Position + camera.ForwardDirection * maxRayQueryRange, delegate(bool _hit, vec3 _position, vec3 _normal, RigidBody _body, bool _hasTerrainCollision)
            {
                // Receiving the async ray query result here
                if (_body != null && _body.RefComponent != null)
                {
                    Entity entity = _body.RefComponent.Entity;
                    if (entity != null)
                    {
                        TriggerId trigger = TriggerId.getIdByName("Interaction");
                        entity[trigger] |= new InteractionMessage(thisEntity);
                    }
                }
            });
        }

        /// <summary>
        /// Drops an item.
        /// </summary>
        public void DropItem(Item item)
        {
            if (item.IsDroppable)
            {
                Item droppedItem = item.Clone();
                
                if(droppedItem is DiscreteItem)
                {
                    var dItem = droppedItem as DiscreteItem;
                    dItem.StackSize = 1;
                }

                // Keep all items in god mode
                if(!GodMode)
                    Inventory.RemoveItem(droppedItem);

                ItemEntity itemEntity = new ItemEntity(droppedItem, false);
                ContainingWorld.AddEntity(itemEntity, mat4.Translate(Position + vec3.UnitY*1f + CameraDirection*1f));
                itemEntity.ApplyImpulse(CameraDirection*200f, new vec3(0, .3f, 0));
            }
        }

        /// <summary>
        /// Adds all active drone constraints to a Csg Diff Node.
        /// </summary>
        public void AddDroneConstraints(CsgOpDiff diffNode, vec3 refPos)
        {
            foreach (var dc in DroneConstraints)
                dc.AddCsgConstraints(diffNode, refPos);
        }

        [Serializable]
        public class InventorySave
        {
            [Serializable]
            public class ResourceItemSave 
            {
                public long Id;
                public string Resource;
                public float Volume;
            }
            [Serializable]
            public class ToolItemSave 
            {
                public long Id;
                public ToolType Type;
                public int StackSize;
            }
            [Serializable]
            public class MaterialItemSave 
            {
                public long Id;
                public MaterialShape Shape;
                public float SizeX;
                public float SizeY;
                public float SizeZ;
                public string Resource;
                public int StackSize;
            }
            
            public List<ResourceItemSave> resourceItems = new List<ResourceItemSave>();
            public List<ToolItemSave> toolItems = new List<ToolItemSave>();
            public List<MaterialItemSave> materialItems = new List<MaterialItemSave>();

            public long[] quickAccess;
            public int currentQuickAccess;
        }

        /// <summary>
        /// Saves the player
        /// </summary>
        public void Save()
        {
            saveInventory();

            Console.WriteLine("[" + DateTime.Now + "] Player saved.");
        }

        void saveInventory()
        {
            InventorySave save = new InventorySave();
            foreach (var item in Inventory.Items)
            {
                if (item is ToolItem)
                {
                    save.toolItems.Add(new InventorySave.ToolItemSave
                    {
                        Id = item.Id,
                        StackSize = (item as ToolItem).StackSize,
                        Type = (item as ToolItem).ToolType
                    });
                }
                else if (item is MaterialItem)
                {
                    save.materialItems.Add(new InventorySave.MaterialItemSave
                    {
                        Id = item.Id,
                        StackSize = (item as MaterialItem).StackSize,
                        SizeX = (item as MaterialItem).Size.x,
                        SizeY = (item as MaterialItem).Size.y,
                        SizeZ = (item as MaterialItem).Size.z,
                        Shape = (item as MaterialItem).Shape,
                        Resource = (item as MaterialItem).Material.Name
                    });
                }
                else if (item is ResourceItem)
                {
                    save.resourceItems.Add(new InventorySave.ResourceItemSave
                    {
                        Id = item.Id,
                        Volume = (item as ResourceItem).Volume,
                        Resource = (item as ResourceItem).Material.Name,
                    });
                }
                //else
                //    throw new InvalidDataException("Unknown item type: " + item.GetType());
            }

            save.quickAccess = new long[Inventory.QuickAccessSlotCount];
            for (int i = 0; i < Inventory.QuickAccessSlotCount; ++i)
                save.quickAccess[i] = Inventory.QuickAccessItems[i] == null ? -1 : Inventory.QuickAccessItems[i].Id;
            save.currentQuickAccess = Inventory.SelectionIndex;

            Directory.CreateDirectory(new FileInfo(UpvoidMiner.SavePathInventory).Directory.FullName);
            File.WriteAllText(UpvoidMiner.SavePathInventory, JsonConvert.SerializeObject(save, Formatting.Indented));
        }

        /// <summary>
        /// Populates the inventory with a list of items that we start with.
        /// </summary>
        void generateInitialItems()
        {
            if (GodMode)
            {
                Inventory.AddItem(new ToolItem(ToolType.GodsShovel));
                Inventory.AddItem(new ToolItem(ToolType.Shovel));
                Inventory.AddItem(new ToolItem(ToolType.Axe));
                Inventory.AddItem(new PipetteItem());
                IEnumerable<TerrainResource> resources = TerrainResource.ListResources();
                foreach (var resource in resources)
                {
                    Inventory.AddResource(resource, 1e9f);
                }
            }
            else if (!File.Exists(UpvoidMiner.SavePathInventory))
            {
                // Tools
                Inventory.AddItem(new ToolItem(ToolType.Shovel));
                Inventory.AddItem(new ToolItem(ToolType.Pickaxe));
                Inventory.AddItem(new ToolItem(ToolType.Axe));
                //Inventory.AddItem(new ToolItem(ToolType.Hammer));
                Inventory.AddItem(new ToolItem(ToolType.DroneChain, 5));

                // Testing resource/material items.
                /*TerrainResource dirt = ContainingWorld.Terrain.QueryMaterialFromName("Dirt");
                TerrainResource stone06 = ContainingWorld.Terrain.QueryMaterialFromName("Stone.06"); 
                Inventory.AddResource(dirt, 10);
                Inventory.AddItem(new ResourceItem(dirt, 3f));
                Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Sphere, new vec3(1)));
                Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Sphere, new vec3(1), 2));
                Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Cylinder, new vec3(1,2,2)));
                Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Sphere, new vec3(2)));
                Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Cube, new vec3(2)));
                Inventory.AddItem(new MaterialItem(stone06, MaterialShape.Cylinder, new vec3(1,2,2)));
                Inventory.AddItem(new MaterialItem(dirt, MaterialShape.Sphere, new vec3(1)));*/
                Inventory.AddItem(new MaterialItem(TerrainResource.FromName("AoiCrystal"), MaterialShape.Sphere, new vec3(1)));
                Inventory.AddItem(new MaterialItem(TerrainResource.FromName("FireRock"), MaterialShape.Cube, new vec3(1)));
                Inventory.AddItem(new MaterialItem(TerrainResource.FromName("AlienRock"), MaterialShape.Cylinder, new vec3(1)));
            }
            else // Load inventory
            {
                InventorySave save = JsonConvert.DeserializeObject<InventorySave>(File.ReadAllText(UpvoidMiner.SavePathInventory));

                Dictionary<long, Item> id2item = new Dictionary<long, Item>();
                foreach (var item in save.toolItems)
                {
                    id2item.Add(item.Id, new ToolItem(item.Type, item.StackSize));
                    Inventory.AddItem(id2item[item.Id]);
                }
                foreach (var item in save.materialItems)
                {
                    id2item.Add(item.Id, new MaterialItem(TerrainResource.FromName(item.Resource), item.Shape, new vec3(item.SizeX, item.SizeY, item.SizeZ), item.StackSize));
                    Inventory.AddItem(id2item[item.Id]);
                }
                foreach (var item in save.resourceItems)
                {
                    id2item.Add(item.Id, new ResourceItem(TerrainResource.FromName(item.Resource), item.Volume));
                    Inventory.AddItem(id2item[item.Id]);
                }

                Inventory.ClearQuickAccess();
                for (int i = 0; i < Inventory.QuickAccessSlotCount; ++i)
                    if (id2item.ContainsKey(save.quickAccess[i]))
                        Inventory.SetQuickAccess(id2item[save.quickAccess[i]], i);
                Inventory.SelectQuickAccessSlot(save.currentQuickAccess);
            }

            Gui.OnUpdate();
        }

        /// <summary>
        /// Adds a drone at the given position.
        /// Does not remove any drone from the inventory.
        /// </summary>
        public void AddDrone(vec3 position)
        {
            Drone d = new Drone(position + new vec3(0, 1, 0), this, DroneType.Chain);
            Drones.Add(d);

            // Add drone to the first constraint it can be added to.
            bool foundConstraint = false;
            foreach (var dc in DroneConstraints)
            {
                if (dc.IsAddable(d))
                {
                    // Drone is addable, i.e. the constraint contains a drone of the same type
                    dc.AddDrone(d);
                    foundConstraint = true;
                    break;
                }
            }

            // If no constraint was found, we add a new one and add the drone to that.
            if (!foundConstraint)
                DroneConstraints.Add(new DroneConstraint(d));

            // Add the drone as new entity.
            ContainingWorld.AddEntity(d, mat4.Translate(d.CurrentPosition), Network.GCManager.CurrentUserID);
        }
        /// <summary>
        /// Removes a drone from drone contraints.
        /// </summary>
        public void RemoveDrone(Drone drone)
        {
            foreach (var dc in DroneConstraints)
            {
                dc.RemoveDrone(drone);

                // In case removing the drone resulted in an "empty" constraint, delete that.
                if (dc.ContainsDrones() == false)
                {
                    DroneConstraints.Remove(dc);
                    break;
                }
            }

            Drones.Remove(drone);
        }

        /// <summary>
        /// Aligns a position according to the current alignment rules
        /// </summary>
        public vec3 AlignPlacementPosition(vec3 pos)
        {
            if (CurrentDiggingAlignment == DiggingAlignment.GridAligned)
                return new vec3(
                    (int)Math.Round(pos.x * 2),
                    (int)Math.Round(pos.y * 2),
                    (int)Math.Round(pos.z * 2)
                ) * 0.5f;
            else return pos;
        }

        /// <summary>
        /// Calculates an alignment system for digging/constructing
        /// </summary>
        public void AlignmentSystem(vec3 worldNormal, out vec3 dirX, out vec3 dirY, out vec3 dirZ)
        {
            switch (CurrentDiggingAlignment)
            {
                case Player.DiggingAlignment.GridAligned:
                case Player.DiggingAlignment.AxisAligned:
                    dirX = vec3.UnitX;
                    dirY = vec3.UnitY;
                    dirZ = vec3.UnitZ;
                    break;
                case Player.DiggingAlignment.PlayerAligned:
                    dirX = camera.RightDirection.Normalized;
                    dirZ = camera.UpDirection.Normalized;
                    dirY = camera.ForwardDirection.Normalized;
                    break;
                case Player.DiggingAlignment.TerrainAligned:
                    dirY = worldNormal.Normalized;
                    dirX = vec3.cross(camera.ForwardDirection, dirY).Normalized;
                    dirZ = vec3.cross(dirX, dirY).Normalized;
                    break;
                default: throw new InvalidOperationException("Unknown alignment");
            }
        }

        /// <summary>
		/// Places the current digging shape shape of a given material
        /// </summary>
        public void PlaceMaterial(TerrainResource material, vec3 worldNormal, vec3 position, float radius)
        {
            position = AlignPlacementPosition(position);

            bool paintMode = false;
            bool fullModification = false;

            var filterMats = paintMode || fullModification ? null : new int[] { 0 };
            bool allowAirChange = !paintMode;

			switch(CurrentDiggingShape)
			{
				case DiggingShape.Sphere:
                    digging.DigSphere(worldNormal, position, radius, filterMats, material.Index, DiggingController.DigMode.Add, allowAirChange);
                    break;
                case DiggingShape.Box:
                    digging.DigBox(worldNormal, position, radius, filterMats, material.Index, DiggingController.DigMode.Add, allowAirChange);
                    break;
                case DiggingShape.Cylinder:
                    digging.DigCylinder(worldNormal, position, radius, filterMats, material.Index, DiggingController.DigMode.Add, allowAirChange);
                    break;
				default:
					throw new Exception("Unsupported digging shape used");
			}
        }
        /// <summary>
		/// Places the current digging shape at a given position with a given radius.
        /// </summary>
        public void DigMaterial(vec3 worldNormal, vec3 position, float radius, IEnumerable<int> filterMaterials)
        {
            position = AlignPlacementPosition(position);

			switch (CurrentDiggingShape)
			{
				case DiggingShape.Sphere:
                    digging.DigSphere(worldNormal, position, radius, filterMaterials);
                    break;
                case DiggingShape.Box:
                    digging.DigBox(worldNormal, position, radius, filterMaterials);
                    break;
                case DiggingShape.Cylinder:
                    digging.DigCylinder(worldNormal, position, radius, filterMaterials);
                    break;
				default:
					throw new Exception("Unsupported digging shape used");
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
            if (addItemMsg == null)
                return;

            // Add the received item to the inventory.
            Inventory.AddItem(addItemMsg.PickedItem);

        }

    }
}

