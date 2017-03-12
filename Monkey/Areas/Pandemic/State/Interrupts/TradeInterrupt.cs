using Monkey.Games.Pandemic.Cards;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.State.Interrupts
{
    /// <summary>
    /// This class represents a proposed trade between 2 players for a city card 
    /// in one of the two players hands.
    /// </summary>
    public class TradeInterrupt: InterruptState
    {
        public TradeInterrupt(PandemicPlayer player1, PandemicPlayer player2, CityCard card)
        {
            Player1 = player1;
            Player2 = player2;
            Card = card;
        }

        /// <summary>
        /// The name of the first player in the trade, for the client.
        /// </summary>
        [JsonProperty(PropertyName = "Player1")]
        public string Player1Name
        {
            get
            {
                return Player1.Name;
            }
        }

        /// <summary>
        /// The name of the second player in the trade, for the client.
        /// </summary>
        [JsonProperty(PropertyName = "Player2")]
        public string Player2Name
        {
            get
            {
                return Player2.Name;
            }
        }

        /// <summary>
        /// The ID of the city card in the trade, for the client.
        /// </summary>
        [JsonProperty(PropertyName = "Card")]
        public int CardId
        {
            get 
            { 
                return Card.Id; 
            }
        }

        /// <summary>
        /// One player in the trade.
        /// </summary>
        [JsonIgnore]
        public PandemicPlayer Player1
        {
            get;
            private set;
        }

        /// <summary>
        /// The other player in the trade.
        /// </summary>
        [JsonIgnore]
        public PandemicPlayer Player2
        {
            get;
            private set;
        }

        /// <summary>
        /// The city card being traded
        /// </summary>
        [JsonIgnore]
        public CityCard Card
        {
            get;
            private set;
        }

        /// <summary>
        /// If player 1 has confirmed the trade.
        /// </summary>
        public bool Player1Confirmed
        {
            get;
            set;
        }

        /// <summary>
        /// If player 2 has confirmed the trade.
        /// </summary>
        public bool Player2Confirmed
        {
            get;
            set;
        }

    }
}