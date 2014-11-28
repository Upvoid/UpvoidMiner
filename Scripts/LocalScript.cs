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
using Engine;
using Engine.Audio;
using Engine.Input;
using Engine.Universe;
using Engine.Rendering;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Download;
using Engine.Webserver;
using Engine.Windows;
using Engine.Gui;
using Engine.Network;
using Common.Cameras;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Diagnostics;
using EfficientUI;
using UpvoidMiner.Items;
using UpvoidMiner.UI;

namespace UpvoidMiner
{
    /// <summary>
    /// Main class for the local scripting domain.
    /// </summary>
    public class LocalScript
    {
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
        /// A global entity that handles the renderjobs for the digging/construction preview shapes
        /// </summary>
        public static AnonymousEntity ShapeIndicatorEntity;
        /// <summary>
        /// The main camera that renders to the screen.
        /// </summary>
        public static GenericCamera camera;
        /// <summary>
        /// A camera controller for free camera movement. Used when noclipEnabled is true.
        /// </summary>
        static FreeCameraControl cameraControl;
        public static Player player = null;
        public static MusicQueue musicQueue = null;
        private static SoundResource birdRes;
        private static Sound birdSound;
        // Note that these are the initial volumes for the specific sounds.
        // The "global" music volume is defined via settings
        const float musicVolume = 1.0f;
        const float birdVolume = 0.5f;
        public static StatUI stats = new StatUI();
        public static MemoryFailsafe memFailsafe = new MemoryFailsafe();

        /// <summary>
        /// Set this to true to enable free camera movement.
        /// </summary>
        public static bool NoclipEnabled { get; private set; }

