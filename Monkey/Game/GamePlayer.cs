using Microsoft.AspNet.SignalR.Hubs;
using Monkey.Games.Agricola;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Game
{
    public class GamePlayer
    {
        public GamePlayer(IGame<GameHub> game, Player player)
        {
            Game = game;
            Player = player;
        }

        [JsonIgnore]
        public IGame<GameHub> Game
        {
            get;
            private set;
        }

        [JsonIgnore]
        public Player Player
        {
            get;
            private set;
        }


        public string Name
        {
            get { return Player.Name; }
        }



        [JsonConverter(typeof(StringEnumConverter))]
        public PlayerColor Color
        {
            get { return Player.Color; }
        }

    }
}