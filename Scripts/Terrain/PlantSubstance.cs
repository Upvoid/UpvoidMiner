using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpvoidMiner
{
    public class PlantSubstance : Substance
    {
        protected PlantSubstance(string name) : base(name)
        {
        }

        public PlantSubstance() : base("Plant")
        {
        }
    }

    public class WoodSubstance : PlantSubstance
    {
        protected WoodSubstance(string name) : base(name)
        {
        }

        public WoodSubstance() : base("Wood")
        {
        }
    }

    public sealed class BirchWoodSubstance : WoodSubstance
    {
        public BirchWoodSubstance() : base("BirchWood")
        {
        }
    }
}
