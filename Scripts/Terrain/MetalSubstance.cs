using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpvoidMiner
{
    public class MetalSubstance : Substance
    {
        protected MetalSubstance(string name, float massDensity) : base(name, massDensity)
        {
        }

        public MetalSubstance() : base("Metal", -1f)
        {
        }
    }

    public sealed class CopperSubstance : MetalSubstance
    {
        public CopperSubstance() : base("Copper", 8960f)
        {
        }

        public override string MatOverlayName()
        {
            return "CopperMat";
        }
    }

    public sealed class TinSubstance : MetalSubstance
    {
        public TinSubstance()
            : base("Tin", 5769f)
        {
        }
    }

    public sealed class BronzeSubstance : MetalSubstance
    {
        public BronzeSubstance()
            : base("Bronze", 8000f)
        {
        }

        public override string MatOverlayName()
        {
            return "BronzeMat";
        }
    }

    public sealed class IronSubstance : MetalSubstance
    {
        public IronSubstance()
            : base("Iron", 7700f)
        {
        }
    }

    public sealed class SteelSubstance : MetalSubstance
    {
        public SteelSubstance()
            : base("Steel", 8000f)
        {
        }

        public override string MatOverlayName()
        {
            return "SteelMat";
        }
    }

    public sealed class GoldSubstance : MetalSubstance
    {
        public GoldSubstance()
            : base("Gold", 19302f)
        {
        }
    }
}
