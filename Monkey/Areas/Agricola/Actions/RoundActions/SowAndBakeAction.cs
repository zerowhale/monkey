using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class SowAndBakeAction: RoundAction
    {
        public SowAndBakeAction(AgricolaGame game, int id)
            : base(game, id)
        {

        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;


            var toSow =  ImmutableArray.Create(((SowAndBakeActionData)data).Sow);
            var toBake = ImmutableArray.Create(((SowAndBakeActionData)data).BakeData);


            if (!ActionService.CanSowAndBake(player, toSow, toBake ))
                return false;

            return true;
        }

        public override void OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            base.OnExecute(player, data);

            var sowData = ImmutableArray.Create(((SowAndBakeActionData)data).Sow);
            var bakeData = ImmutableArray.Create(((SowAndBakeActionData)data).BakeData);

            ActionService.Sow(player, sowData, ResultingNotices);
            ActionService.Bake(player, eventTriggers, ResultingNotices, bakeData);
            
        }
    }
}