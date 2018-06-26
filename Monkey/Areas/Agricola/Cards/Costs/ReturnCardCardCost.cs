using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.Costs
{
    /// <summary>
    /// Represents a cost that may be paid by returning another card.
    /// </summary>
    public class ReturnCardCardCost: CardCost
    {
        public ReturnCardCardCost(XElement definition)
        {
            Text = (string)definition.Attribute("Text");
            Ids = ((string)definition.Attribute("Ids")).Split(',').Select(Int32.Parse).ToImmutableArray<int>();
        }

        /// <summary>
        /// Ids of the cards that may be returned to pay this cost.
        /// </summary>
        public ImmutableArray<int> Ids { get; }

        /// <summary>
        /// Descriptive text of the alternate costs.
        /// </summary>
        public string Text { get; }

    }
}