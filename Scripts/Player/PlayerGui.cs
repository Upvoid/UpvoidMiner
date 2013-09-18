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

            List<string> itemList = new List<string>();
            foreach(Item item in player.inventory) {
                itemList.Add(item.Name);
            }

            
            StringWriter writer = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(writer);

            json.Serialize(jsonWriter, itemList);
            response.AppendBody(writer.GetStringBuilder().ToString());

        }

    }
}

