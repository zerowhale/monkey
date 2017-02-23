using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards
{
    public class MajorImprovement: Card, IImprovement
    {
        public MajorImprovement(XElement definition)
            : base(definition)
        {
            Points = definition.Attribute("Points") != null ? (int)definition.Attribute("Points") : 0;
            Oven = definition.Attribute("Oven") != null ? (bool)definition.Attribute("Oven") : false;
            Fireplace = definition.Attribute("Fireplace") != null ? (bool)definition.Attribute("Fireplace") : false;
            Fireplace = definition.Attribute("Fireplace") != null ? (bool)definition.Attribute("Fireplace") : false;
            CookingHearth = definition.Attribute("CookingHearth") != null ? (bool)definition.Attribute("CookingHearth") : false;
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Points
        {
            get;
            set;
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Fireplace
        {
            get;
            set;
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Oven
        {
            get;
            set;
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool CookingHearth
        {
            get;
            set;
        }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Bakes
        {
            get;
            set;

        }

        [JsonProperty(PropertyName = "Type")]
        public override string JsonType
        {
            get { return "Major"; }
        }
    }
}