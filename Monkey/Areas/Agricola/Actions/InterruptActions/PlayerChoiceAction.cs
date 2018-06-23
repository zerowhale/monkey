using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.InterruptActions
{
    public class PlayerChoiceAction: InterruptAction
    {
        public PlayerChoiceAction(AgricolaPlayer player, PlayerChoiceOption[] options, List<GameActionNotice> resultingNotices)
            : base(player, (int)InterruptActionId.PlayerChoice, resultingNotices)
        {
            Options = ImmutableArray.Create(options);
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            //return ActionService.CanPlowAndSow(player, ((PlowAndSowActionData)data).Fields, new SowData[] { });
            var choice = ((PlayerChoiceData)data).Choice;
            selected = Options.Where(x => x.Id == choice).FirstOrDefault();
            if (selected != null)
                return true;

            return false;
        }

        public override void OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            selected.Event.Execute(player, ResultingNotices);
        }

        public readonly ImmutableArray<PlayerChoiceOption> Options;

        private PlayerChoiceOption selected;
    }
}