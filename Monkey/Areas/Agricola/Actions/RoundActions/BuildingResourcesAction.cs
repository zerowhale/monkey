using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.Data;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.RoundActions
{
    public class BuildingResourcesAction: RoundAction
    {
        public BuildingResourcesAction(AgricolaGame game, Int32 id, BuildingResourcesActionMode mode, GameEventTrigger[] eventTriggers = null)
            : base(game, id, eventTriggers)
        {
            this.mode = mode;
        }

        public override bool CanExecute(AgricolaPlayer player, GameActionData data)
        {
            if (!base.CanExecute(player, data))
                return false;

            if (mode == BuildingResourcesActionMode.DoubleResourceOrFamilyGrowth && ((BuildingResourcesActionData)data).Growth == true)
            {
                if (Game.CurrentRound < 5 || !Curator.CanGrowFamily(player))
                    return false;
            }
            else{
                if (!((BuildingResourcesActionData)data).Resource1.HasValue)
                    return false;

                if (mode == BuildingResourcesActionMode.DoubleResource || mode == BuildingResourcesActionMode.DoubleResourceOrFamilyGrowth)
                {
                    if(!((BuildingResourcesActionData)data).Resource2.HasValue)
                        return false;
                }

            }

            return true;
        }

        public override GameAction OnExecute(AgricolaPlayer player, GameActionData data)
        {
            base.OnExecute(player, data);

            if (mode == BuildingResourcesActionMode.DoubleResourceOrFamilyGrowth && ((BuildingResourcesActionData)data).Growth == true)
            {
                player.AddFamilyMember();
                AddUser(State, player);    // Add the baby to the action display

                this.ResultingNotices.Add(new GameActionNotice(player.Name, NoticeVerb.GrowFamily.ToString()));
            }
            else
            {
                var resource1 = ((BuildingResourcesActionData)data).Resource1;
                var resource2 = ((BuildingResourcesActionData)data).Resource2;

                switch (mode)
                {
                    case BuildingResourcesActionMode.SingleResource:
                        ActionService.AssignCacheResource(player, eventTriggers, ResultingNotices, new ResourceCache(resource1.Value, 1));
                        break;
                    case BuildingResourcesActionMode.SingleResourceWithFood:
                        ActionService.AssignCacheResources(player,
                            eventTriggers, ResultingNotices,
                            new ResourceCache[] { 
                                new ResourceCache(resource1.Value, 1),
                                new ResourceCache(Resource.Food, 1)
                            });
                        break;
                    case BuildingResourcesActionMode.DoubleResource:
                    case BuildingResourcesActionMode.DoubleResourceOrFamilyGrowth:
                        ActionService.AssignCacheResources(player, eventTriggers, ResultingNotices,
                            new ResourceCache[] { 
                                new ResourceCache(resource1.Value, 1), 
                                new ResourceCache(resource2.Value, 1)
                            });
                        break;
                }
            }

            return this;
        }

        private BuildingResourcesActionMode mode { get; }


    }
}