using System;

namespace UpvoidMiner
{
    /// <summary>
    /// Abstract interface for a crafting rule.
    /// </summary>
    public abstract class CraftingRule
    {
        /// <summary>
        /// If true, this rule is already discovered
        /// </summary>
        public bool Discovered { get; protected set; }

        public CraftingRule()
        {
            Discovered = false;
        }
    }
}

