using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.InterruptActions
{
    public abstract class InterruptAction: GameAction
    {

        public InterruptAction(AgricolaPlayer player, int id, List<GameActionNotice> resultingNotices)
            : base((AgricolaGame)player.Game, id, resultingNotices)
        {
            Player = player;
        }

        [JsonProperty(PropertyName="Player")]
        public string PlayerName
        {
            get { return Player.Name; }
        }

        [JsonIgnore]
        public readonly AgricolaPlayer Player;
        
        [JsonProperty(PropertyName = "Type")]
        public string ClassType
        {
            get { return this.GetType().Name; }
        }

    }
}