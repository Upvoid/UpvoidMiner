using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpvoidMiner
{
    public class LooseSubstance : Substance
    {
        protected LooseSubstance(string name) : base(name)
        {
        }

        public LooseSubstance() : base("Loose")
        {
        }
    }

    public sealed class DirtSubstance : LooseSubstance
    {
        public DirtSubstance() : base("Dirt")
        {
        }
    }

    public sealed class SandSubstance : LooseSubstance
    {
        public SandSubstance()
            : base("Sand")
        {
        }
    }
}
