function updateGui(data)
{
    buildInventory(data.inventory, data.quickAccess, data.selection);
    buildQuickAccessBar(data.quickAccess, data.selection);
}

function setupGui()
{
    $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/IngameGuiData", "", updateGui, "json");

    WebsocketHandler.register("/Mods/Upvoid/UpvoidMiner/0.0.1/InventoryUpdate", function(data) {

        if(data == "Update")
            $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/IngameGuiData", "", updateGui, "json");
        else if(data == "ToggleInventory")
            $("#inventory").toggle();
    });
}

function formatQuantity(quantity, isVolumetric)
{
    if(!isVolumetric)
        return "x"+Math.round(quantity);
    else
        return (Math.round(quantity*10, -1)/10)+"m³";
}

function formatItem(item, active, quickAccessSlot)
{
    var html = "";
    
    if(item === null)
    {
        if(quickAccessSlot < 0)
            html += "<li class=\"item empty\"></li>";
        else
            html += "<li class=\"item empty\"><div class=\"slot-number\">"+(quickAccessSlot+1)%10+"</div></li>";
    }
    else
    {
        if(quickAccessSlot < 0)
            html += "<li class=\"item\" id=\"item-"+item.id+"\"><a href=\"javascript:selectItem("+item.id+")\">";
        else
        {
            if(active)
                html += "<li class=\"item active\" id=\"quick-access-slot-"+quickAccessSlot+"\"><a href=\"javascript:selectQuickAccessSlot("+quickAccessSlot+")\">";
            else
                html += "<li class=\"item\" id=\"quick-access-slot-"+quickAccessSlot+"\"><a href=\"javascript:selectQuickAccessSlot("+quickAccessSlot+")\">";
        }
        
        icons = item.icon.split(",");
        
        for(var j = 0; j < icons.length; j++)
        {
            html += "<img src=\"/Resource/Texture/Items/Icons/"+icons[j]+"\" alt=\""+item.name+"\" title=\""+item.name+"\">"
        }
        
        if(!(quickAccessSlot < 0))
            html += "<div class=\"slot-number\">"+(quickAccessSlot+1)%10+"</div>";

        if(item.quantity != 1 || item.isVolumetric)
        {
            html += "<div class=\"quantity\">"+formatQuantity(item.quantity, item.isVolumetric)+"</div>";
        }
        
        html += "</a></li>";
    }
    
    return html;
}

function findQuickAccessSlot(item, quickAccessItems)
{
    for(var i = 0; i < quickAccessItems.length; i++)
    {
        if(quickAccessItems[i] === null)
            continue;

        if(quickAccessItems[i].id == item.id)
            return i;
    }

    return -1;
}

function buildInventory(items, quickAccessItems, selection)
{
    var html = "";
    for(var i = 0; i < items.length; ++i)
    {
        var item = items[i];
        var quickAccessSlot = findQuickAccessSlot(item, quickAccessItems);
        html += formatItem(item, quickAccessSlot == selection, quickAccessSlot);
    }
    
    $('#inventory-list').html(html);
}

function buildQuickAccessBar(quickAccessItems, selection)
{   
    var html = "";
    
    for(var i = 0; i < quickAccessItems.length; ++i)
    {
        html += formatItem(quickAccessItems[i], i == selection, i);
    }
    
    $('#quickaccess-list').html(html);
}

function selectQuickAccessSlot(index)
{
    $(".item.active").removeClass("active");
    $("#quick-access-slot-"+index).addClass("active");
    $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/SelectQuickAccessSlot", {"selectedIndex": index});
}

function selectItem(itemId)
{
    $(".item.active").removeClass("active");
    $("#item-"+itemId).addClass("active");
    $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/SelectItem", {"selectedItem": itemId});
}