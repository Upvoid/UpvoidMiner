using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EfficientUI;

namespace UpvoidMiner
{
    public class InventoryUI : UIProxy
    {
        public InventoryUI()
            : base("Inventory")
        {
            UIProxyManager.AddProxy(this);
        }

        [UIObject]
        public bool IsInventoryOpen
        {
            get { return LocalScript.player != null && LocalScript.player.Gui != null && LocalScript.player.Gui.IsInventoryOpen; }
        }
        [UIObject]
        public bool IsUIOpen
        {
            get { return LocalScript.player != null && LocalScript.player.Gui != null && LocalScript.player.Gui.IsUIOpen; }
        }
    }
}
