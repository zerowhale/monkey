using Monkey.Game.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class DebugAction: RoundAction
    {
        public DebugAction(AgricolaGame game, Int32 id)
            : base(game, id)
        {

        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            return true;
        }

        public override void OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            this.ResultingNotices.Add(new GameActionNotice(player.Name, Notification.NoticeVerb.Debug.ToString()));
        }
    }
}