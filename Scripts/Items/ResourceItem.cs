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
using Engine.Physics;
using Engine.Universe;
using Engine.Rendering;
using Engine.Resources;
using Engine;

namespace UpvoidMiner
{
    /// <summary>
    /// An item that represents a resource based on a terrain material
    /// </summary>
    public class ResourceItem : VolumeItem
    {
        /// <summary>
        /// The terrain material that this resource represents.
        /// </summary>
        public readonly TerrainResource Material;

        public override string Identifier
        {
            get
            {
                return "01-Resources." + Material.Index.ToString("00") + "-" + Material.Name;
            }
        }
        
        public ResourceItem(TerrainResource material, float volume = 0f) :
            base(material.Name, "The terrain resource " + material.Name, 1.0f, ItemCategory.Resources, volume)
        {
            Material = material;
            Icon = material.Name;
        }

        /// <summary>
        /// This can be merged with resource items of the same resource
        /// </summary>
        public override bool TryMerge(Item rhs, bool subtract, bool force, bool dryrun = false)
        {
            ResourceItem item = rhs as ResourceItem;
            if ( item == null ) return false;
            if ( item.Material != Material ) return false;

            return Merge(item, subtract, force, dryrun);
        }

        /// <summary>
        /// Creates a copy of this item.
        /// </summary>
        public override Item Clone()
        {
            return new ResourceItem(Material, Volume);
        }

        #region Inventory Logic
        /// <summary>
        /// Renderjob for the preview sphere
        /// </summary>
        private MeshRenderJob previewShape;
        private MeshRenderJob previewShapeLimited;
        private MeshRenderJob previewShapeIndicator;
        private MeshRenderJob materialAlignmentGrid;
        private RenderComponent previewShapeRenderComp;
        private RenderComponent previewShapeLimitedRenderComp;
        private RenderComponent previewShapeIndicatorRenderComp;
        private RenderComponent materialAlignmentGridRenderComp;
        /// <summary>
        /// Radius of terrain material that is placed if "use"d.
        /// </summary>
        private float useRadius = 1f;

        /// <summary>
        /// Yes, we have a preview for resources.
        /// </summary>
        public override bool HasRayPreview { get { return true; } }

        public override void OnUse(Player player, vec3 _worldPos, vec3 _worldNormal, Entity _hitEntity)
        {
            float radius = useRadius, useVolume;
            switch (player.CurrentDiggingShape)
            {
                case DiggingController.DigShape.Box:
                    useVolume = 8 * useRadius * useRadius * useRadius;
                    if (useVolume > Volume)
                        radius = (float)Math.Pow(Volume / 8, 1 / 3f);
                    break;
                case DiggingController.DigShape.Cylinder:
                    useVolume = 2f * (float)Math.PI * useRadius * useRadius * useRadius;
                    if (useVolume > Volume)
                        radius = (float)Math.Pow(Volume / (2f * (float)Math.PI), 1 / 3f);
                    break;
                case DiggingController.DigShape.Sphere:
                    useVolume = 4f / 3f * (float)Math.PI * useRadius * useRadius * useRadius;
                    if (useVolume > Volume)
                        radius = (float)Math.Pow(Volume / (4f / 3f * (float)Math.PI), 1 / 3f);
                    break;
                case DiggingController.DigShape.Cone:
                    useVolume = 1f / 3f * (float)Math.PI * useRadius * useRadius * useRadius;
                    if (useVolume > Volume)
                        radius = (float)Math.Pow(Volume / (1f / 3f * (float)Math.PI), 1 / 3f);
                    break;
                default:
                    throw new InvalidOperationException("Unknown digging shape");
            }

            player.PlaceMaterial(Material, _worldNormal, _worldPos, radius);
        }

