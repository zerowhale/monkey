using Monkey.Games.Agricola.Events.Conditionals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public abstract class PointCalculator
    {
        public PointCalculator(XElement definition, Card owningCard)
        {
            AllPlayers = definition.Attribute("AllPlayers") != null ? (bool)definition.Attribute("AllPlayers") : false;

            Title = definition.Element("Title")?.Value;

            if (string.IsNullOrEmpty(Title))
                Title = (string)definition.Attribute("Title");

            OwningCard = owningCard;
        }


        public static PointCalculator Create(XElement definition, Card owningCard)
        {
            var cls = (string)definition.Attribute("Class");
            var type = Type.GetType(cls);

            PointCalculator calculator = (PointCalculator)Activator.CreateInstance(type, definition, owningCard);
            return calculator;
        }

        public abstract int GetPoints(AgricolaPlayer player, out string title);

        /// <summary>
        /// The card this point calculator belongs to.
        /// </summary>
        public Card OwningCard { get; }

        /// <summary>
        /// If the victory point condition is available to all players.
        /// </summary>
        public bool AllPlayers { get; }

        /// <summary>
        /// The descriptive title of the point calculation.
        /// </summary>
        public string Title { get; }
    }
}