using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpvoidMiner
{
    public class RockSubstance : Substance
    {
        protected RockSubstance(string name, float massDensity) : base(name, massDensity)
        {
        }

        public RockSubstance()
            : base("Rock", -1f)
        {
        }
    }
    
    public class StoneSubstance : RockSubstance
    {
        protected StoneSubstance(string name, float massDensity) : base(name, massDensity)
        {
        }

        public StoneSubstance()
            : base("Stone", -1f)
        {
        }

        public override string MatOverlayName()
        {
            return "StoneMat";
        }
    }

    public sealed class BasaltSubstance : StoneSubstance
    {
        public BasaltSubstance()
            : base("Basalt", 2900f)
        {
        }
    }

    public sealed class CoalSubstance : RockSubstance
    {
        public CoalSubstance()
            : base("Coal", 1300f)
        {
        }
    }

    public sealed class FireRockSubstance : RockSubstance
    {
        public FireRockSubstance()
            : base("FireRock", 2900f)
        {
        }
    }

    public class OreSubstance : RockSubstance
    {
        protected OreSubstance(string name, float massDensity) : base(name, massDensity)
        {
        }
        public OreSubstance()
            : base("Ore", -1f)
        {
        }
    }

    public sealed class CopperOreSubstance : OreSubstance
    {
        public CopperOreSubstance()
            : base("CopperOre", 3400f)
        {
        }
    }

    public sealed class GoldOreSubstance : OreSubstance
    {
        public GoldOreSubstance()
            : base("GoldOre", 4000f)
        {
        }
    }

    public sealed class VerdaniumOreSubstance : OreSubstance
    {
        public VerdaniumOreSubstance()
            : base("VerdaniumOre", 12000f)
        {
        }
    }
    public class CrystalSubstance : RockSubstance
    {
        protected CrystalSubstance(string name, float massDensity)
            : base(name, massDensity)
        {
        }
        public CrystalSubstance()
            : base("Crystal", -1f)
        {
        }
    }

    public sealed class AegiriumSubstance : CrystalSubstance
    {
        public AegiriumSubstance()
            : base("Aegirium", 3500f)
        {
        }
    }
}
