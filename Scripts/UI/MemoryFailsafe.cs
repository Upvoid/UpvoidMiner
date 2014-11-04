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
using Engine.Memory;

namespace UpvoidMiner
{
    public class MemoryFailsafe : UIProxy
    {
        bool vis = false;

        [UIObject]
        public bool Visible
        {
            get
            {
                if (vis)
                    return true;
                vis = MemoryCPU.RootPool.TotalUsage > MemoryCPU.RootPool.Quota;
                return vis;
            }
        }

        [UIString]
        public string MemoryUsage
        {
            get
            {
                return (MemoryCPU.RootPool.TotalUsage / 1024.0 / 1024.0).ToString("0.0") + " MB";
            }
        }

        [UIString]
        public string MemoryFree
        {
            get
            {
                return (MemoryCPU.AvailableMemory / 1024.0 / 1024.0).ToString("0.0") + " MB";
            }
        }

        public MemoryFailsafe() : base("MemoryFailsafe")
        {
        }
    }
}

