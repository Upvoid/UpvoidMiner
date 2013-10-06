using System;
using System.Collections.Generic;

namespace UpvoidMiner
{
    /// <summary>
    /// An explicit crafting rule, i.e. explicit result, ingredients, and dismantle results.
    /// </summary>
    public class ExplicitCraftingRule : CraftingRule
    {
        public ExplicitCraftingRule(Item result, IEnumerable<Item> ingredients, IEnumerable<Item> dismantleResults = null)
        {
            Result = result;
            Ingredients = ingredients;
            DismantleResult = dismantleResults;
        }
    }
}

