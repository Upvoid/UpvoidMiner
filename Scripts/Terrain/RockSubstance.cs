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

#region Stone

    public class StoneSubstance : RockSubstance
    {
        protected StoneSubstance(string name, float massDensity) : base(name, massDensity)
        {
        }

        public StoneSubstance()
            : base("Stone", -1f)
        {
        }
    }

    public sealed class Stone01Substance : StoneSubstance
    {
        public Stone01Substance()
            : base("Stone.01", 2700f)
        {
        }
    }

    public sealed class Stone02Substance : StoneSubstance
    {
        public Stone02Substance()
            : base("Stone.02", 2700f)
        {
        }
    }

    public sealed class Stone03Substance : StoneSubstance
    {
        public Stone03Substance()
            : base("Stone.03", 2700f)
        {
        }
    }

    public sealed class Stone04Substance : StoneSubstance
    {
        public Stone04Substance()
            : base("Stone.04", 2700f)
        {
        }
    }

    public sealed class Stone05Substance : StoneSubstance
    {
        public Stone05Substance()
            : base("Stone.05", 2700f)
        {
        }
    }

    public sealed class Stone06Substance : StoneSubstance
    {
        public Stone06Substance()
            : base("Stone.06", 2700f)
        {
        }
    }

    public sealed class Stone07Substance : StoneSubstance
    {
        public Stone07Substance()
            : base("Stone.07", 2700f)
        {
        }
    }

    public sealed class Stone08Substance : StoneSubstance
    {
        public Stone08Substance()
            : base("Stone.08", 2700f)
        {
        }
    }

    public sealed class Stone09Substance : StoneSubstance
    {
        public Stone09Substance()
            : base("Stone.09", 2700f)
        {
        }
    }

    public sealed class Stone10Substance : StoneSubstance
    {
        public Stone10Substance()
            : base("Stone.10", 2700f)
        {
        }
    }

    public sealed class Stone11Substance : StoneSubstance
    {
        public Stone11Substance()
            : base("Stone.11", 2700f)
        {
        }
    }

    public sealed class Stone12Substance : StoneSubstance
    {
        public Stone12Substance()
            : base("Stone.12", 2700f)
        {
        }
    }

    public sealed class Stone13Substance : StoneSubstance
    {
        public Stone13Substance()
            : base("Stone.13", 2700f)
        {
        }
    }

    public sealed class Stone14Substance : StoneSubstance
    {
        public Stone14Substance()
            : base("Stone.14", 2700f)
        {
        }
    }
#endregion

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
