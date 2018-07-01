using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoardgamePlatform.Game
{
    /// <summary>
    /// GamePlayer represents a union between a player and an instance of a game.
    /// Games should extend this class to manage player specific game data.
    /// </summary>
    public class GamePlayer
    {
        public GamePlayer(IGame<GameHub> game, Player player)
        {
            Game = game;
            Player = player;
        }

        /// <summary>
        /// The game
        /// </summary>
        [JsonIgnore]
        public IGame<GameHub> Game { get; }

        /// <summary>
        /// The player
        /// </summary>
        [JsonIgnore]
        public Player Player { get; }

        /// <summary>
        /// Name output for the client
        /// </summary>
        public string Name
        {
            get { return Player.Name; }
        }

        /// <summary>
        /// Color output for the client
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PlayerColor Color
        {
            get { return Player.Color; }
        }

    }
}