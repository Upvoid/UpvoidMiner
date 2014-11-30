using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EfficientUI;
using Engine;
using Engine.Resources;
using UpvoidMiner.UI;

namespace UpvoidMiner
{
    public class CraftingUI : UIProxy
    {
        public class BoxSettingsUI : UIProxy
        {
            [UISlider(3, 30)]
            public int Width { get; set; }

            [UISlider(3, 30)]
            public int Height { get; set; }

            [UISlider(3, 30)]
            public int Depth { get; set; }

            public float RequiredVolume { get { return Width * Height * Depth * 0.1f * 0.1f * 0.1f; } }

            [UIString]
            public string WidthInMeter { get { return (Width * 0.1f).ToString("0.0") + "m"; } }

            [UIString]
            public string HeightInMeter { get { return (Height * 0.1f).ToString("0.0") + "m"; } }

            [UIString]
            public string DepthInMeter { get { return (Depth * 0.1f).ToString("0.0") + "m"; } }

            public vec3 Size { get { return new vec3(Width, Height, Depth) * 0.1f; } }

            public BoxSettingsUI()
            {
                Width = Height = Depth = 10;
            }
        }

        public class SphereSettingsUI : UIProxy
        {
            [UISlider(3, 30)]
            public int Radius { get; set; }

            public float RequiredVolume { get { return 4f / 3f * (float)Math.PI * (float)Math.Pow(Radius * 0.1f, 3f); } }

            [UIString]
            public string RadiusInMeter { get { return (Radius * 0.1f).ToString("0.0") + "m"; } }

            public vec3 Size { get { return new vec3(Radius, Radius, Radius) * 0.1f; } }

            public SphereSettingsUI()
            {
                Radius = 10;
            }
        }

        public class CylinderSettingsUI : UIProxy
        {
            [UISlider(3, 30)]
            public int Radius { get; set; }

            [UISlider(3, 30)]
            public int Height { get; set; }

            public float RequiredVolume { get { return 2f * (float)Math.PI * (float)Math.Pow(Radius * 0.1f, 2f) * Height * 0.1f; } }

            [UIString]
            public string RadiusInMeter { get { return (Radius * 0.1f).ToString("0.0") + "m"; } }

            [UIString]
            public string HeightInMeter { get { return (Height * 0.1f).ToString("0.0") + "m"; } }

            public vec3 Size { get { return new vec3(Radius, Height, Radius) * 0.1f; } }

            public CylinderSettingsUI()
            {
                Radius = Height = 10;
            }
        }

        public Item SelectedItem
        {
            get
            {
                return LocalScript.player == null ? null : LocalScript.player.Inventory.Selection;
            }
        }

        private readonly BoxSettingsUI boxSettings = new BoxSettingsUI();
        private readonly SphereSettingsUI sphereSettings = new SphereSettingsUI();
        private readonly CylinderSettingsUI cylinderSettings = new CylinderSettingsUI();

        [UICollection("CraftBoxSettings")]
        public BoxSettingsUI BoxSettings { get { return TypeSelection == 0 ? boxSettings : null; } }

        [UICollection("CraftSphereSettings")]
        public SphereSettingsUI SphereSettings { get { return TypeSelection == 1 ? sphereSettings : null; } }

        [UICollection("CraftCylinderSettings")]
        public CylinderSettingsUI CylinderSettings { get { return TypeSelection == 2 ? cylinderSettings : null; } }

        [UIString]
        public string SelectedMaterialName
        {
            get
            {
                return SelectedItem is ResourceItem ? (SelectedItem as ResourceItem).Material.Name : "";
            }
        }

        [UIImage]
        public TextureDataResource SelectedMaterialIcon
        {
            get
            {
                return SelectedItem is ResourceItem ? (SelectedItem as ResourceItem).Material.Icon : null;
            }
        }

        [UIObject]
        public bool HasCraftingSettings
        {
            get
            {
                return SelectedItem is ResourceItem;
            }
        }

        [UIString]
        public string MassInKg
        {
            get
            {
                return (RequiredVolume *
                    (SelectedItem is ResourceItem ? (SelectedItem as ResourceItem).Material.MassDensity : 0f)).ToString("0");
            }
        }

        public float RequiredVolume
        {
            get
            {
                return TypeSelection == 0
                    ? boxSettings.RequiredVolume
                    : TypeSelection == 1 ? sphereSettings.RequiredVolume : cylinderSettings.RequiredVolume;
            }
        }

        [UIString]
        public string RequiredVolumeInCubicMeter { get { return RequiredVolume.ToString("0.000"); } }

        [UIString]
        public string CurrentVolumeInCubicMeter { get { 
                return (LocalScript.player == null || LocalScript.player.GodMode) ? "&nbsp;&infin;&nbsp;" :
                    SelectedItem is ResourceItem ? (SelectedItem as ResourceItem).Volume.ToString("0.000") : ""; } }

        [UIObject]
        public int TypeSelection { get; private set; }

        [UIObject]
        public bool CanCraft
        {
            get { return SelectedItem is ResourceItem && RequiredVolume <= (SelectedItem as ResourceItem).Volume; }
        }

        public CraftingUI()
            : base("Crafting")
        {
            UIProxyManager.AddProxy(this);
        }

        [UIButton]
        public void BtnBox()
        {
            TypeSelection = 0;
        }

        [UIButton]
        public void BtnSphere()
        {
            TypeSelection = 1;
        }

        [UIButton]
        public void BtnCylinder()
        {
            TypeSelection = 2;
        }

        [UIButton]
        public void BtnCraft()
        {
            if (!CanCraft)
                return;
            var resourceItem = SelectedItem as ResourceItem;
            if (resourceItem == null)
                return;
            var mat = resourceItem.Material;

            MaterialItem item;
            switch (TypeSelection)
            {
                case 0:
                    item = new MaterialItem(mat, MaterialShape.Cube, boxSettings.Size);
                    break;
                case 1:
                    item = new MaterialItem(mat, MaterialShape.Sphere, sphereSettings.Size);
                    break;
                case 2:
                    item = new MaterialItem(mat, MaterialShape.Cylinder, cylinderSettings.Size);
                    break;
                default:
                    return;
            }
            if (LocalScript.player == null)
                return;
            var newItem = LocalScript.player.Inventory.AddItem(item);

            // only if non-god
            if (!LocalScript.player.GodMode)
                LocalScript.player.Inventory.RemoveItem(new ResourceItem(mat, RequiredVolume));

            // Tutorial
            if (newItem && TypeSelection == 0 && mat.Name == "Dirt")
                Tutorials.MsgBasicCraftingDirtCube.Report(1);
            if (newItem && TypeSelection != 0 && mat.Name.StartsWith("Stone"))
                Tutorials.MsgBasicCraftingStoneNonCube.Report(1);
        }
    }
}
