using Monkey.Games.Pandemic.Cards;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.State.Interrupts
{
    public class EventCardInterrupt: InterruptState
    {
        public EventCardInterrupt(PandemicPlayer player, EventCardType type)
        {
            Player = player;
            CardId = type;
        }

        /// <summary>
        /// Event owning player's name. 
        /// For the client.
        /// </summary>
        [JsonProperty(PropertyName="Player")]
        public string PlayerName
        {
            get
            {
                return Player.Name;
            }
        }

        /// <summary>
        /// The player that owns the event card.
        /// </summary>
        [JsonIgnore]
        public PandemicPlayer Player
        {
            get;
            private set;
        }

        public EventCardType CardId
        {
            get;
            private set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public EventCardData Data
        {
            get;
            set;
        }
    }
}