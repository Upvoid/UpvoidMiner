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
using System.Diagnostics;
using Engine;
using Engine.Universe;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Rendering;
using Engine.Physics;
using Engine.Input;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UpvoidMiner
{
    /// <summary>
    /// Main class for the host script.
    /// </summary>
    public class HostScript
    {
        /// <summary>
        /// Starts
        /// </summary>
        public static void Startup(Module module)
        {
            // Get and save the resource domain of the mod, needed for loading resources.
            UpvoidMiner.Mod = module;
            UpvoidMiner.ModDomain = UpvoidMiner.Mod.ResourceDomain;

            // Create the world. Multiple worlds could be created here, but we only want one.
            // Use the UpvoidMinerWorldGenerator, which will create a simple terrain with some vegetation.
            World world = Universe.CreateWorld("UpvoidMinerWorld");
            UpvoidMinerWorldGenerator.init(world);
            world.Start();

            for (int i = 0; i<3; ++i)
            {
                TerrainResource mat = TerrainResource.FromName("Stone." + (i+1).ToString("00"));
                Debug.Assert(mat != null, "Invalid material");
                MaterialItem testItem = new MaterialItem(mat, MaterialShape.Cube, new vec3(1), 1);

                world.AddEntity(new ItemEntity(testItem, false), mat4.Translate(new vec3(5f, i * 2f, ((i % 3) * 2f))));
            }

        }
    }
}
