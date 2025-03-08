using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class PlowAndSowAction: RoundAction
    {
        public PlowAndSowAction(AgricolaGame game, Int32 id, Boolean sowEnabled)
            : base(game, id)
        {
            this.sowEnabled = sowEnabled;
        }

        /// <summary>
        /// Checks if the passed parameters are valid.
        /// Sow values are discarded if sowEnabled is false.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public override bool CanExecute(AgricolaPlayer player, GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;



            var fields = ((PlowAndSowActionData)data).Fields;
            var sow = sowEnabled ? ((PlowAndSowActionData)data).Sow : new SowData[]{};

            return ActionService.CanPlowAndSow(player, Id, fields, sow, ((PlowAndSowActionData)data).PlowUsed);
        }

        public override GameAction OnExecute(AgricolaPlayer player, GameActionData data)
        {
            base.OnExecute(player, data);

            var fields = ((PlowAndSowActionData)data).Fields;
            ActionService.Plow(player, fields, ResultingNotices, ((PlowAndSowActionData)data).PlowUsed);

            if (sowEnabled)
            {
                var toSow = ImmutableArray.Create(((PlowAndSowActionData)data).Sow);
                ActionService.Sow(player, toSow, ResultingNotices);
            }

            return this;

        }

        private Boolean sowEnabled { get; }


    }
}