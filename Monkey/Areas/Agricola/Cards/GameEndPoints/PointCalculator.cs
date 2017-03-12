using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards.GameEndPoints
{
    public abstract class PointCalculator
    {
        public PointCalculator(XElement definition)
        {
            AllPlayers = definition.Attribute("AllPlayers") != null ? (bool)definition.Attribute("AllPlayers") : false;
            Title = (string)definition.Attribute("Title");

        }

        public abstract int GetPoints(AgricolaPlayer player, out string title);

        public static PointCalculator Create(XElement definition)
        {
            var cls = (string)definition.Attribute("Class");
            var type = Type.GetType(cls);

            PointCalculator calculator = (PointCalculator)Activator.CreateInstance(type, definition);
            return calculator;
        }
        public Card OwningCard
        {
            get;
            set;
        }

        public bool AllPlayers
        {
            get;
            private set;
        }

        public string Title
        {
            get;
            private set;
        }
    }
}