using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Costs
{
    public class CardCost
    {

        public static CardCost Create(XElement definition)
        {

            var cls = (string)definition.Attribute("Class");
            var type = cls == null ? typeof(FreeCardCost) : Type.GetType(cls);

            CardCost cost = (CardCost)Activator.CreateInstance(type, definition);
            return cost;
        }

        [JsonProperty(PropertyName = "Type")]
        public string JsonType
        {
            get { return this.GetType().Name; }
        }

    }

}