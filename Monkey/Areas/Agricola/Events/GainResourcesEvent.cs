using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            var result = from item in definition.Elements("ResourceData")
                         select new ResourceData(
                             new ResourceCache((Resource)Enum.Parse(typeof(Resource), (string)item.Attribute("Type")),
                                                             (int)item.Attribute("Count")),
                             item.Attribute("FromExecution") != null ? (int)item.Attribute("FromExecution") : 0,
                             item.Attribute("UntilExecution") != null ? (int)item.Attribute("UntilExecution") : Int32.MaxValue,
                             item.Attribute("FromRound") != null ? (int)item.Attribute("FromRound") : 1
                         );
            Resources = result.ToImmutableArray();

        }

        protected override void OnExecute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices)
        {
            
            var nonAnimals = new List<ResourceCache>();
            var animals = new List<ResourceCache>();

            foreach (var resourceData in Resources)
            {
                int executionCount = 0;
                if (card != null)
                {
                    Object fieldData;
                    ImmutableDictionary<string, object> metadata;
                    if(player.TryGetCardMetadataField(card, GameEvent.MetadataKeyExecutionCount, out metadata, out fieldData))
                    {
                        executionCount = (int)fieldData;
                    }
                }

                if (((AgricolaGame)player.Game).CurrentRound >= resourceData.FromRound
                    && executionCount >= resourceData.FromExecution
                    && executionCount < resourceData.UntilExecution)
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
                ActionService.AssignResources(player, nonAnimals.ToImmutableArray(), resultingNotices);

            if (animals.Count > 0)
                ((AgricolaGame)player.Game).AddInterrupt(new AssignAnimalsAction(player, animals.ToArray(), resultingNotices));

        }

        public ImmutableArray<ResourceData> Resources { get; }

        public class ResourceData
        {
            public ResourceData(ResourceCache resource, int fromExecution, int untilExecution, int fromRound)
            {
                Resource = resource;
                FromExecution = fromExecution;
                UntilExecution = untilExecution;
                FromRound = fromRound;
            }

            public ResourceCache Resource { get; }

            [JsonIgnore]
            public int FromExecution { get; }
            
            [JsonIgnore]
            public int UntilExecution { get; }

            [JsonIgnore]
            public int FromRound { get; }
        }

    }
}