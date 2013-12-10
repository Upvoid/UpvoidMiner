// Copyright (C) by Upvoid Studios
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>

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

