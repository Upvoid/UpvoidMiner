
var inventoryItemsPerRow = 6;

function updateGui(data)
{
    buildInventory(data.inventory, data.quickAccess, data.selection);

    var quickAccessItems = [];

    for(var i = 0; i < data.quickAccess.length; i++)
    {
        if(data.quickAccess[i] != "")
            quickAccessItems.push(data.inventory[data.quickAccess[i]]);
        else
            quickAccessItems.push(null);

        if(i === data.selection)
        {
            buildItemInfo(data.inventory[data.quickAccess[i]]);
        }

    }

    buildQuickAccessBar(quickAccessItems, data.selection);
}

function setupGui()
{
    $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/IngameGuiData", "", updateGui, "json");

    WebsocketHandler.register("/Mods/Upvoid/UpvoidMiner/0.0.1/InventoryUpdate", function(data) {

        if(data == "Update")
            $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/IngameGuiData", "", updateGui, "json");
        else if(data == "ToggleInventory")
        {
            $("#inventory").toggle();
            $("#workbench").toggle();
        }
    });
}

function formatQuantity(quantity, isVolumetric)
{
    if(!isVolumetric)
        return "x"+Math.round(quantity);
    else
        return (Math.round(quantity*10, -1)/10)+"m³";
}

function formatItem(item, active, quickAccessSlot, showOptions)
{
    var html = "";
    
    if(item === null)
    {
        if(quickAccessSlot < 0)
            html += "<li class=\"item empty\"><a></a></li>";
        else
            html += "<li class=\"item empty\"><a><div class=\"slot-number\">"+(quickAccessSlot+1)%10+"</div></a></li>";
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
            html += "<img src=\"/Resource/Texture/Items/Icons/"+icons[j]+"\" alt=\""+item.name+"\" title=\""+item.name+"\">";
        }
        
        if(!(quickAccessSlot < 0))
            html += "<div class=\"slot-number\">"+(quickAccessSlot+1)%10+"</div>";

        if(item.quantity != 1 || item.isVolumetric)
        {
            html += "<div class=\"quantity\">"+formatQuantity(item.quantity, item.isVolumetric)+"</div>";
        }

        html += "</a>";

        if(showOptions)
        {
            html += "<div class='item-options'>";
            if(item.canBeCrafted)
                html += "<a href=\"javascript:craftItem('"+item.identifier+"')\"><span class='icon-stack'><i class='icon-sign-blank icon-stack-base icon-green'></i><i class='icon-plus icon-light'></i></span></a>";
            else if(item.hasDiscoveredCraftingRule)
                html += "<a><span class='icon-stack'><i class='icon-sign-blank icon-stack-base icon-disabled'></i><i class='icon-plus icon-light'></i></span></a>";
            if(item.canBeDismantled)
                html += "<a href=\"javascript:dismantleItem('"+item.id+"')\"><span class='icon-stack'><i class='icon-sign-blank icon-stack-base icon-red'></i><i class='icon-minus icon-light'></i></span></a>";
            if(item.quantity > 0)
                html += "<a href=\"javascript:dropItem('"+item.id+"')\"><span class='icon-stack'><i class='icon-sign-blank icon-stack-base icon-purple'></i><i class='icon-arrow-down icon-light'></i></span></a>";
            html += "</div>";
        }
        
        html += "</li>";
    }
    
    return html;
}

function findQuickAccessSlot(item, quickAccessItems)
{
    for(var i = 0; i < quickAccessItems.length; i++)
    {
        if(quickAccessItems[i] === null)
            continue;

        if(quickAccessItems[i] == item.id)
            return i;
    }

    return -1;
}

function buildInventory(items, quickAccessItems, selection)
{
    var html = "";

    var rowCounter = 0;

    for(var identifier in items)
    {

        if(rowCounter%inventoryItemsPerRow == 0)
        {
            html += "<ul class='item-row'>";
        }

        var item = items[identifier];
        html += formatItem(item, item.quickAccessSlot == selection, item.quickAccessSlot, true);

        rowCounter++;

        if(rowCounter%inventoryItemsPerRow == 0)
        {
            html += "</ul><div class='clearfix'></div>";
        }
    }

    if(rowCounter%inventoryItemsPerRow != 0)
    {
        html += "</ul><div class='clearfix'></div>";
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

function buildItemInfo(item)
{
    var html = "";
    html += "<h2>";

    /*
    for(var i = 0; i < item.icons.length; ++i)
    {
        html += "<img class=\"\" src=\"/Resource/Texture/Items/Icons/"+item.icons[j]+"\" alt=\""+item.name+"\" title=\""+item.name+"\">";
    }
    */

    html += " "+item.name+"</h2>";

    if(item.isVolumetric)
        html += "<p>You have "+item.quantity+"m³ of this item type in your inventory.</p>";
    else
        html += "<p>You have "+item.quantity+" pieces of this item type in your inventory.</p>";

    if(item.hasDiscoveredCraftingRule)
    {
        html += "<div class=\"crafting-info\">Can be crafted using the following ingredients:<ul class='item-row'>";

        html += formatItem(item, false, -1);
        html += "<li class='icon'><i class='icon-arrow-left'></i></li>";

        for(var i = 0; i < item.craftingIngredients.length; i++)
        {
            html += formatItem(item.craftingIngredients[i], false, -1);
            if(i < item.craftingIngredients.length - 1)
                html += "<li class='icon'><i class='icon-plus'></i></li>";
        }

        html += "</ul><div class='clearfix'></div></div>";
    }

    if(item.canBeCrafted)
        html += " <button class=\"btn btn-success\"><i class='icon-plus'></i> Craft</button>";
    else if(item.hasDiscoveredCraftingRule)
        html += " <button class=\"btn btn-default disabled\"><i class='icon-plus'></i> Craft</button>";
    if(item.canBeDismantled)
        html += " <button class=\"btn btn-danger\"><i class='icon-minus'></i> Dismantle</button>";
    if(item.quantity > 0)
        html += " <button class=\"btn btn-primary\"><i class='icon-arrow-down'></i> Drop</button>";

    $('#item-info').html(html);

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
    $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/SelectItem", {"itemId": itemId});
}

function dropItem(itemId)
{
    $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/DropItem", {"itemId": itemId});
}

function craftItem(itemIdentifier)
{
    $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/CraftItem", {"itemIdentifier": itemIdentifier});
}

function dismantleItem(itemId)
{
    $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/DismantleItem", {"itemId": itemId});
}