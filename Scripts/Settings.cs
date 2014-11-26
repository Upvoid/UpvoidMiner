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
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using Engine.Audio;
using Engine.Input;
using Engine.Universe;
using Engine.Rendering;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Webserver;
using Engine.Network;
using Newtonsoft.Json;
using EfficientUI;

namespace UpvoidMiner
{
    enum AudioType
    {
        Master,
        SFX,
        Music,
        Speech
    }

    public class Settings : UIProxy
    {

        public static Settings settings = new Settings();

        public struct VideoMode
        {
            public int Width;
            public int Height;
            //int Screen;
            public VideoMode(int width, int height)
            {
                Width = width;
                Height = height;
            }
        }

        private List<VideoMode> supportedVideoModes;

        private static VideoMode StringToVideoMode(string vidModString)
        {
            String[] curMode = vidModString.Split('x');

            Debug.Assert(curMode.Count() == 2);

            if (curMode.Count() == 2)
            {
                return new VideoMode(int.Parse(curMode[0]), int.Parse(curMode[1]));
            }

            // Error
            return new VideoMode(-2, -2);
        }
        // Local variables for the current settings values
        private VideoMode settingResolution = StringToVideoMode(Scripting.GetUserSettingString("WindowManager/Resolution", "-1x-1"));
        private bool settingFullscreen = Scripting.GetUserSettingString("WindowManager/Fullscreen", "-1") != "-1";
        private int settingMasterVolume = (int)(Audio.GetVolumeForSpecificAudioType((int)AudioType.Master) * 100f);
        private int settingSfxVolume = (int)(Audio.GetVolumeForSpecificAudioType((int)AudioType.SFX) * 100f);
        private int settingMusicVolume = (int)(Audio.GetVolumeForSpecificAudioType((int)AudioType.Music) * 100f);
        private bool settingMuteMusic = Scripting.GetUserSetting("Audio/Mute Music", false);
        private int settingFieldOfView = (int)Scripting.GetUserSettingNumber("Graphics/Field of View", 75.0);
        private bool settingShadows = Scripting.GetUserSetting("Graphics/Enable Shadows", true);
        private bool settingLensflares = Scripting.GetUserSetting("Graphics/Enable Lensflares", false);
        private bool settingVolumetricScattering = Scripting.GetUserSetting("Graphics/Enable Volumetric Scattering", true);
        private bool settingTonemapping = Scripting.GetUserSetting("Graphics/Enable Tonemapping", true);
        private bool settingFog = Scripting.GetUserSetting("Graphics/Enable Fog", true);
        private bool settingFXAA = Scripting.GetUserSetting("Graphics/Enable FXAA", true);
        private bool settingGrass = Scripting.GetUserSetting("Graphics/Enable Grass", true);
        private bool settingDigParticles = Scripting.GetUserSetting("Graphics/Enable Dig Particles", true);
        private double settingMouseSensitivity = Scripting.GetUserSettingNumber("Input/Mouse Sensitivity", 0.5);

        private bool pipelineChanges = false;

        [UICheckBox]
        public bool DigParticles
        {
            get { return settingDigParticles; }
            set { settingDigParticles = value; }
        }

        private Settings()
            : base("Settings")
        {
            // Read the supported video modes
            var modes = Rendering.GetSupportedVideoModes().Distinct().ToList();

            settingResolution = StringToVideoMode(Scripting.GetUserSettingString("WindowManager/Resolution", "-1x-1"));

            // Add native resolution
            supportedVideoModes = new List<VideoMode> { new VideoMode(-1, -1) };
            foreach (string vidMode in modes)
                supportedVideoModes.Add(StringToVideoMode(vidMode));
        }

        [UIObject]
        public List<VideoMode> VideoModesObject
        {
            get { return supportedVideoModes; }
        }

        [UICallback]
        public void VideoModeCallback(int index)
        {
            Debug.Assert(index < supportedVideoModes.Count());
            settingResolution = supportedVideoModes[index];
        }

        [UISlider(0, 100)]
        public int MasterVolume
        {
            get { return settingMasterVolume; }
            set
            {
                settingMasterVolume = value;
                Audio.SetVolumeForSpecificAudioType(settingMasterVolume / 100f, (int)AudioType.Master);
            }
        }

        [UISlider(0, 100)]
        public int SfxVolume
        {
            get { return settingSfxVolume; }
            set
            {
                settingSfxVolume = value;
                Audio.SetVolumeForSpecificAudioType(settingSfxVolume / 100f, (int)AudioType.SFX);
            }
        }

