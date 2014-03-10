using System;
using System.Diagnostics;
using System.Collections.Generic;
using Engine;
using Engine.Rendering;
using Engine.Resources;

namespace UpvoidMiner
{
    /// <summary>
    /// Type of tools
    /// </summary>
    public enum ToolType
    {
        Pickaxe,
        Shovel,
        Axe,
        Hammer,

        DroneChain
    }

    /// <summary>
    /// An item that is a tool
    /// </summary>
    public class ToolItem : DiscreteItem
    {
        /// <summary>
        /// Type of tool.
        /// </summary>
        public readonly ToolType ToolType;

        public override string Identifier
        {
            get
            {
                return "00-Tools." + ((int)ToolType).ToString("00") + "-" + Name;
            }
        }

        public ToolItem(ToolType type, int stackSize = 1) :
            base("", "", 1.0f, ItemCategory.Tools, stackSize)
        {
            ToolType = type;
            Icon = ToolType.ToString();
            switch (ToolType)
            {
                case ToolType.Pickaxe:
                    Name = "Pickaxe";
                    Description = "Tool used for mining stone.";
                    break;
                case ToolType.Shovel:
                    Name = "Shovel";
                    Description = "Tool used for excavating earth.";
                    break;
                case ToolType.Axe:
                    Name = "Axe";
                    Description = "Tool used for chopping trees.";
                    break;
                case ToolType.Hammer:
                    Name = "Hammer";
                    Description = "Tool used for crafting mechanics.";
                    break;

                case ToolType.DroneChain:
                    Name = "Chain Drone";
                    Description = "Drone used for creating chains of vertical digging constraints.";
                    break;

                default: Debug.Fail("Unknown tool type"); break;
            }
        }

        /// <summary>
        /// This can be merged with material items of the same resource and shape and size.
        /// </summary>
        public override bool TryMerge(Item rhs, bool subtract, bool force, bool dryrun = false)
        {
            ToolItem item = rhs as ToolItem;
            if ( item == null ) return false;
            if ( item.ToolType != ToolType ) return false;
            
            return Merge(item, subtract, force, dryrun);
        }

        /// <summary>
        /// Creates a copy of this item.
        /// </summary>
        public override Item Clone()
        {
            return new ToolItem(ToolType, StackSize);
        }

        /// <summary>
        /// Renderjobs for the preview sphere
        /// </summary>
        private MeshRenderJob previewShape;
        private MeshRenderJob previewShapeIndicator;

        /// <summary>
        /// Radius of terrain material that is removed if dug/picked
        /// </summary>
        private const float digRadiusShovelInitial = 1.4f;
        private const float digRadiusPickaxeInitial = 0.9f;
        private const float digRadiusMinFactor = 0.4f;

        private float digRadiusShovel = digRadiusShovelInitial;
        private float digRadiusPickaxe = digRadiusPickaxeInitial;

        public override void OnSelect(Player player)
        {
            // Use correct preview mesh
            MeshResource shapeMesh = null;
            MaterialResource shapeMat = null;
            switch (player.CurrentDiggingShape)
            {
                case Player.DiggingShape.Box:
                    shapeMesh = Resources.UseMesh("::Debug/Box", null);
                    shapeMat = Resources.UseMaterial("Items/DigPreviewBox", UpvoidMiner.ModDomain);
                    break;
                case Player.DiggingShape.Cylinder:
                    shapeMesh = Resources.UseMesh("::Debug/Cylinder", null);
                    shapeMat = Resources.UseMaterial("Items/DigPreviewCylinder", UpvoidMiner.ModDomain);
                    break;
                case Player.DiggingShape.Sphere:
                    shapeMesh = Resources.UseMesh("::Debug/Sphere", null);
                    shapeMat = Resources.UseMaterial("Items/DigPreviewSphere", UpvoidMiner.ModDomain);
                    break;
                default:
                    throw new InvalidOperationException("Unknown digging shape");
            }

            // Create a transparent sphere as 'fill-indicator'.
            previewShape = new MeshRenderJob(Renderer.Transparent.Mesh, shapeMat, shapeMesh, mat4.Scale(0f));
            LocalScript.world.AddRenderJob(previewShape);
            // And a second one for indicating the center.
            previewShapeIndicator = new MeshRenderJob(Renderer.Transparent.Mesh, Resources.UseMaterial("Items/ResourcePreviewIndicator", UpvoidMiner.ModDomain), shapeMesh, mat4.Scale(0f));
            LocalScript.world.AddRenderJob(previewShapeIndicator);
        }

