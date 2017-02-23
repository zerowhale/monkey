using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.State.Interrupts
{
    public class DiscardInterrupt: InterruptState
    {
        public DiscardInterrupt(PandemicPlayer player, int count)
        {
            Player = player;
            Count = count;
        }


        /// <summary>
        /// Event owning player's name. 
        /// For the client.
        /// </summary>
        [JsonProperty(PropertyName = "Player")]
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

        public int Count
        {
            get;
            private set;
        }

    }
}