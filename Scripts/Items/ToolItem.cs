using System;
using System.Diagnostics;
using System.Collections.Generic;
using Engine;
using Engine.Physics;
using Engine.Rendering;
using Engine.Resources;
using Engine.Universe;
using UpvoidMiner.UI;

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
        GodsShovel,
        DroneChain
    }
    public enum ToolMaterial
    {
        Wood,
        Stone,
        Copper,
        Other,
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

        public readonly ToolMaterial ToolMaterial;

        public override string Identifier
        {
            get
            {
                return "00-Tools." + ((int)ToolType).ToString("00") + "-" + ((int)ToolMaterial).ToString("00") + "-" +  Name;
            }
        }

        public ToolItem(ToolType type, ToolMaterial material = ToolMaterial.Other, int stackSize = 1) :
            base("", "", 1.0f, ItemCategory.Tools, stackSize)
        {
            ToolType = type;
            ToolMaterial = material;
            Icon = ToolType + (ToolMaterial == ToolMaterial.Other ? "" :  "," + ToolMaterial + "Mat");
            string materialString = "";
            switch (material)
            {
                case ToolMaterial.Wood:
                    materialString = "Wooden";
                    break;
                case ToolMaterial.Stone:
                    materialString = "Stone";
                    break;
                case ToolMaterial.Copper:
                    materialString = "Copper";
                    break;
            }
            switch (ToolType)
            {
                case ToolType.Pickaxe:
                    Name = materialString + " Pickaxe";
                    Description = "Tool used for mining stone.";
                    break;
                case ToolType.Shovel:
                    Name = materialString + " Shovel";
                    Description = "Tool used for excavating earth.";
                    break;
                case ToolType.Axe:
                    Name = materialString + " Axe";
                    Description = "Tool used for chopping trees.";
                    break;
                case ToolType.GodsShovel:
                    Name = "God's Shovel";
                    Description = "The epic shovel of god.";
                    break;
                case ToolType.Hammer:
                    Name = materialString + " Hammer";
                    Description = "Tool used for crafting mechanics.";
                    break;

                case ToolType.DroneChain:
                    Name = "Chain Drone";
                    Description = "Drone used for creating chains of vertical digging constraints.";
                    break;

                default:
                    Debug.Fail("Unknown tool type");
                    break;
            }
        }

        /// <summary>
        /// This can be merged with material items of the same resource and shape and size.
        /// </summary>
        public override bool TryMerge(Item rhs, bool subtract, bool force, bool dryrun = false)
        {
            ToolItem item = rhs as ToolItem;
            if (item == null)
                return false;
            if (item.ToolType != ToolType)
                return false;
            if (item.ToolMaterial != ToolMaterial)
                return false;
            
            return Merge(item, subtract, force, dryrun);
        }

        /// <summary>
        /// Creates a copy of this item.
        /// </summary>
        public override Item Clone()
        {
            return new ToolItem(ToolType, ToolMaterial, StackSize);
        }

        /// <summary>
        /// Renderjobs and -components for the preview sphere
        /// </summary>
        private MeshRenderJob previewShape;
        private MeshRenderJob previewShapeIndicator;
        private MeshRenderJob materialAlignmentGrid;
        private RenderComponent previewShapeRenderComp;
        private RenderComponent previewShapeIndicatorRenderComp;
        private RenderComponent materialAlignmentGridRenderComp;
        /// <summary>
        /// Radius of terrain material that is removed if dug/picked
        /// </summary>
        private const float digRadiusShovelInitial = 1.4f;
        private const float digRadiusPickaxeInitial = 0.9f;
        private const float digRadiusMinFactor = 0.4f;
        private float digRadiusShovel = digRadiusShovelInitial;
        private float digRadiusPickaxe = digRadiusPickaxeInitial;

        public float DigRadiusShovel { get { return digRadiusShovel; } }

        public float DigRadiusPickaxe { get { return digRadiusPickaxe; } }

        public override void OnSelect(Player player)
        {
            // Use correct preview mesh
            MeshResource shapeMesh = null;
            MaterialResource shapeMat = null;
            switch (player.CurrentDiggingShape)
            {
                case DiggingController.DigShape.Box:
                    shapeMesh = Resources.UseMesh("::Debug/Box", null);
                    shapeMat = Resources.UseMaterial("Items/DigPreviewBox", UpvoidMiner.ModDomain);
                    break;
                case DiggingController.DigShape.Cylinder:
                    shapeMesh = Resources.UseMesh("::Debug/Cylinder", null);
                    shapeMat = Resources.UseMaterial("Items/DigPreviewCylinder", UpvoidMiner.ModDomain);
                    break;
                case DiggingController.DigShape.Sphere:
                    shapeMesh = Resources.UseMesh("::Debug/Sphere", null);
                    shapeMat = Resources.UseMaterial("Items/DigPreviewSphere", UpvoidMiner.ModDomain);
                    break;
                case DiggingController.DigShape.Cone:
                    shapeMesh = Resources.UseMesh("::Debug/Cone", null);
                    shapeMat = Resources.UseMaterial("Items/DigPreviewCone", UpvoidMiner.ModDomain);
                    break;
                default:
                    throw new InvalidOperationException("Unknown digging shape");
            }

            // Create an overlay sphere as 'fill-indicator'.
            previewShape = new MeshRenderJob(Renderer.Overlay.Mesh, shapeMat, shapeMesh, mat4.Scale(0f));
            previewShapeRenderComp = new RenderComponent(previewShape, mat4.Identity);
            LocalScript.ShapeIndicatorEntity.AddComponent(previewShapeRenderComp);

            // And a second one for indicating the center.
            previewShapeIndicator = new MeshRenderJob(Renderer.Overlay.Mesh, Resources.UseMaterial("Items/ResourcePreviewIndicator", UpvoidMiner.ModDomain), shapeMesh, mat4.Scale(0f));
            previewShapeIndicatorRenderComp = new RenderComponent(previewShapeIndicator, mat4.Identity);
            LocalScript.ShapeIndicatorEntity.AddComponent(previewShapeIndicatorRenderComp);

            // And a third one for the alignment grid.
            materialAlignmentGrid = new MeshRenderJob(Renderer.Overlay.Mesh, Resources.UseMaterial("Items/GridAlignment", UpvoidMiner.ModDomain), Resources.UseMesh("Triplequad", UpvoidMiner.ModDomain), mat4.Scale(0f));
            materialAlignmentGridRenderComp = new RenderComponent(materialAlignmentGrid, mat4.Identity);
            LocalScript.ShapeIndicatorEntity.AddComponent(materialAlignmentGridRenderComp);
        }

        public override void OnDeselect(Player player)
        {
            // Remove and delete it on deselect.
            LocalScript.ShapeIndicatorEntity.RemoveComponent(previewShapeRenderComp);
            LocalScript.ShapeIndicatorEntity.RemoveComponent(previewShapeIndicatorRenderComp);
            LocalScript.ShapeIndicatorEntity.RemoveComponent(materialAlignmentGridRenderComp);
            previewShape = null;
            previewShapeIndicator = null;
            materialAlignmentGrid = null;
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
                return true;

            case ToolType.GodsShovel:
                return true;

            case ToolType.DroneChain:
                return true;

            case ToolType.Hammer:
                return false;

            default:
                return false;
        }
        } }


        public override void OnRayPreview(Player _player, RayHit rayHit, CrosshairInfo crosshair)
        {
            var _visible = rayHit != null;
            var _worldPos = rayHit == null ? vec3.Zero : rayHit.Position + rayHit.Normal.Normalized * (0.01f / 7f) /* small security offset */;
            var _worldNormal = rayHit == null ? vec3.UnitY : rayHit.Normal;

            var savPos = _worldPos;

            crosshair.Disabled = rayHit == null;

            if (ToolType == ToolType.Axe)
            {
                if (rayHit != null && rayHit.HasTerrainCollision)
                    crosshair.Disabled = true;
                return;
            }
            if (ToolType == ToolType.DroneChain)
            {
                return;
            }

            //crosshair.IconClass = 

            TerrainMaterial mat = _visible ? _player.ContainingWorld.Terrain.QueryMaterialAtPosition(_worldPos, true) : null;

            vec3 dx, dy, dz;
            _player.AlignmentSystem(_worldNormal, out dx, out dy, out dz);
            mat4 rotMat = new mat4(dx, dy, dz, vec3.Zero);

            // Limit shape if non-noclip
            if (!LocalScript.NoclipEnabled && ToolType != ToolType.GodsShovel)
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
                    // disable on non-dirt
                    if (mat != null && mat.Name == "Dirt")
                        crosshair.Disabled = false;
                    else crosshair.Disabled = true;
                    useRadius = digRadiusShovel; break;
                case ToolType.GodsShovel:
                    useRadius = digRadiusShovel; break;
                default: break;
            }
            _worldPos = _player.AlignPlacementPosition(_worldPos, _worldNormal, useRadius);
            // Set uniform for position and radius
            previewShape.SetColor("uMidPointAndRadius", new vec4(_worldPos, useRadius));
            previewShape.SetColor("uDigDirX", new vec4(dx, 0));
            previewShape.SetColor("uDigDirY", new vec4(dy, 0));
            previewShape.SetColor("uDigDirZ", new vec4(dz, 0));

            materialAlignmentGrid.SetColor("uMidPointAndRadius", new vec4(_worldPos, _player.DiggingGridSize / 2.0f));
            materialAlignmentGrid.SetColor("uCursorPos", new vec4(savPos, 0));
            materialAlignmentGrid.SetColor("uTerrainNormal", new vec4(_worldNormal, 0));
            materialAlignmentGrid.SetColor("uDigDirX", new vec4(dx, 0));
            materialAlignmentGrid.SetColor("uDigDirY", new vec4(dy, 0));
            materialAlignmentGrid.SetColor("uDigDirZ", new vec4(dz, 0));
            // Radius of the primary preview is always impact-radius of the current tool.
            previewShapeRenderComp.Transform = _visible ? mat4.Translate(_worldPos) * mat4.Scale(useRadius) * rotMat : mat4.Scale(0f);

            bool gridAlignmentVisible = _player.CurrentDiggingAlignment == DiggingController.DigAlignment.GridAligned;
            materialAlignmentGridRenderComp.Transform = gridAlignmentVisible ? mat4.Translate(_worldPos) * mat4.Scale(2f * _player.DiggingGridSize) * rotMat : mat4.Scale(0f);
            // Indicator is always in the center and relatively small.
            previewShapeIndicatorRenderComp.Transform = _visible ? mat4.Translate(_worldPos) * mat4.Scale(.1f) * rotMat : mat4.Scale(0f);
        }

        public override void OnUse(Player player, vec3 _worldPos, vec3 _worldNormal, Entity _hitEntity)
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

                case ToolType.GodsShovel:
                    // Shovel has big radius but can only dig dirt
                    player.DigMaterial(_worldNormal, _worldPos, digRadiusShovel, null);
                    return;

                case ToolType.Axe:

                    if (_hitEntity != null)
                    {
                        // Hitmessage to make the log know it has been hit
                        _hitEntity[TriggerId.getIdByName("Hit")] |= new HitMessage(player.thisEntity);
                    }
                    return;

                case ToolType.DroneChain:
                    // Add a drone to the use-position.
                    player.AddDrone(_worldPos);
                    // Remove that drone from inventory.
                    if (!player.GodMode)
                        player.Inventory.RemoveItem(new ToolItem(ToolType));

                    // Tutorial
                    Tutorials.MsgAdvancedBuildingPlaceDrone.Report(1);
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

