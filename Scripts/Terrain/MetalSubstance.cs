using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpvoidMiner
{
    public class MetalSubstance : Substance
    {
        protected MetalSubstance(string name) : base(name)
        {
        }

        public MetalSubstance() : base("Metal")
        {
        }
    }

    public sealed class CopperSubstance : MetalSubstance
    {
        public CopperSubstance() : base("Copper")
        {
        }
    }

    public sealed class IronSubstance : MetalSubstance
    {
        public IronSubstance()
            : base("Iron")
        {
        }
    }

    public sealed class GoldSubstance : MetalSubstance
    {
        public GoldSubstance()
            : base("Gold")
        {
        }
    }
}