        public override void OnSelect(Player player)
        {
            // Use correct preview mesh
            MeshResource shapeMesh = null;
            MaterialResource shapeMat = null; 
            MaterialResource shapeMatLimited = null;
            switch (player.CurrentDiggingShape)
            {
                case DiggingController.DigShape.Box:
                    shapeMesh = Resources.UseMesh("::Debug/Box", null);
                    shapeMat = Resources.UseMaterial("Items/ConstructionPreviewBox", UpvoidMiner.ModDomain);
                    shapeMatLimited = Resources.UseMaterial("Items/ConstructionPreviewBoxLimited", UpvoidMiner.ModDomain);
                    break;
                case DiggingController.DigShape.Cylinder:
                    shapeMesh = Resources.UseMesh("::Debug/Cylinder", null);
                    shapeMat = Resources.UseMaterial("Items/ConstructionPreviewCylinder", UpvoidMiner.ModDomain);
                    shapeMatLimited = Resources.UseMaterial("Items/ConstructionPreviewCylinderLimited", UpvoidMiner.ModDomain);
                    break;
                case DiggingController.DigShape.Sphere: 
                    shapeMesh = Resources.UseMesh("::Debug/Sphere", null); 
                    shapeMat = Resources.UseMaterial("Items/ConstructionPreviewSphere", UpvoidMiner.ModDomain);
                    shapeMatLimited = Resources.UseMaterial("Items/ConstructionPreviewSphereLimited", UpvoidMiner.ModDomain);
                    break;
                case DiggingController.DigShape.Cone:
                    shapeMesh = Resources.UseMesh("::Debug/Cone", null);
                    shapeMat = Resources.UseMaterial("Items/ConstructionPreviewCone", UpvoidMiner.ModDomain);
                    shapeMatLimited = Resources.UseMaterial("Items/ConstructionPreviewConeLimited", UpvoidMiner.ModDomain);
                    break;
                default:
                    throw new InvalidOperationException("Unknown digging shape");
            }

            // Create an overlay sphere as 'fill-indicator'.
            previewShape = new MeshRenderJob(Renderer.Overlay.Mesh, shapeMat, shapeMesh, mat4.Scale(0f));
            previewShapeRenderComp = new RenderComponent(previewShape, mat4.Identity);
            LocalScript.ShapeIndicatorEntity.AddComponent(previewShapeRenderComp);

            // And a second one in case we are limited by the volume at hand.
            previewShapeLimited = new MeshRenderJob(Renderer.Overlay.Mesh, shapeMatLimited, shapeMesh, mat4.Scale(0f));
            previewShapeLimitedRenderComp = new RenderComponent(previewShapeLimited, mat4.Identity);
            LocalScript.ShapeIndicatorEntity.AddComponent(previewShapeLimitedRenderComp);

            // And a third one for indicating the center.
            previewShapeIndicator = new MeshRenderJob(Renderer.Overlay.Mesh, Resources.UseMaterial("Items/ResourcePreviewIndicator", UpvoidMiner.ModDomain), shapeMesh, mat4.Scale(0f));
            previewShapeIndicatorRenderComp = new RenderComponent(previewShapeIndicator, mat4.Identity);
            LocalScript.ShapeIndicatorEntity.AddComponent(previewShapeIndicatorRenderComp);

            // And a fourth one for the alignment grid.
            materialAlignmentGrid = new MeshRenderJob(Renderer.Overlay.Mesh, Resources.UseMaterial("Items/GridAlignment", UpvoidMiner.ModDomain), Resources.UseMesh("Triplequad", UpvoidMiner.ModDomain), mat4.Scale(0f));
            materialAlignmentGridRenderComp = new RenderComponent(materialAlignmentGrid, mat4.Identity);
            LocalScript.ShapeIndicatorEntity.AddComponent(materialAlignmentGridRenderComp);
        }

        public override void OnUseParameterChange(Player player, float _delta) 
        {
            // Adjust use-radius between 0.5m and 5m radius
            useRadius = Math.Max(0.5f, Math.Min(5f, useRadius + _delta / 5f));
        }

