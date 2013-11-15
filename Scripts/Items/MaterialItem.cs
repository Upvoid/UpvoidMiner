using System;
using System.Diagnostics;
using Engine;
using Engine.Universe;
using Engine.Rendering;
using Engine.Resources;
using Engine.Physics;

namespace UpvoidMiner
{
    /// <summary>
    /// Shape of a material item.
    /// </summary>
    public enum MaterialShape
    {
        Cube,
        Cylinder,
        Sphere
    }

    /// <summary>
    /// An item that is a material instance of a given resource.
    /// E.g. an Iron Sphere
    /// </summary>
    public class MaterialItem : DiscreteItem
    {
        /// <summary>
        /// The resource that his item is made of.
        /// </summary>
        public readonly TerrainResource Material;
        /// <summary>
        /// The shape of this item.
        /// </summary>
        public readonly MaterialShape Shape;
        /// <summary>
        /// Size of this item:
        /// Cube: width, height, depth
        /// Cylinder: radius, height, radius
        /// Sphere: radius, (y/z = radius)
        /// </summary>
        public readonly vec3 Size;

        public override string Identifier
        {
            get
            {
                return "02-Materials." + ((int)Shape).ToString("00") + "-" + Shape + "." + Material.Index.ToString("00") + "-" + Material.Name + "." + Size.ToString();
            }
        }

        /// <summary>
        /// Volume of this material item.
        /// </summary>
        public float Volume
        {
            get 
            {
                switch (Shape)
                {
                    case MaterialShape.Cube: return Size.x * Size.y * Size.z;
                    case MaterialShape.Cylinder: return 2 * (float)Math.PI * Size.x * Size.y * Size.z;
                    case MaterialShape.Sphere: return 4f / 3f * (float)Math.PI * Size.x * Size.y * Size.z;
                    default: Debug.Fail("Invalid shape"); return -1;
                }
            }
        }

        /// <summary>
        /// Gets a textual description of the dimensions
        /// </summary>
        public string DimensionString
        {
            get 
            {
                switch (Shape)
                {
                    case MaterialShape.Cube: return "size " + Size.x.ToString("0.0") + " m x " + Size.y.ToString("0.0") + " m x " + Size.z.ToString("0.0") + " m";
                    case MaterialShape.Cylinder: return Size.x.ToString("0.0") + " m radius and " + Size.y.ToString("0.0") + " m height";
                    case MaterialShape.Sphere: return Size.x.ToString("0.0") + " m radius";
                    default: Debug.Fail("Invalid shape"); return "<invalid>";
                }
            }
        }

        public MaterialItem(TerrainResource material, MaterialShape shape, vec3 size, int stackSize = 1):
            base(material.Name + " " + shape, null, 1.0f, ItemCategory.Material, stackSize)
        {
            Material = material;
            Shape = shape;
            Size = size;
            Description = "A " + shape + " made of " + material.Name + " with " + DimensionString;
            Icon = material.Name + "," + shape;
        }

        /// <summary>
        /// This can be merged with material items of the same resource and shape and size.
        /// </summary>
        public override bool TryMerge(Item rhs, bool subtract, bool force, bool dryrun = false)
        {
            MaterialItem item = rhs as MaterialItem;
            if ( item == null ) return false;
            if ( item.Material != Material ) return false;
            if ( item.Shape != Shape ) return false;
            if ( item.Size != Size ) return false;

            return Merge(item, subtract, force, dryrun);
        }

        /// <summary>
        /// Creates a copy of this item.
        /// </summary>
        public override Item Clone()
        {
            return new MaterialItem(Material, Shape, Size, StackSize);
        }

        
        
        #region Inventory Logic
        /// <summary>
        /// Renderjob for the preview material
        /// </summary>
        private MeshRenderJob previewMaterial;
        private MeshRenderJob previewMaterialPlaced;
        private MeshRenderJob previewMaterialPlacedIndicator;
        private bool previewPlacable = false;
        private mat4 previewPlaceMatrix;
        
