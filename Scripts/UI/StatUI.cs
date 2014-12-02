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
//
using System;
using EfficientUI;
using Engine.Universe;
using Engine.Memory;
using Engine;
using Engine.Physics;

namespace UpvoidMiner
{
    public class StatUI : UIProxy
    {
        [UIObject]
        public bool Visible { get { return Settings.settings != null && (Settings.settings.ShowStats || Universe.Below10FPS); } }

        [UIString]
        public int FPS
        {
            get
            {
                if (!Visible)
                    return -1;
                return Universe.TicksPerSecond;
            }
        }

        [UIObject]
        public bool Below10FPS { get { return Universe.Below10FPS; } }

        [UIString]
        public int MsPerFrame
        {
            get
            {
                if (!Visible)
                    return -1;
                return (int)(Universe.Timestep * 1000);
            }
        }

        [UIString]
        public string RamUsage
        {
            get
            {
                if (!Visible)
                    return "";
                var free = (MemoryCPU.AvailableMemory / 1024.0 / 1024.0).ToString("0.0");
                var used = (MemoryCPU.RootPool.TotalUsage / 1024.0 / 1024.0).ToString("0.0");
                return used + " MB (" + free + " MB free)";
            }
        }

        [UIString]
        public string VRamUsage
        {
            get
            {
                if (!Visible)
                    return "";
                var free = (MemoryGPU.AvailableMemory / 1024.0 / 1024.0).ToString("0.0");
                var used = (MemoryGPU.RootPool.TotalUsage / 1024.0 / 1024.0).ToString("0.0");
                return used + " MB (" + free + " MB free)";
            }
        }

        [UIString]
        public string CameraPos
        {
            get
            {
                if (!Visible)
                    return "";
                var pos = LocalScript.camera == null ? new vec3() : LocalScript.camera.Position;
                return vec2str(pos);
            }
        }

        [UIString]
        public string RayResult
        {
            get
            {
                if (!Visible)
                    return "";

                var camera = LocalScript.camera;
                var world = LocalScript.world;

                if (camera == null)
                    return "no camera";

                if (world == null)
                    return "no world";
                if (world.Physics == null)
                    return "no physics";

                var cbody = LocalScript.player == null ? null : LocalScript.player.Character.Body;

                RayHit hit = world.Physics.RayTest(camera.Position, camera.Position + camera.ForwardDirection * 100.0f, cbody);
                if (hit == null)
                    return "no hit";

                var res = "(" + vec2str(hit.Position) + ")";
                if (hit.CollisionBody != null && hit.CollisionBody.RefComponent != null)
                    res += " Entity#" + hit.CollisionBody.RefComponent.Entity.ID;
                if (hit.HasTerrainCollision)
                {
                    res += " Terrain: ";

                    TerrainMaterial mat = world.Terrain.QueryMaterialAtPosition(hit.Position, true);
                    if (mat == null)
                        res += "Air";
                    else
                        res += mat.Name;
                }
                return res;
            }
        }

        private string vec2str(vec3 pos)
        {
            return pos.x.ToString("0.00") + ", " + pos.y.ToString("0.00") + ", " + pos.z.ToString("0.00");
        }

        public StatUI() : base("EngineStats")
        {
        }
    }
}

