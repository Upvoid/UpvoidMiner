using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpvoidMiner
{
    public class RecipeItem : Item
    {
        public readonly Item Result;
        public readonly List<Item> IngredientItems;
        public readonly bool CarryOverSubstance;


        public RecipeItem(Item result, List<Item> ingredientItems, bool carryOverSubstance = true) : 
            base(result.Name + "Recipe", "A recipe for crafting " + result.Name, 0.0f, ItemCategory.Recipes)
        {
            Result = result;
            IngredientItems = ingredientItems;
            CarryOverSubstance = carryOverSubstance;
        }

        public override bool IsEmpty
        {
            get { return empty; }
        }

        private bool empty = true;

        public override bool TryMerge(Item rhs, bool substract, bool force, bool dryrun = false)
        {
            var item = rhs as RecipeItem;
            if (item == null) return false;
            if (!item.Result.TryMerge(Result,false,false,true)) return false;
            empty = substract;

            return true;
        }

        public override Item Clone()
        {
            return new RecipeItem(Result,IngredientItems);
        }

        public override Item Clone(Substance sub)
        {
            return new RecipeItem(Result, IngredientItems);
        }

        public override string Icon
        {
            get { return Result.Icon + ",Recipe"; }
        }
    }
}
