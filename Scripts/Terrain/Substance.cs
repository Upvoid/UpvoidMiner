using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Engine.Resources;

namespace UpvoidMiner
{
    public class Substance
    {
        public readonly string Name;
        public readonly TextureDataResource Icon;
        public readonly float MassDensity;

        protected Substance(string name, float massDensity)
        {
            Name = name;
            Icon = Resources.UseTextureData("Items/Icons/" + Name, UpvoidMiner.ModDomain);
            MassDensity = massDensity;
        }

        public Substance() : this("Substance", -1f)
        {
        }

        public TerrainResource QueryResource()
        {
            if (!GetType().IsSealed) throw new InvalidOperationException("Non-Leaf Substances do not have any terrain resources assigned to them!");
            var res = TerrainResource.FromName(Name);
            if (res == null)
                throw new InvalidOperationException("Unregistered Substance " + Name + "!");
            return res;
        }

        public string Serialize()
        {
            return GetType().FullName;
        }

        public static Substance Deserialize(string name)
        {
            var constructorInfo = Assembly.GetExecutingAssembly().GetType(name).GetConstructor(new Type[]{});
            if (constructorInfo != null)
                return constructorInfo.Invoke(null) as Substance;
            return null;
        }
    }
}
