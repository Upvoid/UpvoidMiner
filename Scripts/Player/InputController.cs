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
using System.Collections.Generic;
using System.Diagnostics;
using Engine;
using Engine.Audio;
using Engine.Universe;
using Engine.Modding;
using Engine.Resources;
using Engine.Scripting;
using Engine.Rendering;
using Engine.Physics;
using Engine.Network;
using Engine.Input;
using System.IO;
using Newtonsoft.Json;


namespace UpvoidMiner
{
    /// <summary>
    /// This class handles input events that are relevant for the player entity.
    /// </summary>
    public class InputController
    {
        private Player player;

        // Flags for modifier keys.
        bool keyModifierShift = false;
        bool keyModifierControl = false;
        bool keyModifierAlt = false;

        public InputController(Player _player)
        {
            player = _player;
            Input.OnPressInput += HandlePressInput;
            Input.OnAxisInput += HandleAxisInput;
        }

        void HandleAxisInput(object sender, InputAxisArgs e)
        {
            if (!Rendering.MainViewport.HasFocus)
                return;

            // CAUTION: this is currently in the wrong thread, isn't it?

            if (e.Axis == AxisType.MouseWheelY)
            {
                float delta = e.RelativeChange / 100;
                // Control + Wheel to change 'use-parameter'.
                if (keyModifierControl)
                {
                    Item selection = player.Inventory.Selection;
                    if (selection != null)
                        selection.OnUseParameterChange(player, delta);
                }
                else // Otherwise used to cycle through quick access.
                {
                    int newIdx = player.Inventory.SelectionIndex - (int)(delta);
                    while (newIdx < 0)
                        newIdx += Inventory.QuickAccessSlotCount;
                    player.Inventory.SelectQuickAccessSlot(newIdx % Inventory.QuickAccessSlotCount);
                }
            }
            else if (e.Axis == AxisType.MouseX)
            {
                if (!player.Gui.IsInventoryOpen)
                {
                    const float rotAzimuthSpeed = -.8f;
                    player.Lookaround(new vec2(e.RelativeChange * rotAzimuthSpeed, 0));
                }
            }
            else if (e.Axis == AxisType.MouseY)
            {
                if (!player.Gui.IsInventoryOpen)
                {
                    const float rotElevationSpeed = -.8f;
                    player.Lookaround(new vec2(0, e.RelativeChange * rotElevationSpeed));
                }
            }
        }

        void HandlePressInput(object sender, InputPressArgs e)
        {
            if (!Rendering.MainViewport.HasFocus)
                return;
            
            // Scale the area using + and - keys.
            // Translate it using up down left right (x, z)
            // and PageUp PageDown (y).
            if (e.PressType == InputPressArgs.KeyPressType.Down)
            {

                switch (e.Key)
                {
                    case InputKey.Shift:
                        keyModifierShift = true;
                        break;
                    case InputKey.Control:
                        keyModifierControl = true;
                        break;
                    case InputKey.Alt:
                        keyModifierAlt = true;
                        break;

                    case InputKey.F8:
                        Renderer.Opaque.Mesh.DebugWireframe = !Renderer.Opaque.Mesh.DebugWireframe;
                        break;

                    case InputKey.F12:
                        //Rendering.WriteNextFrameToFile("Brabbelsabbel.png");
                        break;

                    case InputKey.Q:
                        if (player.Inventory.Selection != null)
                            player.DropItem(player.Inventory.Selection);
                        break;

                    // F1 resets the player position
                    case InputKey.F1:
                        player.SetPosition(Player.SpawnPosition);
                        break;
                        
                    // Tab and shift-Tab cycle between digging shapes
                    case InputKey.Tab:

                        if (!keyModifierControl)
                        {
                            int vals = Enum.GetValues(typeof(DiggingController.DigShape)).Length;
                            int offset = keyModifierShift ? vals - 1 : 1;
                            player.CurrentDiggingShape = (DiggingController.DigShape)(((uint)player.CurrentDiggingShape - 1 + offset) % vals + 1);
                        }
                        else
                        {
                            int vals = Enum.GetValues(typeof(DiggingController.DigAlignment)).Length;
                            int offset = keyModifierShift ? vals - 1 : 1;
                            player.CurrentDiggingAlignment = (DiggingController.DigAlignment)(((uint)player.CurrentDiggingAlignment - 1 + offset) % vals + 1);
                        }

                        // Reselect to refresh shape
                        if (player.Inventory.Selection != null)
                        {
                            player.Inventory.Selection.OnDeselect(player);
                            player.Inventory.Selection.OnSelect(player);
                        }

                        break;
                        
                    default:
                        break;
                }

                // Quickaccess items.
                if (InputKey.Key1 <= e.Key && e.Key <= InputKey.Key9)
                    player.Inventory.SelectQuickAccessSlot((int)e.Key - (int)InputKey.Key1);
                if (e.Key == InputKey.Key0)
                    player.Inventory.SelectQuickAccessSlot(9); // Special '0'.
            }
            else if (e.PressType == InputPressArgs.KeyPressType.Up)
            {
                switch (e.Key)
                {
                    case InputKey.Shift:
                        keyModifierShift = false;
                        break;
                    case InputKey.Control:
                        keyModifierControl = false;
                        break;
                    case InputKey.Alt:
                        keyModifierAlt = false;
                        break;
                }
            }
 
            // Tell the player to use its current item while left mouse button is pressed
            if (e.Key == InputKey.MouseLeft)
            {
                if (player.Inventory.Selection != null && e.PressType == InputPressArgs.KeyPressType.Down && !player.Gui.IsInventoryOpen)
                    player.StartItemUse();
                else
                    player.StopItemUse();
            }

            if (e.Key == InputKey.E && e.PressType == InputPressArgs.KeyPressType.Down && !player.Gui.IsInventoryOpen)
            {
                player.TriggerInteraction();
            }

        }


    }
}