        [UISlider(0, 100)]
        public int MusicVolume
        {
            get { return settingMusicVolume; }
            set
            {
                settingMusicVolume = value;
                Audio.SetVolumeForSpecificAudioType(settingMuteMusic ? 0.0f : settingMusicVolume / 100f, (int)AudioType.Music);
            }
        }

        [UICheckBox]
        public bool MuteMusic
        {
            get { return settingMuteMusic; }
            set
            {
                settingMuteMusic = value;
                Audio.SetVolumeForSpecificAudioType(settingMuteMusic ? 0.0f : settingMusicVolume / 100f, (int)AudioType.Music);
            }
        }

        [UISlider(45, 135)]
        public int FieldOfView
        {
            get { return settingFieldOfView; }
            set
            {
                settingFieldOfView = value;
                LocalScript.camera.HorizontalFieldOfView = value;
            }
        }

        [UICheckBox]
        public bool Fullscreen
        {
            get { return settingFullscreen; }
            set { settingFullscreen = value; }
        }

        [UICheckBox]
        public bool Shadows
        {
            get { return settingShadows; }
            set
            {
                if (settingShadows != value) pipelineChanges = true;
                settingShadows = value;
            }
        }

        [UICheckBox]
        public bool Lensflares
        {
            get { return settingLensflares; }
            set
            {
                if (settingLensflares != value) pipelineChanges = true;
                settingLensflares = value;
            }
        }

        [UICheckBox]
        public bool VolumetricScattering
        {
            get { return settingVolumetricScattering; }
            set
            {
                if (settingVolumetricScattering != value) pipelineChanges = true;
                settingVolumetricScattering = value;
            }
        }

        [UICheckBox]
        public bool Tonemapping
        {
            get { return settingTonemapping; }
            set
            {
                if (settingTonemapping != value) pipelineChanges = true;
                settingTonemapping = value;
            }
        }

        [UICheckBox]
        public bool Fog
        {
            get { return settingFog; }
            set
            {
                if (settingFog != value) pipelineChanges = true;
                settingFog = value;
            }
        }

        [UICheckBox]
        public bool FXAA
        {
            get { return settingFXAA; }
            set
            {
                if (settingFXAA != value) pipelineChanges = true;
                settingFXAA = value;
            }
        }

        [UICheckBox]
        public bool Grass
        {
            get { return settingGrass; }
            set
            {
                if (value == settingGrass)
                    return;

                if (LocalScript.world != null)
                {
                    // update terrain material activity
                    foreach (var resource in TerrainResource.ListResources())
                        if (resource is VegetatedTerrainResource)
                        {
                            var res = resource as VegetatedTerrainResource;
                            res.Material.SetPipelineActive(res.GrassPipelineIndex, value);
                        }
                    // grass change requires terrain rebuilt
                    LocalScript.world.Terrain.RebuildTerrainGeometry();
                }
                settingGrass = value;
            }
        }

        [UICheckBox]
        public bool ShowStats { get; set; }

        [UISlider(10, 50)]
        public int LodFalloff
        {
            get { return (int)LocalScript.world.LodSettings.LodFalloff; }
            set { LocalScript.world.LodSettings.LodFalloff = value; }
        }

        [UISlider(0, 100)]
        public int MinLodDistance
        {
            get { return (int)LocalScript.world.LodSettings.MinLodDistance; }
            set { LocalScript.world.LodSettings.MinLodDistance = value; }
        }

        [UISlider(0, 100)]
        public int MouseSensitivity
        {
            get { return (int)(settingMouseSensitivity * 100); }
            set { settingMouseSensitivity = value / 100.0; }
        }

        public float MouseSensitivityF { get { return MouseSensitivity / 100f; } }

        [UISlider(10, 500)]
        public int MaxTrees { get; set; }

        [UISlider(10, 500)]
        public int MaxTreeDistance { get; set; }

