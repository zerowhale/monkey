using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Actions.InterruptActions;
using Monkey.Games.Agricola.Actions.Services;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    public class GainResourcesPerConversionEvent : GainResourcesEvent
    {
        public GainResourcesPerConversionEvent(XElement definition)
            : base(definition)
        {

            perNumConverted = definition.Attribute("PerNumConverted") != null ? (int)definition.Attribute("PerNumConverted") : 1;
        }

        protected override void OnExecute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices)
        {
            var totalConverted = 0;
            foreach (var resource in ((ResourceConversionTrigger)trigger).TriggeringResourcesConverted)
            {
                totalConverted += resource.AmountConverted;
            }

            var activations = totalConverted / perNumConverted;
            var newResources = new Dictionary<Resource, int>();

            foreach (var resourceData in Resources)
            {
                if (((AgricolaGame)player.Game).CurrentRound >= resourceData.FromRound)
                {
                    newResources[resourceData.Resource.Type] = resourceData.Resource.Count * activations;
                }
            }

            var nonAnimals = new List<ResourceCache>();
            var animals = new List<ResourceCache>();
            foreach (var resource in newResources)
            {
                if (resource.Key.IsAnimal())
                    animals.Add(new ResourceCache(resource.Key, resource.Value));
                else
                    nonAnimals.Add(new ResourceCache(resource.Key, resource.Value));
            }

            
            if (nonAnimals.Count > 0)
                ActionService.AssignResources(player, nonAnimals.ToArray(), resultingNotices);

            if (animals.Count > 0)
                ((AgricolaGame)player.Game).AddInterrupt(new AssignAnimalsAction(player, animals.ToArray(), resultingNotices));
            
        }

        private readonly int perNumConverted = 1;
    }
}