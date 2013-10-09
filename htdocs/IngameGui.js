function updateGui(data)
{
    buildInventory(data.inventory);
    buildQuickAccessBar(data.quickAccess, data.selection);
}

function setupGui()
{
    $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/IngameGuiData", "", updateGui, "json");

    WebsocketHandler.register("/Mods/Upvoid/UpvoidMiner/0.0.1/InventoryUpdate", function(data) {
        if(data == "Update")
            $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/IngameGuiData", "", updateGui, "json");
    });
}

function buildInventory(items)
{
    inventoryList = $('#inventory-list')
    inventoryList.html("");
    
    for(var i = 0; i < items.length; ++i)
    {
        var item = items[i];
        inventoryList.append("<li class=\"item\"><img src=\""+item.icon+"\" alt=\""+item.name+"\"> <span class=\"item-details\">"+item.name+"</span> </li>")
    }
}

function buildQuickAccessBar(quickAccessItems, selection)
{
    var quickaccessList = $('#quickaccess-list');
    quickaccessList.html("");
    
    for(var i = 0; i < quickAccessItems.length; ++i)
    {
        if(quickAccessItems[i] === null)
        {
            quickaccessList.append("<li class=\"item empty\"><div class=\"slot-number\">"+(i+1)%10+"</div></li>");
        }
        else
        {
            var item = quickAccessItems[i];
            
            var itemHtml = "";
            if(i == selection)
                itemHtml += "<li class=\"item active\"><img src=\""+item.icon+"\" alt=\""+item.name+"\" title=\""+item.name+"\">";
            else
                itemHtml += "<li class=\"item\"><img src=\""+item.icon+"\" alt=\""+item.name+"\" title=\""+item.name+"\">";
                
            itemHtml += "<div class=\"slot-number\">"+(i+1)%10+"</div>";
            if(item.quantity != 1 || item.isVolumetric)
            {
                if(item.isVolumetric)
                    itemHtml += "<div class=\"quantity\">"+item.quantity+"m³</div>";
                else
                    itemHtml += "<div class=\"quantity\">x"+item.quantity+"</div>";
            }
            itemHtml += "</li>";
            
            quickaccessList.append(itemHtml);
        }
    }
}
