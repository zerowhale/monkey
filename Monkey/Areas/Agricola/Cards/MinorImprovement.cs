using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards
{
    /// <summary>
    /// Representation of cards in the Minor Improvement decks
    /// </summary>
    public class MinorImprovement : Card
    {

        public MinorImprovement(XElement definition)
            : base(definition, "Minor")
        {
            PassesLeft = definition.Attribute("Passes") != null ? (bool)definition.Attribute("Passes") : false;
            Plow = definition.Elements("Plow").Select(Plow.Create).FirstOrDefault();
        }

        /// <summary>
        /// If the card counts as a Plow
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public readonly Plow Plow;

        /// <summary>
        /// If the card is passed to the player to the left after being used
        /// </summary>
        [JsonIgnore]
        public readonly bool PassesLeft;

    }
}