        /// <summary>
        /// This is called by the engine at mod startup and initializes the local part of the UpvoidMiner mod.
        /// </summary>
        public static void Startup(Module module)
        {
            // Set window title
            var window = Engine.Windows.Windows.GetWindow(0);
            window.SetTitle("Upvoid Miner");
            window.SetMinimumWidth(1100);
            window.SetMinimumHeight(700);

            // Get and save the resource domain of the mod, needed for loading resources.
            UpvoidMiner.Mod = module;
            UpvoidMiner.ModDomain = UpvoidMiner.Mod.ResourceDomain;

            // Get the world (created by the host script).
            world = Universe.GetWorldByName("UpvoidMinerWorld");

            // No loading screen for clients (since the server generates the world)
            if (Scripting.IsHost)
            {
                // Register a callback for the terrain generation so the GUI can be notified when the world is ready.
                world.Terrain.AddVolumeUpdateCallback(VolumeCallback, false, 0, 4);

                // Show a splash screen in the GUI client.
                if (Scripting.IsDeploy)
                    Gui.DefaultUI.LoadURL(UpvoidMiner.ModDomain, "SplashScreen.html");
                else
                    Gui.DefaultUI.LoadURL(UpvoidMiner.ModDomain, "MainMenu.html?Debug");

                // Register a socket for sending progress updates to the loading screen
                generationProgressSocket = new WebSocketHandler();
                Webserver.DefaultWebserver.RegisterWebSocketHandler(UpvoidMiner.ModDomain, "GenerationProgressSocket", generationProgressSocket);

                Webserver.DefaultWebserver.RegisterDynamicContent(UpvoidMiner.ModDomain, "ActivatePlayer", (request, response) => ActivatePlayer(request.GetQuery("GodMode") == "true"));
                Webserver.DefaultWebserver.RegisterDynamicContent(UpvoidMiner.ModDomain, "IsPlayerActivated", (request, response) => response.AppendBody((player != null).ToString()));
                Webserver.DefaultWebserver.RegisterDynamicContent(UpvoidMiner.ModDomain, "GenerationProgressQuery", webGenerationProgress);
                Webserver.DefaultWebserver.RegisterDynamicContent(UpvoidMiner.ModDomain, "OpenSiteInBrowser", (request, response) => Scripting.OpenUrlExternal(request.GetQuery("url")));
            }

            // Create a simple camera that allows free movement.
            camera = new GenericCamera();
            camera.Position = new vec3(150, 40, 150);
            camera.FarClippingPlane = 1750.0;

            // Client-only: register terrain materials
            if (!Scripting.IsHost)
                TerrainResource.RegisterResources(world.Terrain);

            // Place the camera in the world.
            world.AttachCamera(camera);
            Rendering.SetupDefaultPipeline(camera);

            // Create an active region around the player spawn
            // Active regions help the engine to decide which parts of a world are important (to generate, render, etc.)
            // In near future it will be updated when the player moves out of it
            //world.AddActiveRegion(new ivec3(), 100f, 400f, 40f, 40f);

            Settings.settings.ResetSettings(); // aka load from settings
            UIProxyManager.AddProxy(Settings.settings);
            UIProxyManager.AddProxy(stats);
            UIProxyManager.AddProxy(memFailsafe);

            Webserver.DefaultWebserver.RegisterDynamicContent(UpvoidMiner.ModDomain, "QuitGame",
                (WebRequest request, WebResponse response) => Gui.DefaultUI.LoadURL(UpvoidMiner.ModDomain, "ShutdownScreen.html"));
            Webserver.DefaultWebserver.RegisterDynamicContent(UpvoidMiner.ModDomain, "QuitGameReally",
                (WebRequest request, WebResponse response) => Scripting.ShutdownEngine());

            // Register for input press events.
            Input.OnPressInput += HandlePressInput;

            // Register sockets for resource downloading progress bar
            resourceDownloadProgressSocket = new WebSocketHandler();
            Webserver.DefaultWebserver.RegisterWebSocketHandler(UpvoidMiner.ModDomain, "ResourceDownloadProgress", resourceDownloadProgressSocket);

            if (!Scripting.IsHost)
                ActivatePlayer();

            // Play some ambient sounds
            birdRes = Resources.UseSound("Mods/Upvoid/Resources.SFX/1.0.0::Ambient/Birds/BirdAmbient01", UpvoidMiner.ModDomain);
            // Start with zero volume, we adapt that later
            birdSound = new Sound(birdRes, vec3.Zero, true, 0.0f, 1, (int)AudioType.SFX, true);
            birdSound.ReferenceDistance = 2.0f;
            birdSound.Play();

            // Create a new (repeating) music queue with pauses of 5-10s between the songs
            musicQueue = new MusicQueue(4, 8, true);

            // Add songs to the music queue
            musicQueue.Add(new Sound(
                Resources.UseSound("Mods/Upvoid/Resources.Music/1.0.0::Miscellaneous/_ghost_-_Reverie_(small_theme)", UpvoidMiner.ModDomain),
                vec3.Zero, false, musicVolume, 1, (int)AudioType.Music, false));

            musicQueue.Add(new Sound(
                Resources.UseSound("Mods/Upvoid/Resources.Music/1.0.0::Miscellaneous/AlexBeroza_-_Improvisation_On_Friday", UpvoidMiner.ModDomain),
                vec3.Zero, false, musicVolume, 1, (int)AudioType.Music, false));

            musicQueue.Add(new Sound(
                Resources.UseSound("Mods/Upvoid/Resources.Music/1.0.0::Miscellaneous/AlexBeroza_-_Emerge", UpvoidMiner.ModDomain),
                vec3.Zero, false, musicVolume, 1, (int)AudioType.Music, false));

            musicQueue.Add(new Sound(
                Resources.UseSound("Mods/Upvoid/Resources.Music/1.0.0::Miscellaneous/Frank_Nora_-_New_Midnight_Cassette_27_Ambient01", UpvoidMiner.ModDomain),
                vec3.Zero, false, musicVolume, 1, (int)AudioType.Music, false));

            musicQueue.Add(new Sound(
                Resources.UseSound("Mods/Upvoid/Resources.Music/1.0.0::Miscellaneous/Pitx_-_Chords_For_David", UpvoidMiner.ModDomain),
                vec3.Zero, false, musicVolume, 1, (int)AudioType.Music, false));

            musicQueue.Add(new Sound(
                Resources.UseSound("Mods/Upvoid/Resources.Music/1.0.0::Miscellaneous/Pitx_-_Writing_the_future", UpvoidMiner.ModDomain),
                vec3.Zero, false, musicVolume, 1, (int)AudioType.Music, false));

            musicQueue.Add(new Sound(
                Resources.UseSound("Mods/Upvoid/Resources.Music/1.0.0::Miscellaneous/zeos_-_Photo_theme_Window_like", UpvoidMiner.ModDomain),
                vec3.Zero, false, musicVolume, 1, (int)AudioType.Music, false));

            musicQueue.Add(new Sound(
                Resources.UseSound("Mods/Upvoid/Resources.Music/1.0.0::Miscellaneous/zep_hurme_-_Ethereal", UpvoidMiner.ModDomain),
                vec3.Zero, false, musicVolume, 1, (int)AudioType.Music, false));

            // Do not play music. this is done by the music queue 8-)
            //music.Play();
        }

