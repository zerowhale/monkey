using Monkey.Game.Notification;
using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class GainResourcesEvent : TriggeredEvent
    {
        public GainResourcesEvent(XElement definition)
            : base(definition)
        {
            var result = from item in definition.Descendants("ResourceData")
                         select new ResourceData()
                         {
                             Resource = new ResourceCache((Resource)Enum.Parse(typeof(Resource), (string)item.Attribute("Type")),
                                                             (int)item.Attribute("Count")),
                             FromRound = item.Attribute("FromRound") != null ? (int)item.Attribute("FromRound") : 1,
                             FromExecution = item.Attribute("FromExecution") != null ? (int)item.Attribute("FromExecution") : 0,
                             UntilExecution = item.Attribute("UntilExecution") != null ? (int)item.Attribute("UntilExecution") : Int32.MaxValue
                         };
            Resources = result.ToArray();

        }

        protected override void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            
            var nonAnimals = new List<ResourceCache>();
            var animals = new List<ResourceCache>();

            foreach (var resourceData in Resources)
            {
                if (((AgricolaGame)player.Game).CurrentRound >= resourceData.FromRound
                    && this.ExecutionCount >= resourceData.FromExecution
                    && this.ExecutionCount < resourceData.UntilExecution)
                {
                    if (resourceData.Resource.Type.IsAnimal())
                    {
                        animals.Add(resourceData.Resource);
                    }
                    else
                    {
                        nonAnimals.Add(resourceData.Resource);
                    }
                }
            }

            if (nonAnimals.Count > 0)
                ActionService.AssignResources(player, nonAnimals.ToArray(), resultingNotices);

            if (animals.Count > 0)
                ((AgricolaGame)player.Game).AddInterrupt(new AssignAnimalsAction(player, animals.ToArray(), resultingNotices));

        }

        public ResourceData[] Resources
        {
            get;
            set;
        }

        public class ResourceData
        {
            public ResourceCache Resource;

            [JsonIgnore]
            public int FromExecution = 0;
            
            [JsonIgnore]
            public int UntilExecution = Int32.MaxValue;

            [JsonIgnore]
            public int FromRound = 1;
        }

    }
}