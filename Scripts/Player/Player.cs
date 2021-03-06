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
using System.Linq;
using System.Runtime.Serialization;
using Engine;
using Engine.Audio;
using Engine.Universe;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Rendering;
using Engine.Statistics;
using Engine.Physics;
using Engine.Network;
using Engine.Input;
using System.IO;
using Newtonsoft.Json;
using UpvoidMiner.Items;
using UpvoidMiner.UI;

namespace UpvoidMiner
{
    public class CrosshairInfo
    {
        public string IconClass;
        public string IconColor;
        public bool Disabled;

        public void Reset()
        {
            IconClass = "plus";
            IconColor = "#fff";
            Disabled = false;
        }
    }

    /// <summary>
    /// Contains the game logic and the internal state of the player character.
    /// </summary>
    public class Player : EntityScript
    {
        /// <summary>
        /// The render component for the torso.
        /// </summary>
        //private RenderComponent rcTorsoShadow;
        //private CpuParticleSystemBase psTorsoSteam;
        private mat4 torsoSteamOffset = mat4.Translate(new vec3(.13090f, .53312f, -.14736f));
        /// <summary>
        /// Relative torso transformation.
        /// </summary>
        private mat4 torsoTransform = mat4.Scale(2f) * mat4.Translate(new vec3(0, -.5f, 0));
        /// <summary>
        /// Minimum and maximum ranges for ray querys for first-person and no-clip mode, resp.
        /// </summary>
        const float maxRayQueryDistancePlayer = 10.0f;
        const float maxRayQueryDistanceNoClip = 200.0f;
        private const int millisecondsBetweenItemUsages = 250;
        public static vec3 SpawnPosition = new vec3(150, 5, 150);
        /// <summary>
        /// Current crosshair info
        /// </summary>
        public readonly CrosshairInfo Crosshair = new CrosshairInfo();

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

        public DiggingController.DigShape CurrentDiggingShape { get; set; }

        public DiggingController.DigAlignment CurrentDiggingAlignment { get; set; }

        public int DiggingAlignmentAxisRotation { get; set; }

        public int DiggingGridSize { get; set; }

        public DiggingController.DigPivot CurrentDiggingPivot { get; set; }

        public DiggingController.PhysicsMode CurrentPhysicsMode { get; set; }

        public DiggingController.AddMode CurrentDiggingAddMode { get; set; }

        public Player(GenericCamera _camera, bool _godMode)
        {
            GodMode = _godMode;
            Direction = new vec3(1, 0, 0);
            camera = _camera;
            CurrentDiggingShape = DiggingController.DigShape.Sphere;
            CurrentDiggingAlignment = DiggingController.DigAlignment.Axis;
            DiggingAlignmentAxisRotation = 0;
            DiggingGridSize = 2;
            CurrentDiggingAddMode = DiggingController.AddMode.AirOnly;
            CurrentDiggingPivot = DiggingController.DigPivot.Center;
            CurrentPhysicsMode = DiggingController.PhysicsMode.Dynamic;
            Inventory = new Inventory(this);
        }

        protected override void Init()
        {
            // Create a character controller that allows us to walk around.
            if (ContainingWorld == null)
                throw new InvalidOperationException();

            camera.Position = thisEntity.Position;

            input = new InputController(this);

            character = new CharacterController(camera, ContainingWorld, GodMode, GodMode ? 0.45f : 1.85f);

            // For now, attach this entity to a simple sphere physics object.
            character.Body.SetTransformation(thisEntity.Transform);
            thisEntity.AddComponent(new PhysicsComponent(
                character.Body,
                mat4.Translate(new vec3(0, (GodMode ? 0f : character.EyeOffset), 0))));

            // Add Torso mesh.
            /*thisEntity.AddComponent(rcTorsoShadow = new RenderComponent(new MeshRenderJob(Renderer.Shadow.Mesh, Resources.UseMaterial("::Shadow", UpvoidMiner.ModDomain), Resources.UseMesh("Miner/Torso", UpvoidMiner.ModDomain), mat4.Identity),
                                                                        torsoTransform,
                                                                        true));*/
            /*psTorsoSteam = CpuParticleSystem.Create2D(new vec3(), ContainingWorld);
            LocalScript.ParticleEntity.AddComponent(new CpuParticleComponent(psTorsoSteam, mat4.Identity));*/


            // Add camera component.
            thisEntity.AddComponent(cameraComponent = new CameraComponent(camera, mat4.Identity));

            // This digging controller will perform digging and handle digging constraints for us.
            digging = new DiggingController(ContainingWorld, this);

            Gui = new PlayerGui(this);

            AddTriggerSlot("AddItem");

            generateInitialItems();
            LoadWorldItem();

            SetPosition(SpawnPosition);

            // reset crosshair
            Crosshair.Reset();
        }

