using Monkey.Game;
using Monkey.Games.PushPull.Cards;
using Monkey.Games.PushPull.ClientState;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.PushPull
{
    public class PushPullPlayer: GamePlayer
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public PushPullPlayer(PushPullGame game, Player player)
            : base((IGame<GameHub>)game, player)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("New PushPull Player [{0}] [{1}]", player.Name, this.Color);

            playerUpdate = new PartialPlayerUpdate(Name);
        }

        /// <summary>
        /// Gets the players partial update object, containing all the information
        /// that has changed since the last update was requested.
        /// </summary>
        /// <param name="reset"></param>
        /// <returns></returns>
        public PartialPlayerUpdate GetPartialUpdate(bool reset = true)
        {
            if (logger.IsInfoEnabled)
                logger.InfoFormat("Get Partial Update");

            var update = playerUpdate;
            if(reset)
                playerUpdate = new PartialPlayerUpdate(Name);
            return update;
        }

        public bool HasUpdate()
        {
            return playerUpdate.Dirty;
        }

        private PartialPlayerUpdate playerUpdate;

    }
}