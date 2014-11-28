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
using Engine.Resources;
using Engine.Modding;

namespace UpvoidMiner
{
    /// <summary>
    /// Global upvoid miner instance
    /// </summary>
    public static class UpvoidMiner
    {
        /// <summary>
        /// Resource domain of this Mod
        /// </summary>
        public static ResourceDomain ModDomain;
        /// <summary>
        /// Mod Reference
        /// </summary>
        public static Module Mod;

        /// <summary>
        /// The base save path.
        /// </summary>
        public static string SavePathBase = "Savegames/Mods/UpvoidMiner";
        /// <summary>
        /// Save path for the inventory.
        /// </summary>
        public static string SavePathInventory = null;
        /// <summary>
        /// Save path for the turorial.
        /// </summary>
        public static string SavePathTutorial = null;
        /// <summary>
        /// Save path for the inventory.
        /// </summary>
        public static string SavePathWorldItems = null;
        /// <summary>
        /// Save path for entities.
        /// </summary>
        public static string SavePathEntities = null;
    }
}

