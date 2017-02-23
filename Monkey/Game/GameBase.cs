using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Monkey.Games.Agricola;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Game
{
    public abstract class GameBase<T> : IGame<T> where T : GameHub
    {
        
        /// <summary>
        /// Initializes a new game.
        /// At the time this is called the client will not be loaded into the game yet, so direct
        /// calls to client methods will not resolve.
        /// </summary>
        /// <param name="name">Game display name</param>
        /// <param name="viewPath">Game view path</param>
        /// <param name="maxPlayers">Maximum players allowed in a game</param>
        /// <param name="players">List of players playing</param>
        /// <param name="props">Game specific properties</param>
        public GameBase(string name, string viewPath, int maxPlayers, Player[] players, Dictionary<string, object> props) {
            Name = name;
            ViewPath = viewPath;
            Id = Guid.NewGuid();
            MaxPlayers = maxPlayers;
        }

        [JsonIgnore]
        public int MaxPlayers
        {
            get;
            private set;
        }

        public abstract GamePlayer[] Players
        {
            get;
        }

        public abstract string Title
        {
            get;
        }

        [JsonIgnore]
        public Guid Id
        {
            get;
            private set;
        }

        [JsonIgnore]
        public String ViewPath
        {
            get;
            private set;
        }

        public String Name
        {
            get;
            private set;
        }

        protected IHubContext HubContext
        {
            get
            {
                if (hubContext == null) 
                    hubContext = GlobalHost.ConnectionManager.GetHubContext<T>();
                return hubContext;
            }
            
        }

        public abstract void SendPlayerGameStart(GamePlayer player);

        private IHubContext hubContext;

    }
}