        static bool generationDone = false;
        static int generatedChunks = 0;
        static WebSocketHandler generationProgressSocket;

        static void VolumeCallback(int x, int y, int z, int lod, int size)
        {
            if (generationDone)
            {
                generationProgressSocket.SendMessage(1.0f.ToString());
                return;
            }

            if (lod <= 4)
            {
                generatedChunks++;
            }

            if (generatedChunks >= 20)
            {
                generationDone = true;
            }

            generationProgressSocket.SendMessage(((float)generatedChunks / 20f).ToString());
        }

        static void webGenerationProgress(WebRequest request, WebResponse response)
        {
            if (generationDone)
            {
                response.AppendBody(1.0f.ToString());
            }
            else
            {
                response.AppendBody(((float)generatedChunks / 28f).ToString());
            }
        }

        static void ActivatePlayer(bool godMode = false)
        {
            // Activate player only once.
            if (player != null)
            {
                player.Gui.IsInventoryOpen = false;
                player.Gui.IsMenuOpen = false;
                return;
            }

            // Initialize the savegame paths
            UpvoidMiner.SavePathEntities = UpvoidMiner.SavePathBase + "/Entities";
            UpvoidMiner.SavePathWorldItems = UpvoidMiner.SavePathBase + "/WorldItems";
            UpvoidMiner.SavePathInventory = UpvoidMiner.SavePathBase + "/Inventory" + (godMode ? "GodMode" : "AdventureMode");

            // Activate camera movement
            cameraControl = new FreeCameraControl(-10f, camera);

            // Configure the camera to receive user input.
            Input.RootGroup.AddListener(cameraControl);

            // Create particle entity.
            ParticleEntity = new AnonymousEntity(mat4.Identity);
            world.AddEntity(ParticleEntity, Network.GCManager.CurrentUserID);

            // Create shape indicator entity.
            ShapeIndicatorEntity = new AnonymousEntity(mat4.Identity);
            world.AddEntity(ShapeIndicatorEntity, Network.GCManager.CurrentUserID);

            // Create the Player EntityScript and add it to the world.
            player = new Player(camera, godMode);
            world.AddEntity(player, mat4.Translate(Player.SpawnPosition), Network.GCManager.CurrentUserID);

            // Register the update callback that updates the camera position.
            Scripting.RegisterUpdateFunction(Update, UpvoidMiner.Mod);

            // Register save callback
            Savegame.OnSave += s =>
            {
                player.Save();
                UpvoidMinerWorldGenerator.SaveEntities();
            };

            Scripting.OnEngineShutdown += (sender, args) =>
            {
                Gui.DefaultUI.LoadURL(UpvoidMiner.ModDomain, "ShutdownScreen.html");
                player.Save();
                UpvoidMinerWorldGenerator.SaveEntities();
            };

            // Start tutorial
            Tutorials.Init();
        }

        /// Bezier Camera Path Feature
        private static List<vec3> PathPositions = new List<vec3>();
        private static List<vec3> PathDirections = new List<vec3>();
        private static List<vec3> PathTmps = new List<vec3>();
        private static float PathCurrPos = 0;
        private static bool PathPlaying = false;

        private static vec3 bezierOf(List<vec3> vecs, List<vec3> tmp, float t)
        {
            for (int i = 0; i < vecs.Count; ++i)
                tmp[i] = vecs[i];

            for (int i = vecs.Count - 1; i >= 1; --i)
            {
                for (int j = 0; j < i; ++j)
                {
                    tmp[j] = tmp[j] * (1 - t) + tmp[j + 1] * t;
                }
            }
            return tmp[0];
        }

