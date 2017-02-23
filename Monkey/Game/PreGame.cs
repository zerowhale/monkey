using Microsoft.AspNet.SignalR;
using Monkey.Game;
using Monkey.Games.Agricola;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Game
{
    public class PreGame
    {


        public PreGame(string gameTitle, String name, Player creator,  Dictionary<string, object> props, Int32 maxPlayers)
        {
            gameType = gameTitle;
            GameTitle = gameTitle;
            Name = name;
            Id = Guid.NewGuid();
            MaxPlayers = maxPlayers;
            this.props = props;
            Creator = creator;
            
        }

        public void AddPlayer(Player player)
        {
            if (!players.Contains(player))
            {
                if (player.GameLobby != null)
                    player.GameLobby.RemovePlayer(player);

                var colors = Enum.GetValues(typeof(PlayerColor)).Cast<PlayerColor>().ToList();
                colors.Remove(Creator.Color);
                foreach(var otherPlayers in players){
                    colors.Remove(otherPlayers.Color);
                }
                player.Color = colors[(new Random()).Next(colors.Count())];

                players.Add(player);
                player.GameLobby = this;
            }
        }

        public bool TrySetPlayerColor(Player player, PlayerColor color)
        {
            foreach (var otherPlayer in players)
            {
                if (otherPlayer.Color == color)
                    return false;
            }
            player.Color = color;
            return true;
        }

        public void RemovePlayer(Player player)
        {
            players.Remove(player);
            player.GameLobby = null;

            foreach (var otherPlayer in players)
            {
                otherPlayer.Ready = false;
            }
        }

        public IGame<GameHub> StartGame()
        {
            return GameFactory.CreateGame(gameType, Name, players.ToArray(), props);
        }

        [JsonProperty(PropertyName="Creator")]
        public string CreatorName
        {
            get { return Creator.Name;  }
        }

        [JsonIgnore]
        public Player Creator
        {
            get;
            private set;
        }

        public String Code
        {
            get;
            private set;
        }

        public String Name
        {
            get;
            private set;
        }

        public Guid Id
        {
            get;
            private set;
        }

        public string GameTitle
        {
            get;
            private set;
        }

        public Player[] Players
        {
            get { return players.ToArray(); }
        }

        public Int32 NumPlayers
        {
            get { return players.Count ; }
        }

        public Boolean IsFull
        {
            get { return players.Count >= MaxPlayers; }
        }


        public Boolean PlayersReady
        {
            get
            {
                for (var i = 0; i < players.Count; i++)
                    if (!players[i].Ready)
                        return false;
                return true;
            }
        }

        public Int32 MaxPlayers
        {
            get;
            set;
        }

        public Dictionary<string, object> Props
        {
            get { return props.ToDictionary(x => x.Key, y => y.Value); }
        }

        private string gameType;

        private Dictionary<string, object> props = new Dictionary<string, object>();

        private List<Player> players = new List<Player>();
    }
}