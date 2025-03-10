using Microsoft.AspNet.SignalR;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace BoardgamePlatform.Game
{
    public class GameManager
    {

        private GameManager()
        {

        }

        /// <summary>
        /// Singleton accessor.
        /// </summary>
        public static GameManager Instance
        {
            get { return instance; }
        }

        /// <summary>
        /// When a player reconnects they are assigned a new signalR connection
        /// id.  This method reconciles the new id with the old player object
        /// </summary>
        /// <param name="player"></param>
        /// <param name="connectionId"></param>
        public void UpdatePlayerConnection(Player player, string connectionId)
        {
            playersByConnection.Remove(player.ConnectionId.ToString());
            player.ConnectionId = connectionId;
            playersByConnection[connectionId] = player;

        }

        /// <summary>
        /// Begins a new game based off the pregame lobby
        /// </summary>
        /// <param name="pregame"></param>
        /// <returns></returns>
        public IGame<GameHub> StartGame(PreGame pregame)
        {
            
            var game = pregame.StartGame();

            games.Add(game);

            foreach (var p in pregame.Players){
                playersInGames[p.Id] = p;
                var connId = p.ConnectionId.ToString();
                playersByConnection[connId] = p;
            }

            return game;
        }

        public void LeaveGame(Player player)
        {
            var game = player.Game;
            if (game != null)
            {
                playersByConnection.Remove(player.ConnectionId.ToString());
                playersInGames.Remove(player.Id.ToString());
                foreach (var p in game.Players)
                {
                    if (playersInGames.ContainsKey(p.Player.Id))
                        return;
                }
                games.Remove(game);
            }
        }

        /// <summary>
        /// Check if a player is part of the given game.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public Boolean IsPlayerInGame(String id, out Player player)
        {
            if (id != null)
            {

                if (playersInGames.ContainsKey(id))
                {
                    player = playersInGames[id];
                    return true;
                }
            }
            player = null;
            return false;
        }

        /// <summary>
        /// Gets a player by their connection id
        /// </summary>
        /// <param name="connectionId"></param>
        /// <returns></returns>
        public Player GetPlayer(String connectionId){
            return playersByConnection[connectionId];
        }


        /// <summary>
        /// Attempts to get a game from a players connection id.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="game"></param>
        /// <returns>True if the game is found.</returns>
        public Boolean TryGetGameFromConnection(String connectionId, out IGame<GameHub> game)
        {
            var player = playersByConnection[connectionId];
            if (player != null)
            {
                game = player.Game;
                return true;
            }
            game = null;
            return false;
        }

        /// <summary>
        /// All the active games
        /// </summary>
        private List<IGame<GameHub>> games = new List<IGame<GameHub>>();
        
        /// <summary>
        /// All players in all active games by player id
        /// </summary>
        private Dictionary<String, Player> playersInGames = new Dictionary<String, Player>();

        /// <summary>
        /// All players in all active games by connection id
        /// </summary>
        private Dictionary<String, Player> playersByConnection = new Dictionary<String, Player>();

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static GameManager instance = new GameManager();
    }
}