        /// <summary>
        /// Yes, we have a preview for materials (ray for placement, update for holding).
        /// </summary>
        public override bool HasRayPreview { get { return true; } }
        public override bool HasUpdatePreview { get { return true; } }
        
        public override void OnUse(Player player, vec3 _worldPos)
        {
            if (!previewPlacable || player == null)
                return;

            Item droppedItem = new MaterialItem(Material, Shape, Size);
            player.Inventory.RemoveItem(droppedItem);
            
            ItemEntity itemEntity = new ItemEntity(droppedItem);
            player.ContainingWorld.AddEntity(itemEntity, previewPlaceMatrix);
        }
        
        public override void OnSelect()
        {
            MeshResource mesh;
            switch (Shape)
            {
                case MaterialShape.Cube: mesh = Resources.UseMesh("::Debug/Box", null); break;
                case MaterialShape.Sphere: mesh = Resources.UseMesh("::Debug/Sphere", null); break;
                case MaterialShape.Cylinder: mesh = Resources.UseMesh("::Debug/Cylinder", null); break;
                default: throw new NotImplementedException("Invalid shape");
            }

            MaterialResource material;
            if (Material is SolidTerrainResource)
                material = (Material as SolidTerrainResource).RenderMaterial;
            else
                throw new NotImplementedException("Unknown terrain resource");
            
            // Create a solid object for 'holding'.
            previewMaterial = new MeshRenderJob(Renderer.Opaque.Mesh, material, mesh, mat4.Scale(0f));
            LocalScript.world.AddRenderJob(previewMaterial);
            
            // Create a transparent object as 'placement-indicator'.
            previewMaterialPlaced = new MeshRenderJob(Renderer.Transparent.Mesh, Resources.UseMaterial("Items/ResourcePreview", UpvoidMiner.ModDomain), mesh, mat4.Scale(0f));
            LocalScript.world.AddRenderJob(previewMaterialPlaced);
            // And a second one for indicating the center.
            previewMaterialPlacedIndicator = new MeshRenderJob(Renderer.Transparent.Mesh, Resources.UseMaterial("Items/ResourcePreviewIndicator", UpvoidMiner.ModDomain), Resources.UseMesh("::Debug/Sphere", null), mat4.Scale(0f));
            LocalScript.world.AddRenderJob(previewMaterialPlacedIndicator);
        }
        
        public override void OnUseParameterChange(float _delta) 
        {
            // TODO: maybe rotate?
        }
        
        public override void OnRayPreview(Player _player, vec3 _worldPos, vec3 _worldNormal, bool _visible)
        {
            // Hide if not visible.
            if (!_visible)
            {
                previewMaterialPlaced.ModelMatrix = mat4.Scale(0f);
                previewMaterialPlacedIndicator.ModelMatrix = mat4.Scale(0f);
                previewPlacable = false;
                return;
            }

            vec3 dir = _player.CameraDirection;
            vec3 up = _worldNormal;
            vec3 left = vec3.cross(up, dir).Normalized;
            dir = vec3.cross(left, up);

            mat4 scaling;
            float offset;
            switch (Shape)
            {
                case MaterialShape.Cube: 
                    scaling = mat4.Scale(Size / 2f);
                    offset = Size.y / 2f;
                    break;
                case MaterialShape.Sphere: 
                    scaling = mat4.Scale(Size);
                    offset = Size.y;
                    break;
                case MaterialShape.Cylinder: 
                    scaling = mat4.Scale(new vec3(Size.x, Size.y / 2f, Size.z)); 
                    offset = Size.y / 2f;
                    break;
                default: throw new NotImplementedException("Invalid shape");
            }
            mat4 transform = new mat4(
                left, up, dir, _worldPos + (offset + .03f) * _worldNormal);
            
            previewPlacable = true;
            previewPlaceMatrix = transform;

            // The placed object is scaled accordingly
            previewMaterialPlaced.ModelMatrix = previewPlaceMatrix * scaling;
            // Indicator is always in the center and relatively small.
            previewMaterialPlacedIndicator.ModelMatrix = mat4.Translate(_worldPos) * mat4.Scale(.1f);
        }

