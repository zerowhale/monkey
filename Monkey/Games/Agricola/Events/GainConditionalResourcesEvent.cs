using Monkey.Game.Notification;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class GainConditionalResourcesEvent: TriggeredEvent
    {
        public GainConditionalResourcesEvent(XElement definition)
            :base(definition)
        {
            var resourceDependantResources = from item in definition.Descendants("ResourceDependantResource")
                                             select new ResourceDependantResource
                                             {
                                                 RequiredType = (Resource)Enum.Parse(typeof(Resource), (string)item.Attribute("RequiredType")),
                                                 RequiredCount = (int)item.Attribute("RequiredCount"),
                                                 Type = (Resource)Enum.Parse(typeof(Resource), (string)item.Attribute("Type")),
                                                 Count = (int)item.Attribute("Count"),
                                                 Repeat = item.Attribute("Repeat") != null ? (bool)item.Attribute("Repeat") : false
                                             };

            ConditionalResources = (List<ConditionalResource>)resourceDependantResources.OrderByDescending(x => x.RequiredCount).Cast<ConditionalResource>().ToList();

            var roundsRemainingDependantResources = from item in definition.Descendants("RoundsRemainingDependantResource")
                                                    select new RoundsRemainingDependantResource
                                             {
                                                 RoundsRemaining = (int)item.Attribute("RoundsRemaining"),
                                                 Type = (Resource)Enum.Parse(typeof(Resource), (string)item.Attribute("Type")),
                                                 Count = (int)item.Attribute("Count"),
                                             };

            ConditionalResources.AddRange((List<ConditionalResource>)roundsRemainingDependantResources.OrderByDescending(x => x.RoundsRemaining).Cast<ConditionalResource>().ToList());

            

        }

        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {

            foreach (var conversion in ConditionalResources)
            {
                conversion.OnExecute(player, resultingNotices);
            }



        }

        private List<ConditionalResource> ConditionalResources
        {
            get;
            set;
        }

        private abstract class ConditionalResource
        {
            public abstract void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices);

            public Resource Type;
            public int Count;
        }

        private class ResourceDependantResource : ConditionalResource
        {
            public override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
            {
                int owned = 0;
                if (this.RequiredType.IsAnimal())
                {
                    owned = player.Farmyard.GetAnimalCount((AnimalResource)this.RequiredType);
                }
                else
                {
                    owned = player.PersonalSupply.GetResource(this.RequiredType);
                }

                if (!this.Repeat)
                {
                    if (owned >= this.RequiredCount)
                    {
                        ActionService.AssignResource(player, new ResourceCache(this.Type, this.Count), resultingNotices);
                    }
                }
                else
                {
                    var count = (int)(owned / this.RequiredCount);
                    ActionService.AssignResource(player, new ResourceCache(this.Type, this.Count * count), resultingNotices);
                }
            }

            public Resource RequiredType;
            public int RequiredCount;
            public bool Repeat = false;
        }

        private class RoundsRemainingDependantResource : ConditionalResource
        {
            public override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
            {
                var gameRoundsRemaining = ((AgricolaGame)player.Game).RoundsRemaining;
                if(gameRoundsRemaining >= RoundsRemaining){
                    if(this.Type.IsAnimal()){
                        // Animals not supported during this action currently
                    }
                    else{
                        ActionService.AssignResource(player, new ResourceCache(this.Type, this.Count), resultingNotices);
                    }

                }
            }

            public int RoundsRemaining;
        }

    }
}