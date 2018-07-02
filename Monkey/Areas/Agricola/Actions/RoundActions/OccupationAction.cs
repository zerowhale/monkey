using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.RoundActions;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class OccupationAction : RoundAction
    {
        public OccupationAction(AgricolaGame game, int id, bool familyGrowth = false)
            : base(game, id)
        {
            this.familyGrowth = familyGrowth;

        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            var occupationData = (OccupationActionData)data;

            if (occupationData.FamilyGrowth)
            {
                if (((AgricolaGame)player.Game).CurrentRound < 5)
                    return false;
            }
            else
            {
                if (!occupationData.Id.HasValue)
                    return false;

                if (!ActionService.CanPlayOccupation(player, data.ActionId, occupationData.Id.Value))
                    return false;
            }

            return true;
        }

        public override GameAction OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            var occupationData = (OccupationActionData)data;
            if(occupationData.FamilyGrowth){
                player.AddFamilyMember();
                AddUser(player);    // Add the baby to the action display

                ResultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.GrowFamily.ToString()));
            }
            else
            {
                var triggers = new List<GameEventTrigger>();
                triggers.AddRange(eventTriggers);
                triggers.Add(new TakeOccupationActionTrigger());
                ActionService.PlayOccupation(player, ImmutableList.Create<GameEventTrigger>(triggers.ToArray()), ResultingNotices, occupationData);
            }
            return this;
        }

        private bool familyGrowth = false;

    }
}