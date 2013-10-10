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
    var html = "";
    for(var i = 0; i < items.length; ++i)
    {
        var item = items[i];
        html += "<li class=\"item\"><img src=\""+item.icon+"\" alt=\""+item.name+"\"> <span class=\"item-details\">"+item.name+"</span> </li>";
    }
    
    $('#inventory-list').html("");
}

function buildQuickAccessBar(quickAccessItems, selection)
{   
    var html = "";
    
    for(var i = 0; i < quickAccessItems.length; ++i)
    {
        if(quickAccessItems[i] === null)
        {
            html += "<li class=\"item empty\"><div class=\"slot-number\">"+(i+1)%10+"</div></li>";
        }
        else
        {
            var item = quickAccessItems[i];

            if(i == selection)
                html += "<li class=\"item active\" id=\"quick-access-slot-"+i+"\"><a href=\"javascript:selectQuickAcessSlot("+i+")\"><img src=\""+item.icon+"\" alt=\""+item.name+"\" title=\""+item.name+"\">";
            else
                html += "<li class=\"item\" id=\"quick-access-slot-"+i+"\"><a href=\"javascript:selectQuickAcessSlot("+i+")\"><img src=\""+item.icon+"\" alt=\""+item.name+"\" title=\""+item.name+"\">";
                
            html += "<div class=\"slot-number\">"+(i+1)%10+"</div>";
            if(item.quantity != 1 || item.isVolumetric)
            {
                if(item.isVolumetric)
                    html += "<div class=\"quantity\">"+item.quantity+"m³</div>";
                else
                    html += "<div class=\"quantity\">x"+item.quantity+"</div>";
            }
            html += "</a></li>";
        }
    }
    
    $('#quickaccess-list').html(html);
}

function selectQuickAcessSlot(index)
{
    $(".item.active").removeClass("active");
    $("#quick-access-slot-"+index).addClass("active");
    $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/SelectQuickAccessSlot", {"selectedIndex": index});
}