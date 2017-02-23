using Monkey.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Game
{
    public class LobbyManager
    {
        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static LobbyManager Instance{
            get { return instance; }
        }

        public PreGame CreateGameLobby(string gameTitle, string name, Player creator, int maxPlayers, Dictionary<string, object> props)
        {
            var game = new PreGame(gameTitle, name, creator, props, maxPlayers);
            lobbies[game.Id] = game;
            gamesByCreatorId[creator.ConnectionId] = game;

            
            return game;
        }

        public PreGame[] Games
        {
            get
            {
                return lobbies.Values.ToArray();
            }
        }

        public PreGame GetGame(String id)
        {
            return GetGame(Guid.Parse(id));
        }

        public PreGame GetGame(Guid id)
        {
            if (lobbies.ContainsKey(id))
                return lobbies[id];
            return null;
        }

        public PreGame UsersGame(Player user)
        {
            if (gamesByCreatorId.ContainsKey(user.ConnectionId))
                return gamesByCreatorId[user.ConnectionId];
            return null;
        }

        public void RemoveGame(PreGame game)
        {
            if (lobbies.ContainsKey(game.Id))
                lobbies.Remove(game.Id);

            if (gamesByCreatorId.ContainsKey(game.Creator.ConnectionId))
                gamesByCreatorId.Remove(game.Creator.ConnectionId);
        }

        public IGame<GameHub> StartGame(PreGame pregame)
        {
            lobbies.Remove(pregame.Id);
            gamesByCreatorId.Remove(pregame.Creator.ConnectionId);
            gamesAwaitingStart[pregame.Id] = pregame;
            return GameManager.Instance.StartGame(pregame);
        }
        

        private Dictionary<Guid, PreGame> lobbies = new Dictionary<Guid, PreGame>();
        private Dictionary<string, PreGame> gamesByCreatorId = new Dictionary<string, PreGame>();

        private Dictionary<Guid, PreGame> gamesAwaitingStart = new Dictionary<Guid, PreGame>();

        private static LobbyManager instance = new LobbyManager();
    }
}