using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards
{
    public class Improvement : Card
    {
        public Improvement(XElement definition, string jsonType)
            : base(definition, jsonType)
        {
            Points = definition.Attribute("Points") != null ? (int)definition.Attribute("Points") : 0;
            Oven = definition.Attribute("Oven") != null ? (bool)definition.Attribute("Oven") : false;
            Fireplace = definition.Attribute("Fireplace") != null ? (bool)definition.Attribute("Fireplace") : false;
            CookingHearth = definition.Attribute("CookingHearth") != null ? (bool)definition.Attribute("CookingHearth") : false;
        }
        /// <summary>
        /// Victory points for owning this card
        /// </summary>
        public readonly int Points;

        /// <summary>
        /// If the improvement counts as a Cooking Hearth
        /// </summary>
        public readonly bool CookingHearth;

        /// <summary>
        /// If the improvement counts as a Fireplace
        /// </summary>
        public readonly bool Fireplace;

        /// <summary>
        /// If the improvement counts as an Oven
        /// </summary>
        public readonly bool Oven;

        /// <summary>
        /// If the improvement can be used during a Bake Action
        /// </summary>
        public readonly bool Bakes;

    }
}