using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BoardgamePlatform.Game
{
    public abstract class GameHub: Hub
    {

        /// <summary>
        /// Fires when a user connects and creates a player
        /// entry for the user.  Then sends a list of games.
        /// 
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            var userId = GetUserId(); // Context.User.Identity.GetUserId();

            Player existingPlayer;
            if (gameManager.IsPlayerInGame(userId, out existingPlayer))
            {
                gameManager.UpdatePlayerConnection(existingPlayer, Context.ConnectionId);
                JoinGroup(existingPlayer.Game.Id.ToString());
                var connId = Context.ConnectionId;
                var game = existingPlayer.Game;
                game.SendPlayerGameStart(existingPlayer.GamePlayer);
            }
            else
            {
                Clients.Caller.returnToLobby();
            }
            return base.OnConnected();
        }

        public abstract string GetUserId();
        /// <summary>
        /// Adds the current user to a group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        protected Task JoinGroup(string groupName)
        {
            return JoinGroup(Context.ConnectionId, groupName);
        }

        protected Task JoinGroup(string connectionId, string groupName)
        {
            return Groups.Add(connectionId, groupName);
        }
        /// <summary>
        /// Removes the current user from a group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        protected Task LeaveGroup(string groupName)
        {
            return RemoveFromGroup(groupName, Context.ConnectionId);
        }

        /// <summary>
        /// Removes the specified user from a group
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        protected Task RemoveFromGroup(string groupName, string id)
        {
            return Groups.Remove(id, groupName);
        }

        protected void Error(string message)
        {
            Clients.Caller.error(message);
        }

        protected void LeaveFinishedGame()
        {
            var player = GetPlayer();
            var game = player.Game;
            if (game != null)
            {
                var id = game.Id;
                LeaveGroup(id.ToString());
                gameManager.LeaveGame(player.Player);
                JoinGroup(GROUP_LOBBY);

                Clients.Caller.returnToLobby();
                //Clients.Group(id.ToString()).updateGameLobby(game);
                //Clients.Group(GROUP_LOBBY).updateLobbyGameInfo(game.Id, game.NumPlayers);
            }

        }

        /// <summary>
        /// Gets the player for the current connection
        /// </summary>
        /// <returns></returns>
        private GamePlayer GetPlayer()
        {
            return gameManager.GetPlayer(Context.ConnectionId).GamePlayer;
        }

        protected GameManager gameManager = GameManager.Instance;

        public const string GROUP_LOBBY = "lobby";

    }
}