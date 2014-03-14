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

namespace UpvoidMiner
{
    // Message that is (usually) sent when the player interacts with the environment
    [Serializable]
    public class InteractionMessage
    {
        public Entity Sender;

        public InteractionMessage(Entity sender)
        {
            Sender = sender;
        }
    }

    // Basically for triggering the adding of an item
    [Serializable]
    public class AddItemMessage
    {
        public Item PickedItem;

        public AddItemMessage(Item pickedItem)
        {
            PickedItem = pickedItem;
        }
    }

    // This message shall be sent when something has been hit (usually by a rayquery)
    [Serializable]
    public class HitMessage
    {
        public Entity Sender;

        public HitMessage(Entity sender)
        {
            Sender = sender;
        }
    }
}