        public void Update(float elapsedSeconds)
        {
            // Update character controller
            Character.Update(elapsedSeconds);

            using (new ProfileAction("Player::Update", UpvoidMiner.Mod))
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

                bool menuOrInventoryOpen = Gui.IsInventoryOpen || Gui.IsMenuOpen;

                if (!LocalScript.NoclipEnabled)
                {
                    // Update camera when no menu/inventory is open
                    if (!menuOrInventoryOpen)
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
                        /*rcTorsoShadow.Transform =
                       viewMat * torsoTransform;*/

                        // Update camera component.
                        cameraComponent.Camera = camera;

                        // Also add 10cm of forward.xz direction for a "head offset"
                        vec3 forward = Direction;
                        forward.y = 0;
                        cameraComponent.Transform = new mat4(-camLeft, camUp, -camDir, new vec3()) * mat4.Translate(forward * .1f);

                        // Re-Center mouse if UI is not open.
                        Rendering.MainViewport.SetMouseVisibility(false);
                        Rendering.MainViewport.SetMouseGrab(true);
                    }
                    else
                    {
                        // UI is open. Show mouse.
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

                /*mat4 steamTransform = thisEntity.Transform * rcTorsoShadow.Transform * torsoSteamOffset;
            vec3 steamOrigin = new vec3(steamTransform * new vec4(0, 0, 0, 1));
            vec3 steamVeloMin = new vec3(steamTransform * new vec4(.13f, 0.05f, 0, 0));
            vec3 steamVeloMax = new vec3(steamTransform * new vec4(.16f, 0.07f, 0, 0));*/
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
                    float maxRayQueryRange;
                    if (!LocalScript.NoclipEnabled && !GodMode)
                    {
                        maxRayQueryRange = maxRayQueryDistancePlayer;
                    }
                    else
                    {
                        maxRayQueryRange = maxRayQueryDistanceNoClip;
                    }

                    // Send a ray query to find the position on the terrain we are looking at.
                    RayHit hit = ContainingWorld.Physics.RayTest(camera.Position, camera.Position + camera.ForwardDirection * maxRayQueryRange, character.Body);
                    Item selection = Inventory.Selection;
                    if (hit != null)
                    {
                        /// Subtract a few cm toward camera to increase stability near constraints.
                        vec3 pos = hit.Position - camera.ForwardDirection * .04f;

                        if (selection != null)
                        {
                            Crosshair.Reset();
                            selection.OnRayPreview(this, hit, Crosshair);
                        }
                    }
                    else if (selection != null)
                    {
                        Crosshair.Reset();
                        selection.OnRayPreview(this, null, Crosshair);
                    }
                }
                if (Inventory.Selection != null && Inventory.Selection.HasUpdatePreview)
                    Inventory.Selection.OnUpdatePreview(this, elapsedSeconds, Crosshair);

                if (Inventory.Selection == null)
                    Crosshair.Reset();

                // Notify the gui if the player freezing status has changed since the last update frame.
                if (WasFrozen != IsFrozen)
                    Gui.OnUpdate();
                WasFrozen = IsFrozen;
            }
        }

