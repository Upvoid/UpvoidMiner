using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpvoidMiner
{
    public class RockSubstance : Substance
    {
        protected RockSubstance(string name) : base(name)
        {
        }

        public RockSubstance()
            : base("Rock")
        {
        }
    }

#region Stone

    public class StoneSubstance : RockSubstance
    {
        protected StoneSubstance(string name) : base(name)
        {
        }

        public StoneSubstance()
            : base("Stone")
        {
        }
    }

    public sealed class Stone01Substance : StoneSubstance
    {
        public Stone01Substance()
            : base("Stone.01")
        {
        }
    }

    public sealed class Stone02Substance : StoneSubstance
    {
        public Stone02Substance()
            : base("Stone.02")
        {
        }
    }

    public sealed class Stone03Substance : StoneSubstance
    {
        public Stone03Substance()
            : base("Stone.03")
        {
        }
    }

    public sealed class Stone04Substance : StoneSubstance
    {
        public Stone04Substance()
            : base("Stone.04")
        {
        }
    }

    public sealed class Stone05Substance : StoneSubstance
    {
        public Stone05Substance()
            : base("Stone.05")
        {
        }
    }

    public sealed class Stone06Substance : StoneSubstance
    {
        public Stone06Substance()
            : base("Stone.06")
        {
        }
    }

    public sealed class Stone07Substance : StoneSubstance
    {
        public Stone07Substance()
            : base("Stone.07")
        {
        }
    }

    public sealed class Stone08Substance : StoneSubstance
    {
        public Stone08Substance()
            : base("Stone.08")
        {
        }
    }

    public sealed class Stone09Substance : StoneSubstance
    {
        public Stone09Substance()
            : base("Stone.09")
        {
        }
    }

    public sealed class Stone10Substance : StoneSubstance
    {
        public Stone10Substance()
            : base("Stone.10")
        {
        }
    }

    public sealed class Stone11Substance : StoneSubstance
    {
        public Stone11Substance()
            : base("Stone.11")
        {
        }
    }

    public sealed class Stone12Substance : StoneSubstance
    {
        public Stone12Substance()
            : base("Stone.12")
        {
        }
    }

    public sealed class Stone13Substance : StoneSubstance
    {
        public Stone13Substance()
            : base("Stone.13")
        {
        }
    }

    public sealed class Stone14Substance : StoneSubstance
    {
        public Stone14Substance()
            : base("Stone.14")
        {
        }
    }
#endregion

    public sealed class CoalSubstance : RockSubstance
    {
        public CoalSubstance()
            : base("Coal")
        {
        }
    }

    public sealed class FireRockSubstance : RockSubstance
    {
        public FireRockSubstance()
            : base("FireRock")
        {
        }
    }

    public class OreSubstance : RockSubstance
    {
        protected OreSubstance(string name) : base(name)
        {
        }
        public OreSubstance()
            : base("Ore")
        {
        }
    }

    public sealed class CopperOreSubstance : OreSubstance
    {
        public CopperOreSubstance()
            : base("CopperOre")
        {
        }
    }

    public sealed class GoldOreSubstance : OreSubstance
    {
        public GoldOreSubstance()
            : base("GoldOre")
        {
        }
    }

    public sealed class VerdaniumOreSubstance : OreSubstance
    {
        public VerdaniumOreSubstance()
            : base("VerdaniumOre")
        {
        }
    }
    public class CrystalSubstance : RockSubstance
    {
        protected CrystalSubstance(string name)
            : base(name)
        {
        }
        public CrystalSubstance()
            : base("Crystal")
        {
        }
    }

    public sealed class AegiriumSubstance : CrystalSubstance
    {
        public AegiriumSubstance()
            : base("Aegirium")
        {
        }
    }
}
