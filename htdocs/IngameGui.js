
var inventoryItemsPerRow = 6;
var virtualItemSelection = null;
var playerItems = [];

function findItemById(itemId)
{
    for(var identifier in playerItems)
    {
        if(playerItems[identifier].id === itemId)
        {
            return playerItems[identifier];
        }
    }
    return null;
}

function updateGui(data)
{
    buildInventory(data.inventory, data.quickAccess, data.selection);

    if(data.playerIsFrozen)
    {
        $('#generating-world-notice').show();
        $('#screen-overlay').show();
    }
    else
    {
        $('#generating-world-notice').hide();
        $('#screen-overlay').hide();
    }

    playerItems = data.inventory;

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
        else if(data == "ToggleUI")
        {
        	$("#ui-body").toggle();
        }
    });

    WebsocketHandler.register("/Mods/Upvoid/UpvoidMiner/0.0.1/ResourceDownloadProgress", handleResourceDownloadProgress);
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
                html += "<a href=\"javascript:craftItem('"+item.identifier+"')\"><span class='fa-stack'><i class='fa fa-square fa-stack-2x icon-green'></i><i class='fa fa-stack-1x fa-plus'></i></span></a>";
            else if(item.hasDiscoveredCraftingRule)
                html += "<a><span class='fa-stack'><i class='fa fa-square fa-stack-2x icon-disabled'></i><i class='fa fa-stack-1x fa-plus'></i></span></a>";
            if(item.canBeDismantled)
                html += "<a href=\"javascript:dismantleItem('"+item.id+"')\"><span class='fa-stack'><i class='fa fa-square fa-stack-2x icon-red'></i><i class='fa fa-stack-1x fa-minus'></i></span></a>";
            if(item.quantity > 0)
                html += "<a href=\"javascript:dropItem('"+item.id+"')\"><span class='fa-stack'><i class='fa fa-square fa-stack-2x icon-purple'></i><i class='fa fa-stack-1x fa-arrow-down'></i></span></a>";
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
        html += "<li class='icon'><i class='fa fa-arrow-left'></i></li>";

        for(var i = 0; i < item.craftingIngredients.length; i++)
        {
            html += formatItem(item.craftingIngredients[i], false, -1);
            if(i < item.craftingIngredients.length - 1)
                html += "<li class='icon'><i class='fa fa-plus'></i></li>";
        }

        html += "</ul><div class='clearfix'></div></div>";
    }

    if(item.canBeCrafted)
        html += " <button onclick=\"craftItem('"+item.identifier+"')\" class=\"btn btn-success\"><i class='fa fa-plus'></i> Craft</button>";
    else if(item.hasDiscoveredCraftingRule)
        html += " <button class=\"btn btn-default disabled\"><i class='fa fa-plus'></i> Craft</button>";
    if(item.canBeDismantled)
        html += " <button onclick=\"dismantleItem('"+item.id+"')\" class=\"btn btn-danger\"><i class='fa fa-minus'></i> Dismantle</button>";
    if(item.quantity > 0)
        html += " <button onclick=\"dropItem('"+item.id+"')\" class=\"btn btn-primary\"><i class='fa fa-arrow-down'></i> Drop</button>";

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
    virtualItemSelection = null;

    item = findItemById(itemId);

    if(item == null)
        return;

    $(".item.active").removeClass("active");
    $("#item-"+itemId).addClass("active");

    // If the item is non-virtual (i.e. the player really has some of it in the inventory),
    // tell the player to actually select this item.
    if(item.quantity > 0)
    {
        $.get("/Mods/Upvoid/UpvoidMiner/0.0.1/SelectItem", {"itemId": itemId});
    }
    else
    {
        //Selecting a virtual item here
        virtualItemSelection = item;
        buildItemInfo(item);
    }
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

var lastKnownDownloadProgress = 1;
function handleResourceDownloadProgress(data)
{
    $('#resource-download-progress-bar').css('width', (data*100.0)+"%");
    //$('#resource-download-progress-bar').css('width', (data*100.0)+"%");

    $('#resource-download-progress').show();
    $('#resource-download-progress').addClass('active');

    if(data >= 1.0)
    {
        lastKnownDownloadProgress = data;
        $('#resource-download-progress progress').removeClass('active');
        $('#resource-download-progress progress').removeClass('progress-striped');

        window.setTimeout(function() {
            if(lastKnownDownloadProgress >= 1.0)
            {
                //$('#resource-download-progress').hide();
                $('#resource-download-progress').removeClass('active');
            }
        }, 2000);
    }
}
