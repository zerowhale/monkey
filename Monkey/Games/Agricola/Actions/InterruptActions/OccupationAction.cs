using Monkey.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.InterruptActions
{
    public class OccupationAction: InterruptAction
    {
        public OccupationAction(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
            : base(player, (int)InterruptActionId.Occupation, resultingNotices)
        {
        }

        public override bool CanExecute(AgricolaPlayer player, Data.GameActionData data)
        {
            var occupationData = (OccupationActionData)data;

            if (!occupationData.Id.HasValue)
                return true;

            ResourceCache[] costs;
            if (!ActionService.CanPlayOccupation(player, data.ActionId, occupationData.Id.Value, out costs))
                return false;


            return true;
        }

        public override void OnExecute(AgricolaPlayer player, Data.GameActionData data)
        {

            var occupationData = (OccupationActionData)data;
            if(occupationData.Id.HasValue)
                ActionService.PlayOccupation(player, eventTriggers, ResultingNotices, occupationData);
            
        }
    
    }
}