using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using Monkey.Game;


namespace Monkey.Games.PushPull
{
    public class PushPullHub : GameHub
    {

        private void UpdateGameState(PushPullGame game, IGameUpdate update)
        {
            foreach (var player in game.Players)
            {
                Clients.Client(player.Player.ConnectionId.ToString()).update(update);
            }
        }


        /// <summary>
        /// Gets the player for the current connection
        /// </summary>
        /// <returns></returns>
        private PushPullPlayer GetPlayer()
        {
            return (PushPullPlayer)gameManager.GetPlayer(Context.ConnectionId).GamePlayer;
        }

    }
}