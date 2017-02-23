using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Cards
{
    public class EventCard: ICard
    {
        public EventCard(string name, EventCardType type)
        {
            Name = name;
            CardType = type;
        }

        public int Id
        {
            get { return (int)CardType; }
        }

        public string Name
        {
            get;
            private set;
        }

        public EventCardType CardType
        {
            get;
            private set;
        }
    }
}