        public override void OnDeselect(Player player)
        {
            // Remove and delete it on deselect.
            LocalScript.world.RemoveRenderJob(previewShape);
            LocalScript.world.RemoveRenderJob(previewShapeIndicator);
            previewShape = null;
            previewShapeIndicator = null;
        }

        public override void OnUseParameterChange(Player player, float _delta)
        {
            // Adjust dig-radius between 0.5m and 5m radius
            digRadiusShovel = Math.Max(0.5f, Math.Min(5f, digRadiusShovel + _delta / 5f));
            digRadiusPickaxe = Math.Max(0.5f, Math.Min(5f, digRadiusPickaxe + _delta / 5f));
        }

        /// <summary>
        /// Some items have a preview for their impact when used, others do not
        /// </summary>
        public override bool HasRayPreview{ get {
        switch (ToolType)
        {
            case ToolType.Pickaxe:
                return true;

            case ToolType.Shovel:
                return true;

            case ToolType.Axe:
                return false;

            case ToolType.DroneChain:
                return false;

            case ToolType.Hammer:
                return false;

            default:
                return false;
        }
        } }


        public override void OnRayPreview(Player _player, vec3 _worldPos, vec3 _worldNormal, bool _visible)
        {
            _worldPos = _player.AlignPlacementPosition(_worldPos);
            vec3 dx, dy, dz;
            _player.AlignmentSystem(_worldNormal, out dx, out dy, out dz);
            mat4 rotMat = new mat4(dx, dy, dz, vec3.Zero);

            // Limit shape if non-noclip
            if (!LocalScript.NoclipEnabled)
            {
                if (digRadiusShovel > digRadiusShovelInitial) digRadiusShovel = digRadiusShovelInitial;
                if (digRadiusShovel < digRadiusShovelInitial * digRadiusMinFactor) digRadiusShovel = digRadiusShovelInitial * digRadiusMinFactor;
                if (digRadiusPickaxe > digRadiusPickaxeInitial) digRadiusPickaxe = digRadiusPickaxeInitial;
                if (digRadiusPickaxe < digRadiusPickaxeInitial * digRadiusMinFactor) digRadiusPickaxe = digRadiusPickaxeInitial * digRadiusMinFactor;
            }

            float useRadius = 0.0f;
            switch (ToolType)
            {
                case ToolType.Pickaxe:
                    useRadius = digRadiusPickaxe; break;
                case ToolType.Shovel:
                    useRadius = digRadiusShovel; break;
                default: break;
            }
            // Set uniform for position and radius
            previewShape.SetColor("uMidPointAndRadius", new vec4(_worldPos, useRadius));
            previewShape.SetColor("uDigDirX", new vec4(dx, 0));
            previewShape.SetColor("uDigDirY", new vec4(dy, 0));
            previewShape.SetColor("uDigDirZ", new vec4(dz, 0));
            // Radius of the primary preview is always impact-radius of the current tool.
            previewShape.ModelMatrix = _visible ? mat4.Translate(_worldPos) * mat4.Scale(useRadius) * rotMat : mat4.Scale(0f);
            // Indicator is always in the center and relatively small.
            previewShapeIndicator.ModelMatrix = _visible ? mat4.Translate(_worldPos) * mat4.Scale(.1f) * rotMat : mat4.Scale(0f);
        }

        public override void OnUse(Player player, vec3 _worldPos, vec3 _worldNormal)
        {
            switch (ToolType)
            {
                case ToolType.Pickaxe:
                    // Pickaxe has small radius but can dig everywhere
                    player.DigMaterial(_worldNormal, _worldPos, digRadiusPickaxe, null);

                    return;

                case ToolType.Shovel:
                    // Shovel has big radius but can only dig dirt
					player.DigMaterial(_worldNormal, _worldPos, digRadiusShovel, new[] { TerrainResource.FromName("Dirt").Index, TerrainResource.FromName("Desert").Index });
                    return;

                case ToolType.Axe:
                    // TODO
                    return;

                case ToolType.DroneChain:
                    // Add a drone to the use-position.
                    player.AddDrone(_worldPos);
                    // Remove that drone from inventory.
                    player.Inventory.RemoveItem(new ToolItem(ToolType));
                    return;

                case ToolType.Hammer:
                    // TODO
                    return;

                default: 
                    throw new InvalidOperationException("Unknown tool");
            }
        }
    }
}

