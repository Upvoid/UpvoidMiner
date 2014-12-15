using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.XPath;
using EfficientUI;
using Engine.Resources;
using Engine.Scripting;
using UpvoidMiner.UI;

namespace UpvoidMiner
{
    public class RecipeUI : UIProxy
    {
        public class IconUI : UIProxy
        {
            [UIImage]
            public TextureDataResource Icon { get; set; }

            public IconUI(TextureDataResource icon)
            {
                Icon = icon;
            }
        }

        public class IngredientUI : UIProxy
        {
            [UICollection("ItemIcon")]
            public List<IconUI> Icon { get; set; }

            [UIString]
            public string Name
            {
                get
                {
                    return Ingredient.Name;
                }
            }

            [UIString]
            public string Amount
            {
                get
                {
                    return Ingredient.StackDescription.Replace("³", "&sup3;");
                }
            }

            [UIString]
            public string Dimensions
            {
                get
                {
                    if (Ingredient is MaterialItem)
                        return (Ingredient as MaterialItem).DimensionString;
                    return "";
                }
            }

            [UIObject]
            public bool HasDimensions
            {
                get { return (Ingredient is MaterialItem); }
            }


            public readonly Item Ingredient;

            public IngredientUI(Item ingredient)
            {
                Ingredient = ingredient;
                Icon = new List<IconUI>();
                foreach (var iconlayer in Ingredient.Icon.Split(','))
                    Icon.Add(new IconUI(Resources.UseTextureData("Items/Icons/" + iconlayer, UpvoidMiner.ModDomain)));
            }

            [UIObject]
            public bool IsAvailable
            {
                get
                {
                    return LocalScript.player != null && LocalScript.player.Inventory.Items.Any(item => item.TryMerge(Ingredient, true, false, true));
                }
            }
        }
       
        public RecipeUI()
            : base("Recipe")
        {
            UIProxyManager.AddProxy(this);
            Scripting.RegisterUpdateFunction(OnTick,UpvoidMiner.Mod);
        }

        private void OnTick(float elapsedSeconds)
        {
            if (oldSelectedItem != SelectedItem)
            {
                oldSelectedItem = SelectedItem;
                ResultIconStack = new List<IconUI>();
                var resultIcon = SelectedItem is RecipeItem ? (SelectedItem as RecipeItem).Result.Icon : null;
                if (resultIcon != null)
                    foreach (var iconlayer in resultIcon.Split(','))
                        ResultIconStack.Add(new IconUI(Resources.UseTextureData("Items/Icons/" + iconlayer, UpvoidMiner.ModDomain)));

                var ingredients = SelectedItem is RecipeItem ? (SelectedItem as RecipeItem).IngredientItems : null;
                IngredientItems = new List<IngredientUI>();
                if (ingredients != null)
                    foreach (var ingredient in ingredients)
                        IngredientItems.Add(new IngredientUI(ingredient));
            }
        }

        private Item oldSelectedItem;
        [UICollection("IngredientItem")]
        public List<IngredientUI> IngredientItems { get; set;}

        [UICollection("ItemIcon")]
        public List<IconUI> ResultIconStack { get; set;}

        [UIString]
        public string ResultName
        {
            get
            {
                return SelectedItem is RecipeItem ? (SelectedItem as RecipeItem).Result.Name : "";
            }
        }

        public Item SelectedItem
        {
            get
            {
                return LocalScript.player == null ? null : LocalScript.player.Inventory.Selection;
            }
        }

        [UIString]
        public string ResultAmount
        {
            get
            {
                return SelectedItem is RecipeItem ? (SelectedItem as RecipeItem).Result.StackDescription.Replace("³","&sup3;") : "";
            }
        }

        [UIButton]
        public void BtnCraft()
        {
            if (oldSelectedItem == null)
                return;

            if (!CanCraft)
                return;

            Item firstRemoved = null;
            foreach (var ingredient in IngredientItems)
            {
                var rem = LocalScript.player.Inventory.Items.RemoveItem(ingredient.Ingredient, true);
                if (firstRemoved == null)
                    firstRemoved = rem;
            }

            var recipeItem = SelectedItem as RecipeItem;
            if (recipeItem == null)
                return;

            if (recipeItem.CarryOverSubstance)
            {
                var substance = null as Substance;
                if (firstRemoved is ToolItem)
                    substance = (firstRemoved as ToolItem).Substance;
                else if (firstRemoved is CraftingItem)
                    substance = (firstRemoved as CraftingItem).Substance;
                else if (firstRemoved is ResourceItem)
                    substance = (firstRemoved as ResourceItem).Substance;
                else if (firstRemoved is MaterialItem)
                    substance = (firstRemoved as MaterialItem).Substance;

                LocalScript.player.Inventory.Items.AddItem(substance != null
                    ? recipeItem.Result.Clone(substance)
                    : recipeItem.Result.Clone());
            }
            else
                LocalScript.player.Inventory.Items.AddItem(recipeItem.Result.Clone());
            //Tutorial
            if (recipeItem.Result.Name == "Handle")
                Tutorials.MsgBasicRecipeCraftingHandle.Report(4);
            if (recipeItem.Result.Name == "Wood Pickaxe")
                Tutorials.MsgBasicRecipeWoodPickaxe.Report(1);
        }

        [UIObject]
        public bool HasRecipeSettings
        {
            get
            {
                return SelectedItem is RecipeItem;
            }
        }

        [UIObject]
        public string CraftingString
        {
            get
            {
                if (string.IsNullOrEmpty(ResultName))
                    return "";
                var n = ResultName.First() == 'a' || ResultName.First() == 'e' || ResultName.First() == 'i' || ResultName.First() == 'o' || ResultName.First() == 'u';
                return "Craft a" + (n ? "n " : " ") + ResultName;
            }
        }

        [UIObject]
        public bool CanCraft
        {
            get
            {
                return IngredientItems != null && IngredientItems.All(ingredient => ingredient.IsAvailable);
            }
        }
    }
}