        public void Lookaround(vec2 angleDelta)
        {
            AngleAzimuth += angleDelta.x;
            float newAngle = AngleElevation + angleDelta.y;
            if (newAngle < -89.8f)
                newAngle = -89.8f;
            if (newAngle > 89.8f)
                newAngle = 89.8f;
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
            float maxRayQueryRange;
            if (LocalScript.NoclipEnabled || GodMode)
            {
                maxRayQueryRange = maxRayQueryDistanceNoClip;
            }
            else
            {
                maxRayQueryRange = maxRayQueryDistancePlayer;
            }

            // Send a ray query to find the position on the terrain we are looking at.
            RayHit hit = ContainingWorld.Physics.RayTest(camera.Position, camera.Position + camera.ForwardDirection * maxRayQueryRange, Character.Body);
            if (hit != null)
            {
                Entity hitEntity = null;
                RigidBody body = hit.CollisionBody;
                if (body != null && body.RefComponent != null)
                {
                    hitEntity = body.RefComponent.Entity;
                }

                /// Subtract a few cm toward camera to increase stability near constraints.
                vec3 pos = hit.Position - camera.ForwardDirection * .04f;

                // Use currently selected item.
                Item selection = Inventory.Selection;
                if (selection != null)
                    selection.OnUse(this, pos, hit.Normal, hitEntity);
            }
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
            float maxRayQueryRange;
            if (LocalScript.NoclipEnabled || GodMode)
            {
                maxRayQueryRange = maxRayQueryDistanceNoClip;
            }
            else
            {
                maxRayQueryRange = maxRayQueryDistancePlayer;
            }

            RayHit hit = ContainingWorld.Physics.RayTest(camera.Position, camera.Position + camera.ForwardDirection * maxRayQueryRange, Character.Body);
            RigidBody body = hit == null ? null : hit.CollisionBody;
            if (body != null && body.RefComponent != null)
            {
                Entity entity = body.RefComponent.Entity;
                if (entity != null)
                {
                    TriggerId trigger = TriggerId.getIdByName("Interaction");
                    entity[trigger] |= new InteractionMessage(thisEntity);
                }
            }
        }

        /// <summary>
        /// Drops an item.
        /// An optional position for the direction of throw
        /// </summary>
        public void DropItem(Item item, vec3? worldPos = null)
        {
            if (item.IsDroppable)
            {
                Item droppedItem = item.Clone();

                if (droppedItem is DiscreteItem)
                {
                    var dItem = droppedItem as DiscreteItem;
                    dItem.StackSize = 1;
                }

                // Keep all items in god mode
                if (!GodMode)
                    Inventory.RemoveItem(droppedItem);

                var dir = worldPos.HasValue ? (worldPos.Value - Position).Normalized : CameraDirection;
                var entity = ItemManager.InstantiateItem(droppedItem, mat4.Translate(Position + vec3.UnitY * 1f + CameraDirection * 1f), false);
                entity.ApplyImpulse(dir * entity.Mass * 10f, new vec3(0, .3f, 0));
            }
        }

