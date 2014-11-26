using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Engine.Physics;
using Engine.Universe;

namespace UpvoidMiner
{
    public class PipetteItem : DiscreteItem
    {
        public PipetteItem(int stackSize = 1) :
            base("Pipette", "A tool to autmatically select the material under the cursor for building.", 1.0f, ItemCategory.Tools, stackSize)
        {
            Icon = "Pipette";
        }

        public override string Identifier
        {
            get { return "00-Tools.Pipette"; }
        }


        public override bool TryMerge(Item rhs, bool substract, bool force, bool dryrun = false)
        {
            PipetteItem otherItem = rhs as PipetteItem;
            if (otherItem == null)
                return false;
            else
                return Merge(otherItem, substract, force, dryrun);
        }

        public override Item Clone()
        {
            return new PipetteItem();
        }
        public override bool HasRayPreview { get { return true; } }

        public override void OnRayPreview(Player _player, RayHit rayHit, CrosshairInfo crosshair)
        {
            crosshair.IconClass = "eyedropper";
            crosshair.Disabled = rayHit == null || !rayHit.HasTerrainCollision;
        }

        public override void OnUse(Player player, Engine.vec3 _worldPos, Engine.vec3 _worldNormal, Engine.Universe.Entity _hitEntity)
        {
            // Don't do anything (yet) if an entity was selected
            if (_hitEntity != null)
                return;
            else
            {
                // Get the material at the position where the tool has been used
                TerrainMaterial mat = player.ContainingWorld.Terrain.QueryMaterialAtPosition(_worldPos, true);
                // Look for the material in the player's inventory and select it
                foreach (var item in player.Inventory.Items)
                {
                    ResourceItem resItem = item as ResourceItem;
                    if (resItem == null)
                        continue;
                    if (resItem.Material.Material == mat)
                        player.Inventory.SelectItem(item);
                }
            }

        }
    }
}
