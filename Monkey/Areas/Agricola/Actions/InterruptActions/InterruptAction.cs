using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.InterruptActions
{
    /// <summary>
    /// Interrupt actions are created as needed and discarded after execution.
    /// </summary>
    public abstract class InterruptAction: GameAction
    {
        public InterruptAction(AgricolaPlayer player, int id, List<GameActionNotice> resultingNotices)
            : base((AgricolaGame)player.Game, id, resultingNotices)
        {
            Player = player;
        }

        public InterruptAction(AgricolaPlayer player, int id, List<GameActionNotice> resultingNotices, GameEventTrigger[] eventTriggers)
            : base((AgricolaGame)player.Game, id, resultingNotices, eventTriggers)
        {
            Player = player;
        }

        public InterruptAction(AgricolaPlayer player, int id, List<GameActionNotice> resultingNotices, GameEventTrigger eventTrigger)
            : base((AgricolaGame)player.Game, id, resultingNotices, eventTrigger)
        {
            Player = player;
        }

        /// <summary>
        /// The player name, used to send to the client
        /// </summary>
        [JsonProperty(PropertyName="Player")]
        public string PlayerName
        {
            get { return Player.Name; }
        }

        /// <summary>
        /// The player that owns the Interrupt Action
        /// </summary>
        [JsonIgnore]
        public AgricolaPlayer Player { get; }
        
        /// <summary>
        /// Type of this event
        /// </summary>
        [JsonProperty(PropertyName = "Type")]
        public string ClassType
        {
            get { return this.GetType().Name; }
        }

    }
}