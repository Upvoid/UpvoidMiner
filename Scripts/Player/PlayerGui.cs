using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Engine.Webserver;

namespace UpvoidMiner
{
    /// <summary>
    /// Provides the ingame information for all player-centric gui elements (like the inventory, minimap, character info, etc.)
    /// </summary>
    public class PlayerGui
    {
        Player player;

        JsonSerializer json = new JsonSerializer();

        public PlayerGui(Player player)
        {
            // The index.html in htdocs/ contains the actual player gui. It contains javascript functions that get the ingame information displayed.
            // These dynamic content handlers provide that information.
            this.player = player;
            Webserver.DefaultWebserver.RegisterDynamicContent(LocalScript.ModDomain, "Inventory", webInventory);
        }

        void webInventory(WebRequest request, WebResponse response)
        {
            // For now, the inventory is displayed as a list of the item names.

            List<CraftingRule> craftingRules = player.Inventory.DiscoveredRules;
            List<CraftingRule> newRules = new List<CraftingRule>(craftingRules);

            List<string> itemList = new List<string>();
            foreach(Item item in player.Inventory.Items) {
                // Stack size
                string stackSize = item.StackDescription;
                if ( stackSize != "" ) stackSize = " " + stackSize;

                // Shortcut
                string shortCut = item.QuickAccessIndex < 0 ? "" : "[" + ((item.QuickAccessIndex + 1) % 10) + "] ";

                // Crafting
                bool craftable = false;
                bool dismantleable = false;
                foreach (var rule in craftingRules) 
                {
                    if ( rule.CouldBeCraftable(item) )
                    {
                        dismantleable = rule.CouldBeDismantled(item);
                        craftable = rule.IsCraftable(item, player.Inventory.Items);
                        newRules.Remove(rule);
                        break;
                    }
                }

                // Options
                string options = "";
                if ( craftable ) options += ", Craft"; // craftable
                if ( dismantleable ) options += ", Dismantle"; // dismantleable
                options += ", Drop"; // droppable
                if ( options.StartsWith(", ") ) options = " [" + options.Substring(2) + "]";

                // Selection.
                string selectStr = "";
                if ( item == player.Inventory.Selection )
                    selectStr = ">>> ";

                // Actual name assembly
                itemList.Add(selectStr + shortCut + item.Name + stackSize + " (" + item.Description + ")" + options);
            }

            // Also display items that could be crafted or are discovered
            foreach (var rule in newRules)
            {
                Item item = rule.Result;

                // Stack size
                string stackSize = item.StackDescription;
                if ( stackSize != "" ) stackSize = " " + stackSize;

                // Options
                string options = "";
                if ( rule.IsCraftable(null, player.Inventory.Items) ) options += ", Craft"; // craftable
                if ( options.StartsWith(", ") ) options = " [" + options.Substring(2) + "]";

                // Actual name assembly
                itemList.Add("~?~ " + item.Name + stackSize + " (" + item.Description + ")" + options);
            }
            
            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);

            json.Serialize(jsonWriter, itemList);
            response.AppendBody(writer.GetStringBuilder().ToString());

        }

    }
}

