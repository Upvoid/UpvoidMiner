using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpvoidMiner
{
    public class Substance
    {
        public string Name { get; private set; }

        protected Substance(string name)
        {
            Name = name;
        }
    }
}
