using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards
{
    public class Occupation : FullCard
    {
        public Occupation(XElement definition)
            : base(definition)
        {
            MinPlayers = (int)definition.Attribute("MinPlayers");
        }

        public int MinPlayers
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "Type")]
        public override string JsonType
        {
            get { return "Occupation"; }
        }
    }
}