        [UIButton]
        public void ApplySettings()
        {
            // Write all settings to settings file

            // Audio settings
            Scripting.SetUserSettingNumber("Audio/Master Volume", settingMasterVolume);
            Scripting.SetUserSettingNumber("Audio/SFX Volume", settingSfxVolume);
            Scripting.SetUserSettingNumber("Audio/Music Volume", settingMusicVolume);
            Scripting.SetUserSetting("Audio/Mute Music", settingMuteMusic);

            // Graphics settings
            Scripting.SetUserSettingNumber("Graphics/Field of View", settingFieldOfView);

            Scripting.SetUserSetting("Graphics/Enable Shadows", settingShadows);
            Scripting.SetUserSetting("Graphics/Enable Lensflares", settingLensflares);
            Scripting.SetUserSetting("Graphics/Enable Volumetric Scattering", settingVolumetricScattering);
            Scripting.SetUserSetting("Graphics/Enable Tonemapping", settingTonemapping);
            Scripting.SetUserSetting("Graphics/Enable Fog", settingFog);
            Scripting.SetUserSetting("Graphics/Enable FXAA", settingFXAA);

            Scripting.SetUserSetting("Graphics/Enable Grass", settingGrass);
            Scripting.SetUserSetting("Graphics/Enable Dig Particles", settingDigParticles);

            Scripting.SetUserSettingNumber("Input/Mouse Sensitivity", settingMouseSensitivity);

            Scripting.SetUserSettingNumber("Graphics/Lod Falloff", LodFalloff);
            Scripting.SetUserSettingNumber("Graphics/Min Lod Distance", MinLodDistance);
            Scripting.SetUserSettingNumber("Graphics/Max Trees", MaxTrees);
            Scripting.SetUserSettingNumber("Graphics/Max Tree Distance", MaxTreeDistance);

            if (settingFullscreen)
                Scripting.SetUserSettingString("WindowManager/Fullscreen", "0");
            else
                Scripting.SetUserSettingString("WindowManager/Fullscreen", "-1");

            string vidModeString = settingResolution.Width + "x" + settingResolution.Height;
            Scripting.SetUserSettingString("WindowManager/Resolution", vidModeString);

            // rebuild pipeline on changes
            // TODO: fixme
            //if (pipelineChanges)
            //    Rendering.SetupDefaultPipeline(LocalScript.camera);
            pipelineChanges = false;
        }

        [UIButton]
        public void ResetSettings()
        {
            WorldLod lod = LocalScript.world.LodSettings;

            // Reset local setting values to those from user settings
            settingMasterVolume = (int)Scripting.GetUserSettingNumber("Audio/Master Volume", 100);
            settingSfxVolume = (int)Scripting.GetUserSettingNumber("Audio/SFX Volume", 50);
            settingMusicVolume = (int)Scripting.GetUserSettingNumber("Audio/Music Volume", 50);
            settingMuteMusic = Scripting.GetUserSetting("Audio/Mute Music", false);

            settingFieldOfView = (int)Scripting.GetUserSettingNumber("Graphics/Field of View", 75);

            settingShadows = Scripting.GetUserSetting("Graphics/Enable Shadows", settingShadows);
            settingLensflares = Scripting.GetUserSetting("Graphics/Enable Lensflares", false);
            settingVolumetricScattering = Scripting.GetUserSetting("Graphics/Enable Volumetric Scattering", true);
            settingTonemapping = Scripting.GetUserSetting("Graphics/Enable Tonemapping", true);
            settingFog = Scripting.GetUserSetting("Graphics/Enable Fog", true);
            settingFXAA = Scripting.GetUserSetting("Graphics/Enable FXAA", true);
            settingGrass = Scripting.GetUserSetting("Graphics/Enable Grass", true);
            settingDigParticles = Scripting.GetUserSetting("Graphics/Enable Dig Particles", true);

            settingMouseSensitivity = Scripting.GetUserSettingNumber("Input/Mouse Sensitivity", 0.5);

            // property in order to trigger rebuilt
            Grass = Scripting.GetUserSetting("Graphics/Enable Grass", true);

            settingFullscreen = Scripting.GetUserSettingString("WindowManager/Fullscreen", "-1") != "-1";
            settingResolution = StringToVideoMode(Scripting.GetUserSettingString("WindowManager/Resolution", "-1x-1"));

            MinLodDistance = (int)Scripting.GetUserSettingNumber("Graphics/Min Lod Distance", 20);
            LodFalloff = (int)Scripting.GetUserSettingNumber("Graphics/Lod Falloff", 30);
            MaxTrees = (int)Scripting.GetUserSettingNumber("Graphics/Max Trees", 200);
            MaxTreeDistance = (int)Scripting.GetUserSettingNumber("Graphics/Max Tree Distance", 300);

            // Re-apply the former settings
            Audio.SetVolumeForSpecificAudioType(settingMasterVolume / 100f, (int)AudioType.Master);
            Audio.SetVolumeForSpecificAudioType(settingSfxVolume / 100f, (int)AudioType.SFX);
            Audio.SetVolumeForSpecificAudioType(settingMuteMusic ? 0.0f : settingMusicVolume / 100f, (int)AudioType.Music);
            LocalScript.camera.HorizontalFieldOfView = settingFieldOfView;

            pipelineChanges = false;
        }
    }
}