        /// <summary>
        /// Performs some basic input handling.
        /// </summary>
        private static void HandlePressInput(object sender, InputPressArgs e)
        {
            if (!Rendering.MainViewport.HasFocus)
                return;
            // For now, gameplay and debug actions are bound to static keys.

            // N toggles noclip.
            if (e.PressType == InputPressArgs.KeyPressType.Up && e.Key == InputKey.N)
            {
                NoclipEnabled = !NoclipEnabled;

                if (!NoclipEnabled) // just switched to first-person mode again
                {
                    if (player != null)
                        player.Character.Body.SetTransformation(mat4.Translate(camera.Position));
                }
            }

            // Cam Paths
            if (NoclipEnabled && e.PressType == InputPressArgs.KeyPressType.Down)
            {
                switch (e.Key)
                {
                    case InputKey.F9:
                        PathPositions.Add(camera.Position);
                        PathDirections.Add(camera.ForwardDirection);
                        PathTmps.Add(vec3.Zero);
                        Console.WriteLine("Point recorded");
                        break;
                    case InputKey.F11:
                        PathPositions.Clear();
                        PathTmps.Clear();
                        PathDirections.Clear();
                        Console.WriteLine("Path deleted");
                        break;
                    case InputKey.F10:
                        PathPlaying = !PathPlaying;
                        break;
                }
            }
        }

        /// <summary>
        /// Last time of save
        /// </summary>
        private static DateTime lastSave = DateTime.Now;

        /// <summary>
        /// Updates the camera position.
        /// </summary>
        public static void Update(float _elapsedSeconds)
        {
            if (NoclipEnabled && cameraControl != null)
            {
                // Bezier Path
                if (PathPlaying && PathPositions.Count >= 2)
                {
                    PathCurrPos += _elapsedSeconds;
                    while (PathCurrPos > PathPositions.Count)
                        PathCurrPos -= PathPositions.Count;
                    vec3 pos = bezierOf(PathPositions, PathTmps, PathCurrPos / (float)PathPositions.Count);
                    vec3 dir = bezierOf(PathDirections, PathTmps, PathCurrPos / (float)PathPositions.Count);
                    camera.Position = pos;
                    camera.SetTarget(pos + dir, vec3.UnitY);
                }
                cameraControl.Update(_elapsedSeconds);
            }

            if (player != null)
            {
                player.Update(_elapsedSeconds);
            }

            if (musicQueue != null)
            {
                musicQueue.update(_elapsedSeconds);
            }

            UpdateResourceDownloadProgress();

            if ((DateTime.Now - lastSave).TotalSeconds > 10)
            {
                lastSave = DateTime.Now;

                if (player != null)
                    player.Save();
                UpvoidMinerWorldGenerator.SaveEntities();
            }

            // update items
            ItemManager.Update();

            // Update all trees and keep position of closest tree, if any
            vec3 closestTree = UpvoidMinerWorldGenerator.UpdateTrees(camera.Position);

            // Handle bird sound volume (depending on distance to closest tree)
            if (birdSound != null)
            {
                float distToTrees = Math.Max(0.01f, vec3.distance(closestTree, camera.Position));

                if (distToTrees < 50.0f)
                {
                    // Set bird sound position to position of the closest tree
                    birdSound.Position = closestTree + new vec3(0, 2, 0);

                    // Attenuation (by distance) will be handled by Audio-Engine automatically
                    birdSound.Volume = birdVolume;
                }
                else
                {
                    // No close tree / birds at all...
                    birdSound.Volume = 0.0f;
                }
            }
        }
        // This socket notifies the client GUI about progress in the downloading of resources.
        static WebSocketHandler resourceDownloadProgressSocket;
        static long resourceDownloadTotalBytes = 0;
        static long resourceDownloadReceivedBytes = 0;

        static void UpdateResourceDownloadProgress()
        {
            if (Download.BytesReceived != resourceDownloadTotalBytes || Download.BytesReceived != resourceDownloadReceivedBytes)
            {
                double progress = (double)Download.BytesReceived / (double)Download.BytesTotal;
                if (Download.BytesTotal == 0)
                    progress = 1.0;
                else if (progress > 1)
                    progress = 1;

                resourceDownloadProgressSocket.SendMessage(progress.ToString());

                resourceDownloadTotalBytes = Download.BytesTotal;
                resourceDownloadReceivedBytes = Download.BytesReceived;
            }
        }

        /// <summary>
        /// MonoDevelop's debugger requires an executable program, so here is a dummy Main method.
        /// </summary>
        private static void Main()
        {
            throw new Exception("I'm a mod, don't execute me like that!");
        }
    }
}
