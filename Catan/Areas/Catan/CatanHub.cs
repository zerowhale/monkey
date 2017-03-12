using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json.Linq;
using BoardgamePlatform.Game;


namespace Monkey.Games.Catan
{
    public class CatanHub : GameHub
    {

        private void UpdateGameState(CatanGame game, IGameUpdate update)
        {
            foreach (var player in game.Players)
            {
                Clients.Client(player.Player.ConnectionId.ToString()).update(update);
            }
        }

        public override string GetUserId()
        {
            return Context.User.Identity.GetUserId();
        }

        /// <summary>
        /// Gets the player for the current connection
        /// </summary>
        /// <returns></returns>
        private CatanPlayer GetPlayer()
        {
            return (CatanPlayer)gameManager.GetPlayer(Context.ConnectionId).GamePlayer;
        }

    }
}