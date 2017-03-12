using Monkey.Games.Agricola.Cards.Prerequisites;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Cards
{
    /// <summary>
    /// Cards added in the full version of the game.
    /// </summary>
    public abstract class FullCard : Card
    {

        public FullCard(XElement definition)
            : base(definition)
        {
            Prerequisites = definition.Elements("Prerequisite").Select(Prerequisite.Create).ToArray();
            Deck = (Deck)Enum.Parse(typeof(Deck), (string)definition.Attribute("Deck"));
            CacheExchanges = definition.Grandchildren("TakeCacheExchange", "CacheExchange").Select(CacheExchange.Create).ToArray();

        }

        public bool ShouldSerializePrerequisites()
        {
            return Prerequisites.Length > 0;
        }


        public bool ShouldSerializeCacheExchanges()
        {
            return cacheExchanges.Length > 0;
        }



        /// <summary>
        /// Checks if all prerequisites of the card are met
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool PrerequisitesMet(AgricolaPlayer player)
        {
            return !Prerequisites.Any(x => !x.IsMet(player));
        }

        /// <summary>
        /// The deck this card belongs to (Basic, Intermediate, Expert)
        /// </summary>
        [JsonIgnore]
        public Deck Deck
        {
            get;
            private set;
        }


        /// <summary>
        /// The prerequisites to play this card
        /// </summary>
        public Prerequisite[] Prerequisites
        {
            get;
            private set;
        }


        /// <summary>
        /// Cache exchanges that are available
        /// </summary>
        public CacheExchange[] CacheExchanges
        {
            get
            {
                return cacheExchanges;
            }
            set
            {
                cacheExchanges = value;
                if (cacheExchanges != null)
                {
                    foreach (var cacheExchange in cacheExchanges)
                        cacheExchange.Id = Id;
                }
            }
        }


        private CacheExchange[] cacheExchanges;


    }
}