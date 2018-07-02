using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.InterruptActions
{
    public class PlowAction: InterruptAction
    {
        public PlowAction(AgricolaPlayer player, List<GameActionNotice> resultingNotices, bool optional)
            : base(player, (int)InterruptActionId.Plow, resultingNotices)
        {
            this.Optional = optional;
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            var fields = ((PlowAndSowActionData)data).Fields;
            if (Optional && (fields == null || fields.Length == 0))
                return true;

            return ActionService.CanPlowAndSow(player, Id, fields, new SowData[] { });
        }

        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            ActionService.Plow(player, ((PlowAndSowActionData)data).Fields, ResultingNotices);
            return this;
        }

        public bool Optional { get; }
    }
}