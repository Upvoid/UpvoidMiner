using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EfficientUI;
using Engine.Resources;
using Engine.Scripting;

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
                get { return Ingredient.StackDescription.Replace("³","&sup3;"); }
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

        [UIButton]
        public void BtnCraft()
        {
            if (oldSelectedItem == null)
                return;

            if (!CanCraft)
                return;

            foreach (var ingredient in IngredientItems)
                LocalScript.player.Inventory.Items.RemoveItem(ingredient.Ingredient,true);

            var recipeItem = SelectedItem as RecipeItem;
            if (recipeItem != null)
                LocalScript.player.Inventory.Items.AddItem(recipeItem.Result.Clone());
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
        public bool CanCraft
        {
            get
            {
                return IngredientItems != null && IngredientItems.All(ingredient => ingredient.IsAvailable);
            }
        }
    }
}
