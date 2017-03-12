using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BoardgamePlatform.Game;

namespace BoardgamePlatform.Game
{
    public class Player
    {
        public Player(string id, string connectionId)
        {
            ConnectionId = connectionId;
            Id = id;
            Ready = false;
            Color = PlayerColor.Red;
        }
  
        [JsonConverter(typeof(StringEnumConverter))]
        public PlayerColor Color
        {
            get;
            set;
        }

        public String Name
        {
            get;
            set;
        }


        /// <summary>
        /// The Identity user id of the player
        /// </summary>
        public string Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Hub connection Id
        /// </summary>
        [JsonIgnore]
        public string ConnectionId
        {
            get;
            set;
        }

        /// <summary>
        /// Player object for whatever game the user is playing
        /// </summary>
        [JsonIgnore]
        public GamePlayer GamePlayer
        {
            get;
            set;
        }

        [JsonIgnore]
        public IGame<GameHub> Game
        {
            get { return GamePlayer != null ? GamePlayer.Game : null; }
        }

        [JsonIgnore]
        public PreGame GameLobby
        {
            get
            {
                return gameLobby;
            }
            set
            {
                gameLobby = value;
                Ready = false;
            }
        }

        public Boolean Ready
        {
            get;
            set;
        }


        private PreGame gameLobby = null;
    
    }
}