        public override void OnUpdatePreview(Player _player, float _elapsedSeconds)
        {
            mat4 scaling;
            switch (Shape)
            {
                case MaterialShape.Cube: scaling = mat4.Scale(.15f); break;
                case MaterialShape.Sphere: scaling = mat4.Scale(.2f); break;
                case MaterialShape.Cylinder: scaling = mat4.Scale(.2f); break;
                default: throw new NotImplementedException("Invalid shape");
            }

            // Position the item preview in the right-lower corner of the screen.
            // FIXME: find proper alignment, current one is quite empirical.
            vec3 dir = _player.CameraDirection;
            vec3 up = vec3.UnitY;
            vec3 left = vec3.cross(up, dir).Normalized;
            vec3 offset = up * -.15f + left * -.7f + dir * 0.6f;
            mat4 transform = _player.Transformation * mat4.Translate(offset);

            previewMaterial.ModelMatrix = transform * scaling;
        }
        
        public override void OnDeselect()
        {
            // Remove and delete it on deselect.
            LocalScript.world.RemoveRenderJob(previewMaterial);
            LocalScript.world.RemoveRenderJob(previewMaterialPlaced);
            LocalScript.world.RemoveRenderJob(previewMaterialPlacedIndicator);
            previewMaterial = null;
            previewMaterialPlaced = null;
            previewMaterialPlacedIndicator = null;
        }
        #endregion

        #region Item Entity        
        /// <summary>
        /// Is called when an entity is created for this item (e.g. if dropped).
        /// This function is supposed to add renderjobs and physicscomponents.
        /// Don't forget to add components to the item entity!
        /// </summary>
        public override void SetupItemEntity(ItemEntity itemEntity, Entity entity)
        {
            // Create an appropriate physics shape.
            CollisionShape collShape;
            MeshResource mesh;
            mat4 scaling;
            switch (Shape)
            {
                case MaterialShape.Cube: 
                    collShape = new BoxShape(Size / 2f);
                    scaling = mat4.Scale(Size / 2f);
                    mesh = Resources.UseMesh("Box", UpvoidMiner.ModDomain);
                    break;
                case MaterialShape.Sphere: 
                    collShape = new SphereShape(Size.x);
                    scaling = mat4.Scale(Size);
                    mesh = Resources.UseMesh("Sphere", UpvoidMiner.ModDomain);
                    break;
                case MaterialShape.Cylinder: 
                    collShape = new CylinderShape(Size.x, Size.y);
                    mesh = Resources.UseMesh("Cylinder", UpvoidMiner.ModDomain);
                    scaling = mat4.Scale(new vec3(Size.x, Size.y / 2f, Size.z)); 
                    break;
                default: throw new NotImplementedException("Invalid Shape");
            }

            // Create the physical representation of the item.
            RigidBody body = itemEntity.ContainingWorld.Physics.CreateAndAddRigidBody(
                50f,
                entity.Transform,
                collShape
                );
            
            itemEntity.AddPhysicsComponent(new PhysicsComponent(body, mat4.Identity));
            
            MaterialResource material;
            if (Material is SolidTerrainResource)
                material = (Material as SolidTerrainResource).RenderMaterial;
            else
                throw new NotImplementedException("Unknown terrain resource");
            
            // Create the graphical representation of the item.
            MeshRenderJob renderJob = new MeshRenderJob(
                Renderer.Opaque.Mesh,
                material,
                mesh,
                mat4.Identity
                );
            itemEntity.AddRenderComponent(new RenderComponent(renderJob, scaling, true));
            
            MeshRenderJob renderJobShadow = new MeshRenderJob(
                Renderer.Shadow.Mesh, 
                Resources.UseMaterial("::Shadow", UpvoidMiner.ModDomain), 
                mesh,
                mat4.Identity
                );
            itemEntity.AddRenderComponent(new RenderComponent(renderJobShadow, scaling, true));
            
        }
        #endregion
    }
}

