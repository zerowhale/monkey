using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions
{
    public abstract class GameAction
    {
        public GameAction(AgricolaGame game, int id) 
            : this(game, id, new List<GameActionNotice>()) { }

        public GameAction(AgricolaGame game, int id, List<GameActionNotice>resultingNotices)
            : this(game, id, resultingNotices, new GameEventTrigger[] { }) { }

        public GameAction(AgricolaGame game, int id, List<GameActionNotice> resultingNotices, GameEventTrigger eventTrigger)
        {
            Game = game;
            Id = id;
            ResultingNotices = resultingNotices;
            this.eventTriggers = ImmutableList<GameEventTrigger>.Empty.Add(eventTrigger);
        }

        public GameAction(AgricolaGame game, int id, List<GameActionNotice> resultingNotices, GameEventTrigger[] eventTriggers)
        {
            Game = game;
            Id = id;
            ResultingNotices = resultingNotices;
            this.eventTriggers = eventTriggers != null ? ImmutableList.Create(eventTriggers) : ImmutableList<GameEventTrigger>.Empty;
        }


        /// <summary>
        /// Called when a player attempts to use this action.
        /// The default behavior checks if the action is open and the player
        /// has a family member to place.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public Boolean TryExecute(AgricolaPlayer player, GameActionData data)
        {
            if (CanExecute(player, data)) {
                if(ResultingNotices != null)
                    ResultingNotices.Clear();
                OnExecute(player, data);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called when an action executes.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        public abstract void OnExecute(AgricolaPlayer player, GameActionData data);

        /// <summary>
        /// Called before an action executes to verify that the action
        /// can take place given the supplied data, player, and game state.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract Boolean CanExecute(AgricolaPlayer player, GameActionData data);


        /// <summary>
        /// Listing of notices that have resulted from using this action.
        /// </summary>
        [JsonIgnore]
        public List<GameActionNotice> ResultingNotices { get; }

        /// <summary>
        /// The game this action belongs to
        /// </summary>
        [JsonIgnore]
        public AgricolaGame Game { get; }

        /// <summary>
        /// The action id.
        /// </summary>
        public Int32 Id { get; }

        protected ImmutableList<GameEventTrigger> eventTriggers { get; }

    }
}