        public override void OnRayPreview(Player _player, RayHit rayHit, CrosshairInfo crosshair)
        {
            var _visible = rayHit != null;
            var _worldPos = rayHit == null ? vec3.Zero : rayHit.Position + rayHit.Normal.Normalized * (0.01f / 7f) /* small security offset */;
            var _worldNormal = rayHit == null ? vec3.UnitY : rayHit.Normal;

            var savPos = _worldPos;

            var indPos = _player.AlignPlacementPosition(_worldPos, _worldNormal, .1f);
            var oldWorldPos = _worldPos;
            _worldPos = _player.AlignPlacementPosition(_worldPos, _worldNormal, useRadius);
            vec3 dx, dy, dz;
            _player.AlignmentSystem(_worldNormal, out dx, out dy, out dz);
            mat4 rotMat = new mat4(dx, dy, dz, vec3.Zero);

            // If the indicated volume is greater than the available volume, show limitation sphere.
            float volumeFactor = 0;
            float useVolume;
            switch (_player.CurrentDiggingShape)
            {
                case DiggingController.DigShape.Box:
                    volumeFactor = 8;
                    break;
                case DiggingController.DigShape.Cylinder:
                    volumeFactor = 2 * (float)Math.PI;
                    break;
                case DiggingController.DigShape.Sphere:
                    volumeFactor = 4f / 3f * (float)Math.PI;
                    break;
                case DiggingController.DigShape.Cone:
                    volumeFactor = 1f / 3f * (float)Math.PI;
                    break;
                default:
                    throw new InvalidOperationException("Unknown digging shape");
            }
            useVolume = volumeFactor * useRadius * useRadius * useRadius;
            if (_visible && useVolume > Volume)
            {
                float availableRadius = (float)Math.Pow(Volume / volumeFactor, 1 / 3f);
                var limPos = _player.AlignPlacementPosition(oldWorldPos, _worldNormal, availableRadius);
                previewShapeLimitedRenderComp.Transform = mat4.Translate(limPos) * mat4.Scale(availableRadius) * rotMat;
                previewShapeLimited.SetColor("uMidPointAndRadius", new vec4(_worldPos, availableRadius));
            }
            else previewShapeLimitedRenderComp.Transform = mat4.Scale(0f);

            // Radius of the primary preview is always use-radius.
            previewShapeRenderComp.Transform = _visible ? mat4.Translate(_worldPos) * mat4.Scale(useRadius) * rotMat : mat4.Scale(0f);
            previewShape.SetColor("uMidPointAndRadius", new vec4(_worldPos, useRadius));
            previewShape.SetColor("uDigDirX", new vec4(dx, 0));
            previewShape.SetColor("uDigDirY", new vec4(dy, 0));
            previewShape.SetColor("uDigDirZ", new vec4(dz, 0));
            previewShapeLimited.SetColor("uDigDirX", new vec4(dx, 0));
            previewShapeLimited.SetColor("uDigDirY", new vec4(dy, 0));
            previewShapeLimited.SetColor("uDigDirZ", new vec4(dz, 0));

            materialAlignmentGrid.SetColor("uMidPointAndRadius", new vec4(_worldPos, _player.DiggingGridSize / 2.0f));
            materialAlignmentGrid.SetColor("uCursorPos", new vec4(savPos, 0));
            materialAlignmentGrid.SetColor("uTerrainNormal", new vec4(_worldNormal, 0));
            materialAlignmentGrid.SetColor("uDigDirX", new vec4(dx, 0));
            materialAlignmentGrid.SetColor("uDigDirY", new vec4(dy, 0));
            materialAlignmentGrid.SetColor("uDigDirZ", new vec4(dz, 0));

            bool gridAlignmentVisible = _player.CurrentDiggingAlignment == DiggingController.DigAlignment.GridAligned;
            materialAlignmentGridRenderComp.Transform = gridAlignmentVisible ? mat4.Translate(_worldPos) * mat4.Scale(2f * _player.DiggingGridSize) * rotMat : mat4.Scale(0f);

            // Indicator is always in the center and relatively small.
            previewShapeIndicatorRenderComp.Transform = _visible ? mat4.Translate(indPos) * mat4.Scale(.1f) * rotMat : mat4.Scale(0f);
        }

        public override void OnDeselect(Player player)
        {
            // Remove and delete it on deselect.
            LocalScript.ShapeIndicatorEntity.RemoveComponent(previewShapeRenderComp);
            LocalScript.ShapeIndicatorEntity.RemoveComponent(previewShapeLimitedRenderComp);
            LocalScript.ShapeIndicatorEntity.RemoveComponent(previewShapeIndicatorRenderComp);
            LocalScript.ShapeIndicatorEntity.RemoveComponent(materialAlignmentGridRenderComp);
            previewShape = null;
            previewShapeLimited = null;
            previewShapeIndicator = null;
            materialAlignmentGrid = null;
        }
        #endregion
    }
}

