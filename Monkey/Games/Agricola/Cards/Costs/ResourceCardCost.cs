using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Costs
{
    public class ResourceCardCost: CardCost
    {
        public ResourceCardCost(XElement definition){
            var result = from x in definition.Descendants("Cost")
                            select new ResourceCache((Resource)Enum.Parse(typeof(Resource), (string)x.Attribute("Resource")), (int)x.Attribute("Amount"));

            Resources = result.ToArray();
        }

        public ResourceCache[] Resources
        {
            get;
            private set;
        }
    }
}