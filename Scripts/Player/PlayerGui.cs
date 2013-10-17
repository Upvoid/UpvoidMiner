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
        /// <summary>
        /// Returns true if any form of UI is open (and mouse should be visible and movable).
        /// </summary>
        public bool IsGuiOpen { get; private set; }

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
                    identifier = item.Identifier;
                    name = item.Name;
                    quickAccessSlot = item.QuickAccessIndex;
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

                public string icon = "";
                public long id = -1;
                public string name = "";
                public string identifier = "";
                public int quickAccessSlot = -1;
                public float quantity = 0;
                public bool isVolumetric = false;
                public bool hasDiscoveredCraftingRule = false;
                public bool canBeCrafted = false;
                public bool canBeDismantled = false;
                public List<GuiItem> craftingIngredients = new List<GuiItem>();

                public static Dictionary<string, GuiItem> FromItemCollection(IEnumerable<Item> items)
                {
                    Dictionary<string, GuiItem> guiItems = new Dictionary<string, GuiItem>();
                    foreach(Item item in items)
                    {
                        guiItems.Add(item.Identifier, new GuiItem(item));
                    }

                    return guiItems;
                }
            }

            public Dictionary<string, GuiItem> inventory = new Dictionary<string, GuiItem>();
            public List<string> quickAccess = new List<string>();
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
            Webserver.DefaultWebserver.RegisterDynamicContent(LocalScript.ModDomain, "DropItem", webDropItem);
            updateSocket = Webserver.DefaultWebserver.RegisterWebSocketHandler(LocalScript.ModDomain, "InventoryUpdate");

            // On all relevant changes in the inventory, we order the GUI client to update itself.
            player.Inventory.OnSelectionChanged += (arg1, arg2) => OnUpdate();
            player.Inventory.OnQuickAccessChanged += (arg1, arg2) => OnUpdate();
            player.Inventory.Items.OnAdd += arg1 => OnUpdate();
            player.Inventory.Items.OnRemove += arg1 => OnUpdate();
            player.Inventory.Items.OnQuantityChange += arg1 => OnUpdate();

            // Workaround for missing keyboard input in the WebGui: Toggle the inventory from here
            Input.OnPressInput += (object sender, InputPressArgs e) => 
            { 
                if(e.Key == InputKey.I && e.PressType == InputPressArgs.KeyPressType.Down) 
                {
                    IsGuiOpen = !IsGuiOpen;
                    updateSocket.SendMessage("ToggleInventory"); 
                }
            };
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

            foreach (var item in player.Inventory.QuickAccessItems)
            {
                if (item != null)
                    info.quickAccess.Add(item.Identifier);
                else
                    info.quickAccess.Add("");
            }

            info.selection = player.Inventory.SelectionIndex;

            foreach (CraftingRule cr in player.Inventory.DiscoveredRules)
            {
                if (info.inventory.ContainsKey(cr.Result.Identifier))
                {
                    GuiInfo.GuiItem item = info.inventory[cr.Result.Identifier];
                    item.hasDiscoveredCraftingRule = true;
                    if (cr.CanBeDismantled)
                        item.canBeDismantled = true;
                    if (cr.IsCraftable(player.Inventory.Items.ItemFromIdentifier(item.identifier), player.Inventory.Items))
                    {
                        item.canBeCrafted = true;
                    }

                    item.craftingIngredients = new List<GuiInfo.GuiItem>(GuiInfo.GuiItem.FromItemCollection(cr.Ingredients).Values);
                }
                else
                {
                    GuiInfo.GuiItem virtualItem = new GuiInfo.GuiItem(cr.Result);
                    virtualItem.quantity = 0;
                    virtualItem.hasDiscoveredCraftingRule = true;

                    if (cr.IsCraftable(null, player.Inventory.Items))
                    {
                        virtualItem.canBeCrafted = true;
                    }

                    virtualItem.craftingIngredients = new List<GuiInfo.GuiItem>(GuiInfo.GuiItem.FromItemCollection(cr.Ingredients).Values);

                    info.inventory.Add(cr.Result.Identifier, virtualItem);
                }
            }

            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);
            json.Formatting = Formatting.Indented;
            json.Serialize(jsonWriter, info);
            response.AddHeader("Content-Type", "application/json");
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
            int selectedItemId = Convert.ToInt32(request.GetQuery("itemId"));
            Item item = player.Inventory.Items.ItemById(selectedItemId);

            if (item == null)
                return;

            // Place the item in the quick access bar at position 9 (bound to key 0) and select it.
            player.Inventory.SetQuickAccess(item, 9);
            player.Inventory.Select(9);
        }

        void webDropItem(WebRequest request, WebResponse response)
        {
            // The GUI client calls this when an item is droppped from the inventory.

            int itemId = Convert.ToInt32(request.GetQuery("itemId"));
            Item item = player.Inventory.Items.ItemById(itemId);

            if (item == null)
                return;

            player.DropItem(item);
        }

        void webDismantleItem(WebRequest request, WebResponse response)
        {
            // The GUI client calls this when an item is droppped from the inventory.

            int itemId = Convert.ToInt32(request.GetQuery("itemId"));
            Item item = player.Inventory.Items.ItemById(itemId);

            if (item == null)
                return;

            // For dismantling, we need a crafting rule that results in the given item
            foreach (var cr in player.Inventory.DiscoveredRules) {
                if (cr.Result.Identifier == item.Identifier)
                {
                    //TODO: perform dismantling
                    break;
                }
            }
        }

        void webCraftItem(WebRequest request, WebResponse response)
        {
            // The GUI client calls this when the player crafts an item.

            string itemIdentifier = request.GetQuery("itemIdentifier");
            Item item = player.Inventory.Items.ItemFromIdentifier(itemIdentifier);

            if (item == null)
                return;

            // For crafting, we need a crafting rule that results in the given item
            foreach (var cr in player.Inventory.DiscoveredRules) {
                if (cr.Result.Identifier == item.Identifier)
                {
                    cr.Craft(item, player.Inventory.Items);
                    break;
                }
            }
        }
    }
}

