using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.State.Interrupts
{
    /// <summary>
    /// This interrupt is used when the maximum number of research stations 
    /// already exist on the map, and the player attempting to build a new one
    /// must be prompted to select an old one for removal.
    /// </summary>
    public class MoveResearchStationInterrupt : InterruptState
    {
        
        /// <summary>
        /// Create a new Move Research Station Interrupt.
        /// </summary>
        /// <param name="player">The player building the new research station.</param>
        /// <param name="newLocation">The city where the new research station will be built.</param>
        public MoveResearchStationInterrupt(PandemicPlayer player, City newLocation, bool debitAction = false)
        {
            Player = player;
            NewLocation = newLocation;
            DebitAction = debitAction;
        }

        /// <summary>
        /// The player building the new research station.
        /// </summary>
        [JsonIgnore]
        public PandemicPlayer Player
        {
            get;
            set;
        }

        /// <summary>
        /// The player name.
        /// Used by the client.
        /// </summary>
        [JsonProperty(PropertyName="Player")]
        public string PlayerName
        {
            get { return Player.Name; }
        }

        /// <summary>
        /// The location the research station will be built at.
        /// </summary>
        public City NewLocation
        {
            get;
            set;
        }

        public bool DebitAction
        {
            get;
            set;
        }

    }
}