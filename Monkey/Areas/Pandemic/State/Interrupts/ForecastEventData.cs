using Monkey.Games.Pandemic.Cards;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.State.Interrupts
{
    public class ForecastEventData : EventCardData
    {
        public ForecastEventData(ICard[] cards)
        {
            Cards = cards;
        }

        public bool TryReorder(int[] ids)
        {
            if (!IdsValid(ids))
                return false;

            var cards = new ICard[6];


            for(var i=0;i<ids.Length;i++)
            {
                var id = ids[i];
                var card = Cards.Where(x => x.Id == id).First();
                cards[i] = card;
            }

            Cards = cards;
            return true;
        }

        public bool IdsValid(int[] ids)
        {
            return ids.Intersect(CardIds).Count() == ids.Length;
        }

        [JsonProperty(PropertyName="Cards")]
        public int[] CardIds
        {
            get
            {
                return Cards.Select(x => x.Id).ToArray();
            }
        }

        [JsonIgnore]
        public ICard[] Cards
        {
            get;
            set;
        }
    }
}