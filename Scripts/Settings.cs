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
using UpvoidMiner.UI;

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
        private readonly static List<Setting> allSettings = new List<Setting>();
        public static Settings settings;

        private List<VideoMode> supportedVideoModes;

        // Transforms a string of the form "widthxHeight" to VideoMode(width, height);
        private static VideoMode StringToVideoMode(string vidModString)
        {
            string[] curMode = vidModString.Split('x');

            Debug.Assert(curMode.Count() == 2);

            if (curMode.Count() == 2)
            {
                return new VideoMode(int.Parse(curMode[0]), int.Parse(curMode[1]));
            }

            // Error
            return new VideoMode(-2, -2);
        }

        // A VideoMode wraps a width/height pair
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


        // Encapsulates one single setting
        public abstract class Setting
        {
            public string id;   // JSON key, e.g. "Graphics/Shadow Quality"
            public string desc;  // a longer description, e.g. for tooltips, optional
            public Setting(string identifier, string description)
            {
                id = identifier;
                desc = description;
            }

            // Reload the setting from user.settings file
            public abstract void reloadSettingFromFile();

            // Save the current setting to user.settings file
            public abstract void SaveSetting();
        }

        public class SettingDouble : Setting
        {
            public double value;
            public double defValue;
            public SettingDouble(string identifier, double defaultValue, string description = "") :
                base(identifier, description)
            {
                defValue = defaultValue;

                allSettings.Add(this);
            }

            public override void reloadSettingFromFile()
            {
                value = Scripting.GetUserSettingNumber(id, defValue);
            }

            public override void SaveSetting()
            {
                Scripting.SetUserSettingNumber(id, value);
            }
        }


        public class SettingBool : Setting
        {
            public bool value;
            public bool defValue;
            public SettingBool(String identifier, bool defaultValue, String description = "") :
                base(identifier, description)
            {
                defValue = defaultValue;

                allSettings.Add(this);
            }

            public override void reloadSettingFromFile()
            {
                value = Scripting.GetUserSetting(id, defValue);
            }

            public override void SaveSetting()
            {
                Scripting.SetUserSetting(id, value);
            }
        }

        // Window Manager
        SettingDouble settingResolutionWidth          = new SettingDouble("WindowManager/Resolution/Width", -1);
        SettingDouble settingResolutionHeight         = new SettingDouble("WindowManager/Resolution/Height", -1);
        SettingDouble settingFullscreen               = new SettingDouble("WindowManager/Fullscreen", -1);
        SettingDouble settingInternalResolutionWidth  = new SettingDouble("WindowManager/InternalResolution/Width", -1);
        SettingDouble settingInternalResolutionHeight = new SettingDouble("WindowManager/InternalResolution/Height", -1);

        // Audio
        SettingDouble settingMasterVolume = new SettingDouble("Audio/Master Volume", 100);
        SettingDouble settingSfxVolume    = new SettingDouble("Audio/SFX Volume", 50);
        SettingDouble settingMusicVolume  = new SettingDouble("Audio/Music Volume", 50);
        SettingBool   settingMuteMusic    = new SettingBool("Audio/Mute Music", false);
        
        // Graphics
        SettingDouble settingAnisotropicFiltering = new SettingDouble("Graphics/Anisotropic Filtering", 4);
        SettingDouble settingShadowQuality        = new SettingDouble("Graphics/Shadow Quality", 512);
        SettingBool   settingVolumetricScattering = new SettingBool("Graphics/Enable Volumetric Scattering", false);
        SettingBool   settingTonemapping          = new SettingBool("Graphics/Enable Tonemapping", true);
        SettingBool   settingFXAA                 = new SettingBool("Graphics/Enable FXAA", true);
        SettingBool   settingLensflares           = new SettingBool("Graphics/Enable Lensflares", false);
        SettingDouble settingFieldOfView          = new SettingDouble("Graphics/Field of View", 75.0);
        
        // LoD
        SettingBool   settingGrass           = new SettingBool("Graphics/Enable Grass", true);
        SettingBool   settingDigParticles    = new SettingBool("Graphics/Enable Dig Particles", true);
        SettingDouble settingMaxTreeDistance = new SettingDouble("Graphics/Max Tree Distance", 150);
        SettingDouble settingMinLodDistance  = new SettingDouble("Graphics/Min Lod Distance", 20);
        SettingDouble settingLodFalloff      = new SettingDouble("Graphics/Lod Falloff", 30);
        
        // Other
        SettingDouble settingMouseSensitivity = new SettingDouble("Input/Mouse Sensitivity", 0.5);
        SettingBool   settingHideTutorial     = new SettingBool("Misc/Hide Tutorial", false);
        SettingBool   settingShowStats        = new SettingBool("Debug/Show Stats", false);


        private bool pipelineChanges = false;

        [UITextBox]
        public string InternalSize { 
            get
            {
                return settingInternalResolutionWidth.value.ToString() + "x" + 
                       settingInternalResolutionHeight.value.ToString();
            }
            set
            {
                int w = -1, h = -1;
                if (value.Contains("x"))
                {
                    var split = value.Split('x');
                    if (split.Length == 2)
                    {
                        int tw, th;
                        if (int.TryParse(split[0], out tw) &&
                            int.TryParse(split[1], out th) &&
                            tw >= 2 &&
                            th >= 2)
                        {
                            w = tw;
                            h = th;
                        }
                    }
                }
            settingInternalResolutionWidth.value = w;
            settingInternalResolutionHeight.value = h;
            }
        }

        [UIButton]
        public void ApplyInternalSize()
        {
            int w = (int)settingInternalResolutionWidth.value;
            int h = (int)settingInternalResolutionHeight.value;
            if (w == -1)
            {
                w = Engine.Windows.Windows.GetWindow(0).GetWidth();
                h = Engine.Windows.Windows.GetWindow(0).GetHeight();
                if (Rendering.ActiveMainPipeline != null)
                    Rendering.ActiveMainPipeline.SetInternalRenderSize(w, h);
                w = -1;
                h = -1;
            }

            if (Rendering.ActiveMainPipeline != null)
                Rendering.ActiveMainPipeline.SetInternalRenderSize(w, h);
        }

        [UICheckBox]
        public bool DigParticles
        {
            get { return settingDigParticles.value; }
            set { settingDigParticles.value = value; }
        }

        private Settings()
            : base("Settings")
        {
            // Read the supported video modes
            var modes = Rendering.GetSupportedVideoModes().Distinct().ToList();

            // Add native resolution
            supportedVideoModes = new List<VideoMode> { new VideoMode(-1, -1) };
            foreach (string vidMode in modes)
            {
                var mode = StringToVideoMode(vidMode);
                if (mode.Width > 0 && mode.Height > 0 &&
                    (mode.Width < 1100 || mode.Height < 700))
                    continue;
                supportedVideoModes.Add(mode);
            }
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

            settingResolutionWidth.value = supportedVideoModes[index].Width;
            settingResolutionHeight.value = supportedVideoModes[index].Height;
        }

        [UISlider(0, 100)]
        public int MasterVolume
        {
            get { return (int)settingMasterVolume.value; }
            set
            {
                settingMasterVolume.value = value;
                Audio.SetVolumeForSpecificAudioType((float)settingMasterVolume.value / 100f, (int)AudioType.Master);
            }
        }

        [UISlider(0, 100)]
        public int SfxVolume
        {
            get { return (int)settingSfxVolume.value; }
            set
            {
                settingSfxVolume.value = value;
                Audio.SetVolumeForSpecificAudioType((float)settingSfxVolume.value / 100f, (int)AudioType.SFX);
            }
        }

        [UISlider(0, 100)]
        public int MusicVolume
        {
            get { return (int)settingMusicVolume.value; }
            set
            {
                settingMusicVolume.value = value;
                Audio.SetVolumeForSpecificAudioType(settingMuteMusic.value ? 0.0f : (float)settingMusicVolume.value / 100f, (int)AudioType.Music);
            }
        }

        [UICheckBox]
        public bool MuteMusic
        {
            get { return settingMuteMusic.value; }
            set
            {
                settingMuteMusic.value = value;
                Audio.SetVolumeForSpecificAudioType(value ? 0.0f : (float)settingMusicVolume.value / 100f, (int)AudioType.Music);
            }
        }

        [UISlider(45, 135)]
        public int FieldOfView
        {
            get { return (int)settingFieldOfView.value; }
            set
            {
                settingFieldOfView.value = value;
                LocalScript.camera.HorizontalFieldOfView = value;
            }
        }

        [UICheckBox]
        public bool Fullscreen
        {
            get { return (settingFullscreen.value > -1); }
            set { settingFullscreen.value = value ? 0 : -1; }
        }

        [UICheckBox]
        public bool HideTutorial
        {
            get { return settingHideTutorial.value; }
            set { settingHideTutorial.value = value; }
        }

        [UISlider(0,4)]
        public int ShadowQuality
        {
            get { return (int)settingShadowQuality.value; }
            set
            {
                if (settingShadowQuality.value != value)
                {
                    settingShadowQuality.value = value;
                    pipelineChanges = true;
                }
            }
        }

        [UICheckBox]
        public bool Lensflares
        {
            get { return settingLensflares.value; }
            set
            {
                if (settingLensflares.value != value)
                {
                    settingLensflares.value = value;
                    pipelineChanges = true;
                }
                
            }
        }

        [UICheckBox]
        public bool VolumetricScattering
        {
            get { return settingVolumetricScattering.value; }
            set
            {
                if (settingVolumetricScattering.value != value)
                {
                    settingVolumetricScattering.value = value;
                    pipelineChanges = true;
                }
                
                
            }
        }

        [UICheckBox]
        public bool Tonemapping
        {
            get { return settingTonemapping.value; }
            set
            {
                if (settingTonemapping.value != value)
                {
                    settingTonemapping.value = value;
                    pipelineChanges = true;
                }
                    
                
            }
        }

        [UICheckBox]
        public bool FXAA
        {
            get { return settingFXAA.value; }
            set
            {
                if (settingFXAA.value != value)
                {
                    settingFXAA.value = value;
                    pipelineChanges = true;
                }
            }
        }

        [UICheckBox]
        public bool Grass
        {
            get { return settingGrass.value; }
            set
            {
                if (value == settingGrass.value)
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
                settingGrass.value = value;
            }
        }

        [UICheckBox]
        public bool ShowStats 
        {
            get 
            {
                return settingShowStats.value;
            }
            set
            {
                settingShowStats.value = value;
            }
        }

        [UISlider(10, 50)]
        public int LodFalloff
        {
            get { return (int)settingLodFalloff.value; }
            set 
            {
                settingLodFalloff.value = value;
                LocalScript.world.LodSettings.LodFalloff = value; 
            }
        }

        [UISlider(0, 100)]
        public int MinLodDistance
        {
            get { return (int)settingMinLodDistance.value; }
            set 
            {
                settingMinLodDistance.value = value;
                LocalScript.world.LodSettings.MinLodDistance = value; 
            }
        }

        [UISlider(0, 100)]
        public int MouseSensitivity
        {
            get 
            {
                return (int)(settingMouseSensitivity.value * 100); 
            }
            set 
            { 
                settingMouseSensitivity.value = value / 100.0; 
            }
        }

        public float MouseSensitivityF { get { return MouseSensitivity / 100f; } }

        [UISlider(20, 500)]
        public int MaxTreeDistance
        { 
            get
            {
                return (int)settingMaxTreeDistance.value;
            }
            set
            {
                float fadeOutMin = Math.Max(5, value - 5);     // >= 5
                float fadeOutMax = Math.Max(10, value + 5);    // >= 10
                float fadeTime = 1.0f; // 1 second
                
                settingMaxTreeDistance.value = value;
                UpvoidMinerWorldGenerator.setTreeLodSettings(fadeOutMin, fadeOutMax, fadeTime);
            }
        }

        [UIButton]
        public void ApplySettings()
        {
            // Write all settings to settings file

            Debug.Assert(allSettings.Count > 0);
            foreach(Setting set in allSettings)
            {
                set.SaveSetting();
            }

            // rebuild pipeline on changes
            // TODO: fixme
            //if (pipelineChanges)
            //    Rendering.SetupDefaultPipeline(LocalScript.camera);
            pipelineChanges = false;
        }

        [UIButton]
        public void ResetTutorial()
        {
            Tutorials.ResetTutorials();
        }

        [UIButton]
        public void ResetSettings()
        {
            // Reset local setting values to those from user settings
            Debug.Assert(allSettings.Count > 0);
            foreach (Setting set in allSettings)
            {
                set.reloadSettingFromFile();
            }

            
            ApplyInternalSize();

            // Re-apply the former settings
            Audio.SetVolumeForSpecificAudioType((float)settingMasterVolume.value / 100f, (int)AudioType.Master);
            Audio.SetVolumeForSpecificAudioType((float)settingSfxVolume.value / 100f, (int)AudioType.SFX);
            Audio.SetVolumeForSpecificAudioType(settingMuteMusic.value ? 0.0f : (float)settingMusicVolume.value / 100f, (int)AudioType.Music);

            LocalScript.camera.HorizontalFieldOfView = settingFieldOfView.value;

            pipelineChanges = false;
        }

        internal static void InitSettings()
        {
            if (settings == null)
                settings = new Settings();
        }
    }
}