        /// <summary>
        /// Converts one item to terrain resource
        /// </summary>
        public void Convert(Item item, bool convertAll)
        {
            if (item.IsDroppable && item is MaterialItem)
            {
                var matItem = item as MaterialItem;
                if (convertAll)
                {
                    Inventory.AddResource(matItem.Substance, matItem.Volume * (item as DiscreteItem).StackSize);
                    if (matItem.Substance is WoodSubstance)
                        Tutorials.MsgBasicRecipeConvertWood.Report(matItem.Volume * (item as DiscreteItem).StackSize);
                    if (!GodMode)
                        Inventory.RemoveItem(item);
                    return;
                }
                Item droppedItem = item.Clone();

                var dItem = droppedItem as DiscreteItem;
                dItem.StackSize = 1;

                Inventory.AddResource(matItem.Substance, matItem.Volume);
                if (matItem.Substance is WoodSubstance)
                    Tutorials.MsgBasicRecipeConvertWood.Report(matItem.Volume);

                // Keep all items in god mode
                if (!GodMode)
                    Inventory.RemoveItem(droppedItem);
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
        public class ItemSave
        {
            [Serializable]
            public class ResourceItemSave
            {
                public long Id;
                public string Substance;
                public float Volume;
            }

            [Serializable]
            public class PipetteItemSave
            {
                public long Id;
                public int StackSize;
            }

            [Serializable]
            public class ToolItemSave
            {
                public long Id;
                public ToolType Type;
                public string Substance;
                public int StackSize;
                public double Durability;
            }

            [Serializable]
            public class MaterialItemSave
            {
                public long Id;
                public MaterialShape Shape;
                public float SizeX;
                public float SizeY;
                public float SizeZ;
                public string Substance;
                public int StackSize;
            }

            [Serializable]
            public class RecipeItemSave
            {
                public long Id;
                public ItemSave ResultSave;
                public List<ItemSave> IngredientItemSaves;
            }

            [Serializable]
            public class CraftingItemSave
            {
                public long Id;
                public CraftingItem.ItemType Type;
                public string Substance;
                public int StackSize;
            }

            [Serializable]
            public class TorchItemSave
            {
                public long Id;
                public int StackSize;
            }

            public ResourceItemSave ResourceItem;
            public ToolItemSave ToolItem;
            public MaterialItemSave MaterialItem;
            public PipetteItemSave PipetteItem;
            public RecipeItemSave RecipeItem;
            public CraftingItemSave CraftingItem;
            public TorchItemSave TorchItem;

            public long Id()
            {
                if (ResourceItem != null)
                    return ResourceItem.Id;
                if (ToolItem != null)
                    return ToolItem.Id;
                if (MaterialItem != null)
                    return MaterialItem.Id;
                if (CraftingItem != null)
                    return CraftingItem.Id;
                if (RecipeItem != null)
                    return RecipeItem.Id;
                if (TorchItem != null)
                    return TorchItem.Id;
                return -1;
            }

            public Item DeserializeItem()
            {
                if (ResourceItem != null)
                    return new ResourceItem(Substance.Deserialize(ResourceItem.Substance), ResourceItem.Volume);
                if (ToolItem != null)
                    return new ToolItem(ToolItem.Type, Substance.Deserialize(ToolItem.Substance), ToolItem.StackSize) { Durability = ToolItem.Durability };
                if (MaterialItem != null)
                    return new MaterialItem(Substance.Deserialize(MaterialItem.Substance), MaterialItem.Shape, new vec3(MaterialItem.SizeX, MaterialItem.SizeY, MaterialItem.SizeZ), MaterialItem.StackSize);
                if (PipetteItem != null)
                    return new PipetteItem(PipetteItem.StackSize);
                if (RecipeItem != null)
                    return new RecipeItem(RecipeItem.ResultSave.DeserializeItem(), RecipeItem.IngredientItemSaves.Select(ingredient => ingredient.DeserializeItem()).ToList());
                if (CraftingItem != null)
                    return new CraftingItem(CraftingItem.Type, Substance.Deserialize(CraftingItem.Substance), CraftingItem.StackSize);
                if (TorchItem != null)
                    return new TorchItem(TorchItem.StackSize);
                return null;
            }
        }

        [Serializable]
        public class WorldItemSave
        {
            [Serializable]
            public class WorldItem
            {
                public ItemSave Item;
                public mat4 Transform;
                public bool FixedPosition;
            }

            public List<WorldItem> items = new List<WorldItem>();
        }

        [Serializable]
        public class InventorySave
        {
            public const int SaveVersion = 2;
            public int Version = -1;
            public List<ItemSave> items = new List<ItemSave>();
            public long[] quickAccess;
            public int currentQuickAccess;

            public static ItemSave saveObj(Item item)
            {
                if (item == null)
                    return new ItemSave();


                if (item is ToolItem)
                {
                    return new ItemSave
                    {
                        ToolItem = new ItemSave.ToolItemSave
                            {
                                Id = item.Id,
                                StackSize = (item as ToolItem).StackSize,
                                Substance = (item as ToolItem).Substance.Serialize(),
                                Type = (item as ToolItem).ToolType,
                                Durability = (item as ToolItem).Durability
                            }
                    };
                }
                else if (item is MaterialItem)
                {
                    return new ItemSave
                    {
                        MaterialItem = new ItemSave.MaterialItemSave
                            {
                                Id = item.Id,
                                StackSize = (item as MaterialItem).StackSize,
                                SizeX = (item as MaterialItem).Size.x,
                                SizeY = (item as MaterialItem).Size.y,
                                SizeZ = (item as MaterialItem).Size.z,
                                Shape = (item as MaterialItem).Shape,
                                Substance = (item as MaterialItem).Substance.Serialize()
                            }
                    };
                }
                else if (item is ResourceItem)
                {
                    return new ItemSave
                    {
                        ResourceItem = new ItemSave.ResourceItemSave
                            {
                                Id = item.Id,
                                Volume = (item as ResourceItem).Volume,
                                Substance = (item as ResourceItem).Substance.Serialize(),
                            }
                    };
                }
                else if (item is PipetteItem)
                {
                    return new ItemSave
                    {
                        PipetteItem = new ItemSave.PipetteItemSave
                        {
                            Id = item.Id,
                            StackSize = (item as PipetteItem).StackSize
                        }
                    };
                }
                else if (item is RecipeItem)
                {
                    return new ItemSave
                    {
                        RecipeItem = new ItemSave.RecipeItemSave
                        {
                            Id = item.Id,
                            ResultSave = saveObj((item as RecipeItem).Result),
                            IngredientItemSaves = (item as RecipeItem).IngredientItems.Select(saveObj).ToList(),
                        }
                    };
                }
                else if (item is CraftingItem)
                {
                    return new ItemSave
                    {
                        CraftingItem = new ItemSave.CraftingItemSave
                        {
                            Id = item.Id,
                            Type = (item as CraftingItem).Type,
                            Substance = (item as CraftingItem).Substance.Serialize(),
                            StackSize = (item as CraftingItem).StackSize
                        }
                    };
                }
                else if (item is TorchItem)
                {
                    return new ItemSave
                    {
                        TorchItem = new ItemSave.TorchItemSave
                        {
                            Id = item.Id,
                            StackSize = (item as TorchItem).StackSize,
                        }
                    };
                }
                else
                    throw new InvalidDataException("Unknown item type: " + item.GetType());
            }
        }

        /// <summary>
        /// Saves the player
        /// </summary>
        public void Save()
        {
            saveInventory();
            saveWorld();
        }

        void saveWorld()
        {
            WorldItemSave save = new WorldItemSave();
            foreach (var kvp in ItemManager.AllItemsEntities)
            {
                save.items.Add(new WorldItemSave.WorldItem()
                {
                    Item = InventorySave.saveObj(kvp.Key),
                    Transform = kvp.Value.thisEntity.Transform,
                    FixedPosition = kvp.Value.FixedPosition
                });
            }
            Directory.CreateDirectory(new FileInfo(UpvoidMiner.SavePathWorldItems).Directory.FullName);
            File.WriteAllText(UpvoidMiner.SavePathWorldItems, JsonConvert.SerializeObject(save, Formatting.Indented));
        }

        void saveInventory()
        {
            InventorySave save = new InventorySave();
            save.Version = InventorySave.SaveVersion;
            foreach (var item in Inventory.Items)
                save.items.Add(InventorySave.saveObj(item));

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
            bool genDefault;

            if (GodMode)
            {
                Inventory.AddItem(new ToolItem(ToolType.GodsShovel, new AegiriumSubstance()));
                Inventory.AddItem(new ToolItem(ToolType.Shovel, new CopperSubstance()));
                Inventory.AddItem(new ToolItem(ToolType.Axe, new CopperSubstance()));
                Inventory.AddItem(new ToolItem(ToolType.DroneChain, new IronSubstance(), 5));
                Inventory.AddItem(new PipetteItem());
                Inventory.AddItem(new TorchItem(1));
                foreach (var resource in TerrainResource.ListResources().Where(resource => resource.MassDensity > 0.0f))
                    Inventory.AddResource(resource.Substance, 1e9f);
                genDefault = false;
            }
            else if (!File.Exists(UpvoidMiner.SavePathInventory))
            {
                genDefault = true;
            }
            else // Load inventory
            {
                var save = JsonConvert.DeserializeObject<InventorySave>(File.ReadAllText(UpvoidMiner.SavePathInventory));
                if (save.Version == InventorySave.SaveVersion)
                {

                    var id2item = new Dictionary<long, Item>();
                    foreach (var item in save.items)
                    {
                        id2item.Add(item.Id(), item.DeserializeItem());
                        Inventory.AddItem(id2item[item.Id()]);
                    }

                    Inventory.ClearQuickAccess();
                    for (int i = 0; i < Inventory.QuickAccessSlotCount; ++i)
                        if (id2item.ContainsKey(save.quickAccess[i]))
                            Inventory.SetQuickAccess(id2item[save.quickAccess[i]], i);
                    Inventory.SelectQuickAccessSlot(save.currentQuickAccess);

                    genDefault = false;
                }
                else
                    genDefault = true;
            }

            if (genDefault)
            {
                // Tools
                Inventory.AddItem(new ToolItem(ToolType.Shovel, new BirchWoodSubstance()));
                Inventory.AddItem(new ToolItem(ToolType.Axe, new BirchWoodSubstance()));
                //Inventory.AddItem(new ToolItem(ToolType.Hammer));
                Inventory.AddItem(new ToolItem(ToolType.DroneChain, new IronSubstance(), 5));

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
                /*Inventory.AddItem(new MaterialItem(TerrainResource.FromName("BlueCrystal"), MaterialShape.Sphere, new vec3(1), 10));
                Inventory.AddItem(new MaterialItem(TerrainResource.FromName("FireRock"), MaterialShape.Cube, new vec3(1), 10));
                Inventory.AddItem(new MaterialItem(TerrainResource.FromName("AlienRock"), MaterialShape.Cylinder, new vec3(1), 10));*/
            }

            // Resupply drones
            var drones = Inventory.Items.Sum(i => i is ToolItem && (i as ToolItem).ToolType == ToolType.DroneChain ? (i as ToolItem).StackSize : 0);
            if (drones < 5)
                Inventory.AddItem(new ToolItem(ToolType.DroneChain, new IronSubstance(), 5 - drones));

            // Resupply hands
            if (Inventory.Items.Sum(i => i is ToolItem && (i as ToolItem).ToolType == ToolType.Hands ? 1 : 0) == 0)
                Inventory.AddItem(new ToolItem(ToolType.Hands, new Substance()));


            // Resupply Recipes
            foreach (var item in Inventory.Items.Reverse().OfType<RecipeItem>())
            {
                Inventory.Items.RemoveItem(item, true);
            }
            Inventory.AddItem(new RecipeItem(new CraftingItem(CraftingItem.ItemType.Handle, new WoodSubstance(), 20),
                new List<Item>
                    {
                        new MaterialItem(new WoodSubstance(), MaterialShape.Cylinder, new vec3(0.3f,0.5f,0.3f))
                    }));

            var matTypes = new List<Substance>
            {
                new WoodSubstance(),
                new StoneSubstance(),
                new CopperSubstance()
            };
            var toolTypes = new List<Tuple<ToolType, CraftingItem.ItemType>>
            {
                Tuple.Create(ToolType.Shovel,CraftingItem.ItemType.ShovelBlade),
                Tuple.Create(ToolType.Pickaxe,CraftingItem.ItemType.PickaxeHead),
                Tuple.Create(ToolType.Axe,CraftingItem.ItemType.AxeHead)
            };

            foreach (var tool in toolTypes)
            {
                foreach (var mat in matTypes)
                {
                    Inventory.AddItem(
                        new RecipeItem(new CraftingItem(tool.Item2, mat),
                            new List<Item>
                            {
                                new ResourceItem(mat, 0.5f)
                            }));
                    Inventory.AddItem(new RecipeItem(new ToolItem(tool.Item1, mat),
                        new List<Item>
                        {
                            new CraftingItem(tool.Item2, mat),
                            new CraftingItem(CraftingItem.ItemType.Handle,new WoodSubstance())
                        }));
                }
            }

            Inventory.AddItem(
                new RecipeItem(new ResourceItem(new CopperSubstance(), 0.1f),
                    new List<Item>
                            {
                                new ResourceItem(new CopperOreSubstance(), 40.0f)
                            }, false));

            Inventory.AddItem(
                new RecipeItem(new ResourceItem(new CharcoalSubstance(), 0.8f),
                    new List<Item>
                            {
                                new ResourceItem(new WoodSubstance(), 1.0f)
                            }, false));

            Inventory.AddItem(
                new RecipeItem(new TorchItem(),
                    new List<Item>
                            {
                                new ResourceItem(new CoalSubstance(), 0.1f),
                                new CraftingItem(CraftingItem.ItemType.Handle,new Substance())
                            }, false));

            Gui.OnUpdate();
        }

        private void LoadWorldItem()
        {
            if (File.Exists(UpvoidMiner.SavePathWorldItems))
            {
                WorldItemSave save = JsonConvert.DeserializeObject<WorldItemSave>(File.ReadAllText(UpvoidMiner.SavePathWorldItems));
                foreach (var item in save.items)
                {
                    ItemManager.InstantiateItem(item.Item.DeserializeItem(), item.Transform, item.FixedPosition);
                }
            }
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
            //TODO(MS):refactor
            ContainingWorld.AddEntity(d, mat4.Translate(d.CurrentPosition), /*Network.GCManager.CurrentUserID*/0);
        }

        /// <summary>
        /// Removes a drone from drone constraints.
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
        public vec3 AlignPlacementPosition(vec3 pos, vec3 worldNormal, float height)
        {
            vec3 offset = vec3.Zero;
            var dirX = vec3.Zero;
            var dirY = vec3.Zero;
            var dirZ = vec3.Zero;
            AlignmentSystem(worldNormal, out dirX, out dirY, out dirZ);
            switch (CurrentDiggingPivot)
            {
                case DiggingController.DigPivot.Top:
                    offset = -dirY * height;
                    break;
                case DiggingController.DigPivot.Center:
                    offset = vec3.Zero;
                    break;
                case DiggingController.DigPivot.Bottom:
                    offset = dirY * height;
                    break;
            }

            if (CurrentDiggingAlignment == DiggingController.DigAlignment.GridAligned)
            {
                var rotMat = new mat3(dirX, dirY, dirZ);
                var rotPos = rotMat.Transpose * pos;

                var snapPos = new vec3(
                    (int)Math.Round(rotPos.x * (2.0f / DiggingGridSize)),
                    (int)Math.Round(rotPos.y * (2.0f / DiggingGridSize)),
                    (int)Math.Round(rotPos.z * (2.0f / DiggingGridSize))
                ) * (DiggingGridSize / 2.0f) + offset;

                return rotMat * snapPos;
            }
            else
                return pos + offset;
        }

        /// <summary>
        /// Calculates an alignment system for digging/constructing
        /// </summary>
        public void AlignmentSystem(vec3 worldNormal, out vec3 dirX, out vec3 dirY, out vec3 dirZ)
        {
            switch (CurrentDiggingAlignment)
            {
                case DiggingController.DigAlignment.GridAligned: // fall-through intended
                case DiggingController.DigAlignment.Axis:
                    float alpha = (float)(DiggingAlignmentAxisRotation * 5 * Math.PI / 180);
                    dirX = new vec3((float)Math.Cos(alpha), 0, (float)-Math.Sin(alpha));
                    dirY = vec3.UnitY;
                    dirZ = new vec3((float)Math.Sin(alpha), 0, (float)Math.Cos(alpha));
                    break;
                case DiggingController.DigAlignment.View:
                    dirX = -camera.RightDirection.Normalized;
                    dirZ = camera.UpDirection.Normalized;
                    dirY = -camera.ForwardDirection.Normalized;
                    break;
                case DiggingController.DigAlignment.Terrain:
                    dirY = worldNormal.Normalized;
                    dirX = vec3.cross(camera.ForwardDirection, dirY).Normalized;
                    dirZ = vec3.cross(dirX, dirY).Normalized;
                    break;
                default:
                    throw new InvalidOperationException("Unknown alignment");
            }
        }

        /// <summary>
        /// Places the current digging shape shape of a given material
        /// </summary>
        public void PlaceMaterial(TerrainResource material, vec3 worldNormal, vec3 position, float radius)
        {
            position = AlignPlacementPosition(position, worldNormal, radius);

            var filterMats = CurrentDiggingAddMode != DiggingController.AddMode.AirOnly ? null : new int[] { 0 };
            bool allowAirChange = CurrentDiggingAddMode != DiggingController.AddMode.NonAirOnly;

            switch (CurrentDiggingShape)
            {
                case DiggingController.DigShape.Sphere:
                    digging.DigSphere(worldNormal, position, radius, filterMats, material.Index, DiggingController.DigMode.Add, allowAirChange);
                    break;
                case DiggingController.DigShape.Box:
                    digging.DigBox(worldNormal, position, radius, filterMats, material.Index, DiggingController.DigMode.Add, allowAirChange);
                    break;
                case DiggingController.DigShape.Cylinder:
                    digging.DigCylinder(worldNormal, position, radius, filterMats, material.Index, DiggingController.DigMode.Add, allowAirChange);
                    break;
                case DiggingController.DigShape.Cone:
                    digging.DigCone(worldNormal, position, radius, filterMats, material.Index, DiggingController.DigMode.Add, allowAirChange);
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
            position = AlignPlacementPosition(position, worldNormal, radius);

            switch (CurrentDiggingShape)
            {
                case DiggingController.DigShape.Sphere:
                    digging.DigSphere(worldNormal, position, radius, filterMaterials);
                    break;
                case DiggingController.DigShape.Box:
                    digging.DigBox(worldNormal, position, radius, filterMaterials);
                    break;
                case DiggingController.DigShape.Cylinder:
                    digging.DigCylinder(worldNormal, position, radius, filterMaterials);
                    break;
                case DiggingController.DigShape.Cone:
                    digging.DigCone(worldNormal, position, radius, filterMaterials);
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

        public void RefreshSelection()
        {
            // Reselect to refresh shape
            if (Inventory.Selection != null)
            {
                Inventory.Selection.OnDeselect(this);
                Inventory.Selection.OnSelect(this);
            }
        }
    }
}

