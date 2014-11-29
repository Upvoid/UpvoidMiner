using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EfficientUI;

namespace UpvoidMiner
{
    public class UsageUI : UIProxy
    {
        public Item SelectedItem
        {
            get
            {
                return LocalScript.player == null ? null : LocalScript.player.Inventory.Selection;
            }
        }

        [UIString]
        public string Title { get { return SelectedItem is ResourceItem ? "Terrain Placement" : SelectedItem is MaterialItem ? "Item Placement" : "Digging"; } }

        private static bool IsDiggingType(ToolType type)
        {
            switch (type)
            {
                case ToolType.GodsShovel:
                case ToolType.Shovel:
                case ToolType.Pickaxe:
                    return true;
                default:
                    return false;
            }
        }

        [UIObject]
        public bool HasReplacement
        {
            get
            {
                return SelectedItem is ResourceItem;
            }
        }
        [UIObject]
        public bool HasPhysics
        {
            get
            {
                return SelectedItem is MaterialItem;
            }
        }
        [UIObject]
        public bool HasGridAngle
        {
            get
            {
                return HasPivotAlignment && LocalScript.player != null && (LocalScript.player.CurrentDiggingAlignment == DiggingController.DigAlignment.Axis || LocalScript.player.CurrentDiggingAlignment == DiggingController.DigAlignment.GridAligned);
            }
        }
        [UIObject]
        public bool HasGridSize
        {
            get
            {
                return HasPivotAlignment && LocalScript.player != null && LocalScript.player.CurrentDiggingAlignment == DiggingController.DigAlignment.GridAligned;
            }
        }

        [UIObject]
        public bool HasShape
        {
            get
            {
                return !(SelectedItem is MaterialItem);
            }
        }

        [UIObject]
        public bool HasPivotAlignment
        {
            get
            {
                return true;
            }
        }

        [UIObject]
        public bool HasUsageSettings
        {
            get
            {
                return SelectedItem is ResourceItem ||
                    SelectedItem is MaterialItem ||
                       (SelectedItem is ToolItem && IsDiggingType((SelectedItem as ToolItem).ToolType));
            }
        }

        [UIObject]
        public int CurrentShape { get { return LocalScript.player == null ? 1 : (int)LocalScript.player.CurrentDiggingShape; } }
        [UIObject]
        public int CurrentAlign { get { return LocalScript.player == null ? 1 : (int)LocalScript.player.CurrentDiggingAlignment; } }
        [UIObject]
        public int CurrentPivot { get { return LocalScript.player == null ? 1 : (int)LocalScript.player.CurrentDiggingPivot; } }
        [UIObject]
        public int CurrentReplace { get { return LocalScript.player == null ? 1 : (int)LocalScript.player.CurrentDiggingAddMode; } }
        [UIObject]
        public int CurrentPhysics { get { return LocalScript.player == null ? 1 : (int)LocalScript.player.CurrentPhysicsMode; } }

        [UISlider(0, 17)]
        public int AxisAngle
        {
            get
            {
                return LocalScript.player == null ? 0 : LocalScript.player.DiggingAlignmentAxisRotation;
            }
            set
            {
                if (LocalScript.player == null)
                    return;
                LocalScript.player.DiggingAlignmentAxisRotation = value;
            }
        }
        [UIString]
        public string AxisAngleString { get { return (AxisAngle * 5) + "&deg;"; } }

        [UISlider(1, 20)]
        public int GridSize
        {
            get
            {
                return LocalScript.player == null ? 0 : LocalScript.player.DiggingGridSize;
            }
            set
            {
                if (LocalScript.player == null)
                    return;
                LocalScript.player.DiggingGridSize = value;
            }
        }
        [UIString]
        public string GridSizeString { get { return (GridSize*0.5f) + "m"; } }

        public UsageUI()
            : base("Digging")
        {
            UIProxyManager.AddProxy(this);
        }

        [UIButton]
        public void ShapeSphere() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingShape = DiggingController.DigShape.Sphere; LocalScript.player.RefreshSelection(); }
        [UIButton]
        public void ShapeBox() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingShape = DiggingController.DigShape.Box; LocalScript.player.RefreshSelection(); }
        [UIButton]
        public void ShapeCylinder() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingShape = DiggingController.DigShape.Cylinder; LocalScript.player.RefreshSelection(); }
        [UIButton]
        public void ShapeCone() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingShape = DiggingController.DigShape.Cone; LocalScript.player.RefreshSelection(); }

        [UIButton]
        public void AlignAxis() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingAlignment = DiggingController.DigAlignment.Axis; LocalScript.player.RefreshSelection(); }
        [UIButton]
        public void AlignView() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingAlignment = DiggingController.DigAlignment.View; LocalScript.player.RefreshSelection(); }
        [UIButton]
        public void AlignTerrain() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingAlignment = DiggingController.DigAlignment.Terrain; LocalScript.player.RefreshSelection(); }
        [UIButton]
        public void AlignGrid() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingAlignment = DiggingController.DigAlignment.GridAligned; LocalScript.player.RefreshSelection(); }

        [UIButton]
        public void PivotTop() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingPivot = DiggingController.DigPivot.Top; LocalScript.player.RefreshSelection(); }
        [UIButton]
        public void PivotCenter() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingPivot = DiggingController.DigPivot.Center; LocalScript.player.RefreshSelection(); }
        [UIButton]
        public void PivotBottom() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingPivot = DiggingController.DigPivot.Bottom; LocalScript.player.RefreshSelection(); }

        [UIButton]
        public void ReplaceAir() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingAddMode = DiggingController.AddMode.AirOnly; }
        [UIButton]
        public void ReplaceMaterial() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingAddMode = DiggingController.AddMode.NonAirOnly; }
        [UIButton]
        public void ReplaceAll() { if (LocalScript.player == null) return; LocalScript.player.CurrentDiggingAddMode = DiggingController.AddMode.Overwrite; }

        [UIButton]
        public void PhysicsThrown() { if (LocalScript.player == null) return; LocalScript.player.CurrentPhysicsMode = DiggingController.PhysicsMode.Thrown; }
        [UIButton]
        public void PhysicsDynamic() { if (LocalScript.player == null) return; LocalScript.player.CurrentPhysicsMode = DiggingController.PhysicsMode.Dynamic; }
        [UIButton]
        public void PhysicsStatic() { if (LocalScript.player == null) return; LocalScript.player.CurrentPhysicsMode = DiggingController.PhysicsMode.Static; }
    }
}
