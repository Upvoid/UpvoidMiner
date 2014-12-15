using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpvoidMiner
{
    public class LooseSubstance : Substance
    {
        protected LooseSubstance(string name, float massDensity) : base(name, massDensity)
        {
        }

        public LooseSubstance() : base("Loose", -1f)
        {
        }
    }

    public sealed class DirtSubstance : LooseSubstance
    {
        public DirtSubstance() : base("Dirt", 830f)
        {
        }
    }

    public sealed class SandSubstance : LooseSubstance
    {
        public SandSubstance()
            : base("Sand", 1320f)
        {
        }
    }
}
