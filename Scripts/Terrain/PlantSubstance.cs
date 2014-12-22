using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpvoidMiner
{
    public class PlantSubstance : Substance
    {
        protected PlantSubstance(string name, float massDensity) : base(name, massDensity)
        {
        }

        public PlantSubstance() : base("Plant", -1f)
        {
        }
    }

    public class WoodSubstance : PlantSubstance
    {
        protected WoodSubstance(string name, float massDensity) : base(name, massDensity)
        {
        }

        public WoodSubstance() : base("Wood", -1f)
        {
        }

        public override string MatOverlayName
        {
            get { return "WoodMat"; }
        }
    }

    public sealed class BirchWoodSubstance : WoodSubstance
    {
        public BirchWoodSubstance() : base("BirchWood", 650f)
        {
        }
    }
}
