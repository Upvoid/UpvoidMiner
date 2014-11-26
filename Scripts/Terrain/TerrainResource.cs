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
using Engine.Universe;
using System.Diagnostics;
using System.Collections.Generic;
using Engine.Scripting;

namespace UpvoidMiner
{
    /// <summary>
    /// A material of the terrain.
    /// </summary>
    public abstract partial class TerrainResource
    {
        /// <summary>
        /// The underlying terrain material.
        /// </summary>
        public readonly TerrainMaterial Material;
        /// <summary>
        /// The index of the material
        /// </summary>
        public readonly int Index;
        /// <summary>
        /// Material name.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Density in kg/m^3
        /// </summary>
        public readonly float MassDensity;

        public TerrainResource(string name, float massDensity, bool translucent = false)
        {
            Material = Scripting.IsHost ? terrain.RegisterMaterial(name, translucent) : terrain.QueryMaterialFromName(name);
            Index = Material.MaterialIndex;
            Name = name;
            MassDensity = massDensity;
        }
    }
}

