using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using BoardgamePlatform.Game;
using Microsoft.AspNet.SignalR.Hubs;

namespace Monkey.Lobby
{
  
    public class LobbyHub : Hub
    {

        /// <summary>
        /// Fires when a user connects and creates a player
        /// entry for the user.  Then sends a list of games.
        /// 
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            var userId = Context.User.Identity.GetUserId();

            Player existingPlayer;
            if (gameManager.IsPlayerInGame(userId, out existingPlayer))
            {
                gameManager.UpdatePlayerConnection(existingPlayer, Context.ConnectionId);
                JoinGroup(existingPlayer.Game.Id.ToString());
                var connId = Context.ConnectionId;
                players.Add(connId, existingPlayer);


                Clients.Caller.launchGame(existingPlayer.Game.ViewPath);

            }
            else
            {

                var connId = Context.ConnectionId;
                var player =  new Player(userId, connId)
                {
                    Name = Context.User.Identity.Name
                };
                players.Add(connId, player);
                JoinGroup(GROUP_LOBBY);
                SendGameList();
            }
            return base.OnConnected();
        }



        /// <summary>
        /// Fires when a connection dies.  Clean up the player entry
        /// associated with the connection unless the player is in a game.
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            var player = GetPlayer();
            if (player != null)
            {
                LeaveGameLobby();
                players.Remove(player.ConnectionId.ToString());
            }

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            Console.WriteLine("Reconnected:" + Context.ConnectionId);
            return base.OnReconnected();
        }

        public void Leave()
        {
            LeaveGameLobby();
        }

        public void SendGameList()
        {
            var games = lobby.Games;
            Clients.Caller.loadGamesList(games);
        }

        public void CreateGameLobby(string gameType, string name, int maxPlayers, Dictionary<string, object> props)
        {
            var player = GetPlayer();
            if (player != null)
            {
                var game = lobby.CreateGameLobby(gameType, name, player, maxPlayers, props);
                JoinGameLobby(game.Id);
                Clients.Group(GROUP_LOBBY).addGameLobby(game);
            }
            else
            {
                Error("Failed to create lobby, player id [" + player.ConnectionId + "] not found.");
            }
        }


        public void RequestColor(PlayerColor color)
        {
            var id = Context.ConnectionId;
            var player = players[id];
            if (player.GameLobby.TrySetPlayerColor(player, color))
                Clients.Group(player.GameLobby.Id.ToString()).updateGameLobby(player.GameLobby);
        }

        /// <summary>
        /// If the current user is in a game lobby, remove them from the lobby
        /// </summary>
        public void LeaveGameLobby()
        {
            var player = GetPlayer();
            if (player != null)
            {
                var ownersGame = lobby.UsersGame(player);
                if (ownersGame != null)
                {
                    RemoveGame(ownersGame);
                }
                else if (player.GameLobby != null)
                {
                    var game = player.GameLobby;
                    game.RemovePlayer(player);
                    Clients.Group(GROUP_LOBBY).updateLobbyGameInfo(game.Id, game.NumPlayers);
                    LeaveGroup(game.Id.ToString());
                    JoinGroup(GROUP_LOBBY);
                    Clients.Group(game.Id.ToString()).updateGameLobby(game);
                    Clients.Caller.leaveGameLobby();
                }
            }
        }


        public void PlayerReady(Boolean ready)
        {
            var player = GetPlayer();
            if (player != null && player.GameLobby != null)
            {
                var pregame = player.GameLobby;
                player.Ready = ready;
                Clients.Group(pregame.Id.ToString()).updateGameLobby(pregame);

                if (pregame.PlayersReady)
                {
                    var game = lobby.StartGame(pregame);
                    Clients.Group(GROUP_LOBBY).removeLobbyGame(pregame.Id);

                    StartGame(game);

                    foreach (var p in game.Players)
                    {
                        JoinGroup(p.Player.ConnectionId.ToString(), game.Id.ToString());
                    }
                }
            }
        }


        /// <summary>
        /// Handles a user request to join a game lobby
        /// </summary>
        /// <param name="gameId"></param>
        public void JoinGameLobby(string gameId)
        {
            JoinGameLobby(new Guid(gameId));
        }


        /// <summary>
        /// Attempts to join a user to a game lobby
        /// </summary>
        /// <param name="id"></param>
        private void JoinGameLobby(Guid id)
        {
            var game = lobby.GetGame(id);
            var player = GetPlayer();
            if (game != null && player != null)
            {
                if (game.IsFull)
                {
                    Clients.Caller.joinLobbyFailed("GAME_FULL");
                }
                else
                {
                    JoinGroup(id.ToString());
                    game.AddPlayer(player);
                    Clients.Caller.moveToGameLobby(game);
                    Clients.Group(id.ToString()).updateGameLobby(game);
                    Clients.Group(GROUP_LOBBY).updateLobbyGameInfo(game.Id, game.NumPlayers);
                }
            }
            else
            {
                if (game == null)
                {
                    Error("Failed to join game lobby: Game not found.");
                }
                else if (player == null)
                {
                    Error("Failed to join game lobby: Player not found.");
                }
            }
        }

        /// <summary>
        /// Removes a game lobby from the listing.
        /// Disbands all players in the lobby.
        /// </summary>
        /// <param name="game"></param>
        private void RemoveGame(PreGame game)
        {
            lobby.RemoveGame(game);
            Clients.Group(game.Id.ToString()).leaveGameLobby("GAME_DISBANDED");
            LeaveGroup(game.Id.ToString());
            JoinGroup(GROUP_LOBBY);
            foreach (var i in game.Players)
            {
                JoinGroup(i.ConnectionId.ToString(), GROUP_LOBBY);
            }

            Clients.Group(GROUP_LOBBY).removeLobbyGame(game.Id);
        }


        /// <summary>
        /// Kicks off the game for all players in the lobby
        /// </summary>
        /// <param name="game"></param>
        private void StartGame(IGame<GameHub> game)
        {
            foreach (var player in game.Players)
            {
                Clients.Client(player.Player.ConnectionId).launchGame(game.ViewPath);
            }
        }

        /// <summary>
        /// Adds the current user to a group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        private Task JoinGroup(string groupName)
        {
            return JoinGroup(Context.ConnectionId, groupName);
        }

        private Task JoinGroup(string connectionId, string groupName)
        {
            return Groups.Add(connectionId, groupName);
        }
        /// <summary>
        /// Removes the current user from a group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        private Task LeaveGroup(string groupName)
        {
            return RemoveFromGroup(groupName, Context.ConnectionId);
        }

        /// <summary>
        /// Removes the specified user from a group
        /// </summary>
        /// <param name="groupName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        private Task RemoveFromGroup(string groupName, string id)
        {
            return Groups.Remove(id, groupName);
        }

        private void Error(string message)
        {
            Clients.Caller.error(message);
        }



        /// <summary>
        /// Gets the player for the current connection
        /// </summary>
        /// <returns></returns>
        private Player GetPlayer()
        {
            if (players.ContainsKey(Context.ConnectionId))
                return players[Context.ConnectionId];
            return null;
        }


        public const string GROUP_LOBBY = "Lobby";

        private GameManager gameManager = GameManager.Instance;

        private static Dictionary<string, Player> players = new Dictionary<string, Player>();
        private LobbyManager lobby = LobbyManager.Instance;

    }
}