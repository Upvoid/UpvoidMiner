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
            public string id;
            // JSON key, e.g. "Graphics/Shadow Resolution"
            public string desc;
            // a longer description, e.g. for tooltips, optional
            public Setting(string identifier, string description)
            {
                id = identifier;
                desc = description;
            }
            // Reload the setting from user.settings file
            public abstract void reloadSettingFromFile();
            // Reset the setting to the state before it was saved to file
            public abstract void ResetSetting();
            // Save the current setting to user.settings file
            public abstract void SaveSetting();
        }

        public class SettingDouble : Setting
        {
            public double value;
            public double defValue;
            public double preSaveValue;

            public SettingDouble(string identifier, double defaultValue, string description = "") :
                base(identifier, description)
            {
                defValue = defaultValue;
                preSaveValue = defaultValue;

                // Get initial value from file (or default value)
                reloadSettingFromFile();

                allSettings.Add(this);
            }

            public override void reloadSettingFromFile()
            {
                value = Scripting.GetUserSettingNumber(id, defValue);
                preSaveValue = value;
            }

            public override void ResetSetting()
            {
                value = preSaveValue;
            }

            public override void SaveSetting()
            {
                preSaveValue = value;
                Scripting.SetUserSettingNumber(id, value);
            }
        }

        public class SettingBool : Setting
        {
            public bool value;
            public bool defValue;
            public bool preSaveValue;

            public SettingBool(String identifier, bool defaultValue, String description = "") :
                base(identifier, description)
            {
                defValue = defaultValue;
                preSaveValue = defaultValue;

                // Get initial value from file (or default value)
                reloadSettingFromFile();

                allSettings.Add(this);
            }

            public override void reloadSettingFromFile()
            {
                value = Scripting.GetUserSetting(id, defValue);
                preSaveValue = value;
            }

            public override void ResetSetting()
            {
                value = preSaveValue;
            }

            public override void SaveSetting()
            {
                preSaveValue = value;
                Scripting.SetUserSetting(id, value);
            }
        }
        // Window Manager
        private SettingDouble settingResolutionWidth = new SettingDouble("WindowManager/Width", -1);
        private SettingDouble settingResolutionHeight = new SettingDouble("WindowManager/Height", -1);
        private SettingDouble settingFullscreen = new SettingDouble("WindowManager/FullscreenMode", -1);
        private SettingDouble settingInternalResolutionWidth = new SettingDouble("WindowManager/InternalWidth", -1);
        private SettingDouble settingInternalResolutionHeight = new SettingDouble("WindowManager/InternalHeight", -1);
        private SettingBool settingRestrictTo720p = new SettingBool("WindowManager/Restrict To 720p", false);
        // Audio
        private SettingDouble settingMasterVolume = new SettingDouble("Audio/Master Volume", 100);
        private SettingDouble settingSfxVolume = new SettingDouble("Audio/SFX Volume", 50);
        private SettingDouble settingMusicVolume = new SettingDouble("Audio/Music Volume", 50);
        private SettingBool settingMuteMusic = new SettingBool("Audio/Mute Music", false);
        // Graphics
        private SettingDouble settingAnisotropicFiltering = new SettingDouble("Graphics/Anisotropic Filtering", 4);
        private SettingDouble settingTextureResolution = new SettingDouble("Graphics/Texture Resolution", 512);
        private SettingDouble settingShadowResolution = new SettingDouble("Graphics/Shadow Resolution", 512);
        private SettingBool settingVolumetricScattering = new SettingBool("Graphics/Enable Volumetric Scattering", false);
        private SettingBool settingTonemapping = new SettingBool("Graphics/Enable Tonemapping", true);
        private SettingBool settingFXAA = new SettingBool("Graphics/Enable FXAA", true);
        private SettingBool settingLensflares = new SettingBool("Graphics/Enable Lensflares", false);
        private SettingDouble settingFieldOfView = new SettingDouble("Graphics/Field of View", 75.0);
        // LoD
        private SettingBool settingGrass = new SettingBool("Graphics/Enable Grass", true);
        private SettingBool settingDigParticles = new SettingBool("Graphics/Enable Dig Particles", true);
        private SettingDouble settingMaxTreeDistance = new SettingDouble("Graphics/Max Tree Distance", 150);
        private SettingDouble settingMinLodDistance = new SettingDouble("Graphics/Min Lod Distance", 20);
        private SettingDouble settingLodFalloff = new SettingDouble("Graphics/Lod Falloff", 30);
        // Other
        private SettingDouble settingMouseSensitivity = new SettingDouble("Input/Mouse Sensitivity", 0.5);
        private SettingBool settingHideTutorial = new SettingBool("Misc/Hide Tutorial", false);
        private SettingBool settingShowStats = new SettingBool("Debug/Show Stats", false);
        private bool pipelineChanges = false;
        private bool textureChanges = false;

        [UIObject]
        public bool ChangesOnApply { get { return pipelineChanges || textureChanges; } }

        [UICheckBox]
        public bool RestrictTo720p
        {
            get { return settingRestrictTo720p.value; }
            set
            {
                if (settingRestrictTo720p.value == value)
                    return;
                settingRestrictTo720p.value = value;
                pipelineChanges = true;
            }
        }

        [UITextBox]
        public string InternalSize
        {
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

        [UISlider(0, 4)]
        public int AnisotropicFiltering
        {
            get
            {
                switch ((int)settingAnisotropicFiltering.value)
                {
                    case 1:
                        return 0;
                    case 2:
                        return 1;
                    case 4:
                        return 2;
                    case 8:
                        return 3;
                    case 16:
                        return 4;
                    default:
                        return 0;
                }
            }
            set
            {
                int anisFilt;
                switch (value)
                {
                    case 0:
                        anisFilt = 1;
                        break;
                    case 1:
                        anisFilt = 2;
                        break;
                    case 2:
                        anisFilt = 4;
                        break;
                    case 3:
                        anisFilt = 8;
                        break;
                    case 4:
                        anisFilt = 16;
                        break;
                    default:
                        anisFilt = 1;
                        break;
                }
                if (settingAnisotropicFiltering.value != anisFilt)
                {
                    settingAnisotropicFiltering.value = anisFilt;
                    textureChanges = true;
                }
            }
        }

        [UIString]
        public string AnisotropicFilteringString
        {
            get { return (int)settingAnisotropicFiltering.value + "x"; }
        }

        [UISlider(0, 4)]
        public int TextureResolution
        {
            get
            {
                switch ((int)settingTextureResolution.value)
                {
                    case 128:
                        return 0;
                    case 256:
                        return 1;
                    case 512:
                        return 2;
                    case 1024:
                        return 3;
                    case 2048:
                        return 4;
                    default:
                        return 128;
                }
            }
            set
            {
                int texRes;
                switch (value)
                {
                    case 0:
                        texRes = 128;
                        break;
                    case 1:
                        texRes = 256;
                        break;
                    case 2:
                        texRes = 512;
                        break;
                    case 3:
                        texRes = 1024;
                        break;
                    case 4:
                        texRes = 2048;
                        break;
                    default:
                        texRes = 128;
                        break;
                }
                if (settingTextureResolution.value != texRes)
                {
                    settingTextureResolution.value = texRes;
                    textureChanges = true;
                }
            }
        }

        [UIString]
        public string TextureResolutionString
        {
            get { return (int)settingTextureResolution.value + "&sup2;"; }
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

        [UISlider(0, 4)]
        public int ShadowResolution
        {
            get
            {
                switch ((int)settingShadowResolution.value)
                {
                    case 0:
                        return 0;
                    case 256:
                        return 1;
                    case 512:
                        return 2;
                    case 1024:
                        return 3;
                    case 2048:
                        return 4;
                    default:
                        return 0;
                }
            }
            set
            {
                int shadowRes;
                switch (value)
                {
                    case 0:
                        shadowRes = 0;
                        break;
                    case 1:
                        shadowRes = 256;
                        break;
                    case 2:
                        shadowRes = 512;
                        break;
                    case 3:
                        shadowRes = 1024;
                        break;
                    case 4:
                        shadowRes = 2048;
                        break;
                    default:
                        shadowRes = 2;
                        break;
                }

                if (settingShadowResolution.value != shadowRes)
                {
                    settingShadowResolution.value = shadowRes;
                    pipelineChanges = true;
                }
            }
        }

        [UIString]
        public string ShadowResolutionString
        {
            get
            {
                if (settingShadowResolution.value <= 2)
                    return "none";
                else
                    return (int)settingShadowResolution.value + "&sup2;";
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
        public void SettingsPresetMin()
        {
            AnisotropicFiltering = 0;   // NOTE: This is the setting in [0..4]
            TextureResolution = 0;      // NOTE: This is the setting in [0..4]
            ShadowResolution = 0;       // NOTE: This is the setting in [0..4]
            VolumetricScattering = false;
            Tonemapping = false;
            FXAA = false;
            Grass = false;
            DigParticles = false;
            MinLodDistance = 0;
            LodFalloff = 10;
            MaxTreeDistance = 30;
            RestrictTo720p = true;
        }

        [UIButton]
        public void SettingsPresetLow()
        {
            AnisotropicFiltering = 1;// NOTE: This is the setting in [0..4]
            TextureResolution = 1;   // NOTE: This is the setting in [0..4]
            ShadowResolution = 1;    // NOTE: This is the setting in [0..4]
            VolumetricScattering = false;
            Tonemapping = false;
            FXAA = true;
            Grass = true;
            DigParticles = true;
            MinLodDistance = 10;
            LodFalloff = 20;
            MaxTreeDistance = 50;
            RestrictTo720p = false;
        }

        [UIButton]
        public void SettingsPresetMedium()
        {
            AnisotropicFiltering = 2;// NOTE: This is the setting in [0..4]
            TextureResolution = 2;   // NOTE: This is the setting in [0..4]
            ShadowResolution = 2;    // NOTE: This is the setting in [0..4]
            VolumetricScattering = false;
            Tonemapping = true;
            FXAA = true;
            Grass = true;
            DigParticles = true;
            MinLodDistance = 20;
            LodFalloff = 30;
            MaxTreeDistance = 100;
            RestrictTo720p = false;
        }

        [UIButton]
        public void SettingsPresetHigh()
        {
            AnisotropicFiltering = 3;  // NOTE: This is the setting in [0..4]
            TextureResolution = 3;     // NOTE: This is the setting in [0..4]
            ShadowResolution = 3;      // NOTE: This is the setting in [0..4]
            VolumetricScattering = true;
            Tonemapping = true;
            FXAA = true;
            Grass = true;
            DigParticles = true;
            MinLodDistance = 30;
            LodFalloff = 40;
            MaxTreeDistance = 200;
            RestrictTo720p = false;
        }

        [UIButton]
        public void SettingsPresetMax()
        {
            AnisotropicFiltering = 4; // NOTE: This is the setting in [0..4]
            TextureResolution = 4;    // NOTE: This is the setting in [0..4]
            ShadowResolution = 4;     // NOTE: This is the setting in [0..4]
            VolumetricScattering = true;
            Tonemapping = true;
            FXAA = true;
            Grass = true;
            DigParticles = true;
            MinLodDistance = 50;
            LodFalloff = 50;
            MaxTreeDistance = 300;
            RestrictTo720p = false;
        }

        private void SaveAllSettings()
        {
            // Write all settings to settings file
            Debug.Assert(allSettings.Count > 0);
            foreach (Setting set in allSettings)
            {
                set.SaveSetting();
            }
        }

        private void RebuildTextures()
        {
            // rebuild textures on changes only
            if (textureChanges)
                TextureResource.RebuildTextures();
            textureChanges = false;
        }

        private void RebuildPipeline()
        {
            // rebuild pipeline on changes only
            if (pipelineChanges)
                Rendering.SetupDefaultPipeline(LocalScript.camera);
            pipelineChanges = false;
        }

        [UIButton]
        public void ApplySettings()
        {
            if (settingInternalResolutionWidth.value != settingInternalResolutionWidth.preSaveValue ||
                settingInternalResolutionHeight.value != settingInternalResolutionHeight.preSaveValue)
            {
                ApplyInternalSize();
            }

            SaveAllSettings();
            RebuildPipeline();
            RebuildTextures();
        }

        [UIButton]
        public void ResetTutorial()
        {
            Tutorials.ResetTutorials();
        }

        [UIButton]
        public void ResetSettings()
        {
            // Reset setting values
            Debug.Assert(allSettings.Count > 0);
            foreach (Setting set in allSettings)
            {
                set.ResetSetting();
            }


            ApplyInternalSize();

            // Re-apply the former settings
            Audio.SetVolumeForSpecificAudioType((float)settingMasterVolume.value / 100f, (int)AudioType.Master);
            Audio.SetVolumeForSpecificAudioType((float)settingSfxVolume.value / 100f, (int)AudioType.SFX);
            Audio.SetVolumeForSpecificAudioType(settingMuteMusic.value ? 0.0f : (float)settingMusicVolume.value / 100f, (int)AudioType.Music);

            if (LocalScript.world != null)
            {
                LocalScript.world.LodSettings.LodFalloff = (float)settingLodFalloff.value;
                LocalScript.world.LodSettings.MinLodDistance = (float)settingMinLodDistance.value;
            }

            if (LocalScript.camera != null)
            {
                LocalScript.camera.HorizontalFieldOfView = settingFieldOfView.value;
            }


            {
                int maxTreeDist = (int)settingMaxTreeDistance.value;
                float fadeOutMin = Math.Max(5, maxTreeDist - 5);     // >= 5
                float fadeOutMax = Math.Max(10, maxTreeDist + 5);    // >= 10
                float fadeTime = 1.0f; // 1 second
                UpvoidMinerWorldGenerator.setTreeLodSettings(fadeOutMin, fadeOutMax, fadeTime);
            }


            pipelineChanges = false;
        }

        internal static void InitSettings()
        {
            if (settings == null)
                settings = new Settings();
        }
    }
}
