using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards
{
    public class Occupation : Card
    {
        public Occupation(XElement definition)
            : base(definition, "Occupation")
        {
            MinPlayers = (int)definition.Attribute("MinPlayers");
        }

        public readonly int MinPlayers;
    }
}