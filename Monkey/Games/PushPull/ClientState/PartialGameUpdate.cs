using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.PushPull.ClientState
{
    /// <summary>
    /// Conveys partial update information to the client, used to update the client-side
    /// data objects.  Usage of this assumes a full game update was previously sent to the client
    /// </summary>
    public class PartialGameUpdate: IGameUpdate
    {
        public PartialGameUpdate()
        {
        }

        /// <summary>
        /// Adds a player update object to the list of players being updated.
        /// </summary>
        /// <param name="player"></param>
        public void AddPlayer(PartialPlayerUpdate player)
        {
            if (Players == null)
                Players = new List<PartialPlayerUpdate>();
            Players.Add(player);
        }

        /// <summary>
        /// Player data to update on the client.
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<PartialPlayerUpdate> Players
        {
            get;
            private set;
        }

        /// <summary>
        /// The player who's turn it is currently
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string ActivePlayer
        {
            get;
            set;
        }

        /// <summary>
        /// The active players remaining actions
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? ActionsRemaining
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public GameState? State
        {
            get;
            set;
        }


    }
}