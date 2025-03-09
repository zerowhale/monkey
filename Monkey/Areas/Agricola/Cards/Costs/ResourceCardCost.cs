using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Costs
{
    /// <summary>
    /// Represents a card cost that can be paid with resources.
    /// </summary>
    public class ResourceCardCost: CardCost
    {
        public ResourceCardCost(IEnumerable<ResourceCache> resources) { 
            Resources = resources.ToImmutableArray();
        }

        public ResourceCardCost(ImmutableArray<ResourceCache> resources)
        {
            Resources = resources;
        }

        public ResourceCardCost(XElement definition){
            var result = from x in definition.Descendants("Cost")
                            select new ResourceCache((Resource)Enum.Parse(typeof(Resource), (string)x.Attribute("Resource")), (int)x.Attribute("Amount"));

            Resources = result.ToImmutableArray<ResourceCache>();
        }

        public ImmutableArray<ResourceCache> Resources { get; }
    }
}