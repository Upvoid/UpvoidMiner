using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Engine.Webserver;
using Engine.Input;

namespace UpvoidMiner
{
    /// <summary>
    /// Provides the ingame information for all player-centric gui elements (like the inventory, minimap, character info, etc.)
    /// </summary>
    public class PlayerGui
    {
        Player player;

        JsonSerializer json = new JsonSerializer();
        WebSocketHandler updateSocket;

        // Container class for the data we send to the GUI client.
        [Serializable]
        class GuiInfo
        {
            [Serializable]
            public class GuiItem {
                public GuiItem(Item item)
                {
                    icon = item.Icon;
                    id = item.Id;
                    name = item.Name;
                    quantity = 1.0f;
                    isVolumetric = false;

                    VolumeItem volumeItem = item as VolumeItem;
                    if(volumeItem != null)
                    {
                        isVolumetric = true;
                        quantity = volumeItem.Volume;
                    }

                    DiscreteItem discreteItem = item as DiscreteItem;
                    if(discreteItem != null)
                    {
                        quantity = discreteItem.StackSize;
                    }
                }

                public string icon;
                public long id;
                public string name;
                public float quantity;
                public bool isVolumetric;

                public static List<GuiItem> FromItemCollection(IEnumerable<Item> items)
                {
                    List<GuiItem> guiItems = new List<GuiItem>();
                    foreach(Item item in items)
                    {
                        if (item != null)
                            guiItems.Add(new GuiItem(item));
                        else
                            guiItems.Add(null);
                    }

                    return guiItems;
                }
            }

            public List<GuiItem> inventory;
            public List<GuiItem> quickAccess;
            public int selection;
        }

        public PlayerGui(Player player)
        {
            // The index.html in htdocs/ contains the actual player gui. It contains javascript functions that get the ingame information displayed.
            // These dynamic content handlers provide that information.
            this.player = player;
            Webserver.DefaultWebserver.RegisterDynamicContent(LocalScript.ModDomain, "IngameGuiData", webInventory);
            Webserver.DefaultWebserver.RegisterDynamicContent(LocalScript.ModDomain, "SelectQuickAccessSlot", webSelectQuickAccessSlot);
            Webserver.DefaultWebserver.RegisterDynamicContent(LocalScript.ModDomain, "SelectItem", webSelectItem);
            updateSocket = Webserver.DefaultWebserver.RegisterWebSocketHandler(LocalScript.ModDomain, "InventoryUpdate");

            // On all relevant changes in the inventory, we order the GUI client to update itself.
            player.Inventory.OnSelectionChanged += (arg1, arg2) => OnUpdate();
            player.Inventory.OnQuickAccessChanged += (arg1, arg2) => OnUpdate();
            player.Inventory.Items.OnAdd += arg1 => OnUpdate();
            player.Inventory.Items.OnRemove += arg1 => OnUpdate();
            player.Inventory.Items.OnQuantityChange += arg1 => OnUpdate();

            // Workaround for missing keyboard input in the WebGui: Toggle the inventory from here
            Input.OnPressInput += (object sender, InputPressArgs e) => { if(e.Key == InputKey.I && e.PressType == InputPressArgs.KeyPressType.Down) updateSocket.SendMessage("ToggleInventory"); };
        }

        /// <summary>
        /// Is called when GUI changes for the player occurred.
        /// </summary>
        public void OnUpdate()
        {
            // This sends a message via our update socket to the GUI client, which will then update itself.
            updateSocket.SendMessage("Update");
        }

        void webInventory(WebRequest request, WebResponse response)
        {
            // Compile all relevant info for the gui into a GuiInfo instance and send it to the GUI client.
            GuiInfo info = new GuiInfo();
            info.inventory = GuiInfo.GuiItem.FromItemCollection(player.Inventory.Items);
            info.quickAccess = GuiInfo.GuiItem.FromItemCollection(player.Inventory.QuickAccessItems);
            info.selection = player.Inventory.SelectionIndex;

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);

            json.Serialize(jsonWriter, info);
            response.AppendBody(writer.GetStringBuilder().ToString());
        }

        void webSelectQuickAccessSlot(WebRequest request, WebResponse response)
        {
            // The GUI client calls this when a quick access slot is selected. Get the selected index and pass it to the player inventory.
            int selectedIndex = Convert.ToInt32(request.GetQuery("selectedIndex"));
            player.Inventory.Select(selectedIndex);
        }

        void webSelectItem(WebRequest request, WebResponse response)
        {
            // The GUI client calls this when a item in the inventory is selected.

            // Get the selected item
            int selectedItemId = Convert.ToInt32(request.GetQuery("selectedItem"));
            Item item = player.Inventory.Items.ItemById(selectedItemId);

            if (item == null)
                return;

            // Place the item in the quick access bar at position 9 (bound to key 0) and select it.
            player.Inventory.SetQuickAccess(item, 9);
            player.Inventory.Select(9);
        }
    }
}

