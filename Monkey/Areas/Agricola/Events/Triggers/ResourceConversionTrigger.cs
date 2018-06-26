using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class ResourceConversionTrigger : GameEventTrigger
    {
        public ResourceConversionTrigger()
            :base()
        {

        }

        public ResourceConversionTrigger(XElement definition)
            : base(definition)
        {
            var inputTypes = definition.Descendants("InputType");
            var numInputTypes = inputTypes.Count();
            if (numInputTypes > 0)
            {
                var triggerOnInputTypes = new Resource[numInputTypes];
                var i = 0;
                foreach (var inputType in inputTypes)
                {
                    triggerOnInputTypes[i++] = (Resource)Enum.Parse(typeof(Resource), (String)inputType);
                }
                TriggerOnInputTypes = triggerOnInputTypes.ToImmutableArray();
            }
            else
            {
                 TriggerOnInputTypes = ImmutableArray.Create(new[] { Resource.Boar, Resource.Cattle, Resource.Clay, Resource.Food, Resource.Grain, Resource.Reed, Resource.Sheep, Resource.Stone, Resource.Vegetables, Resource.Wood });
            }

            var cardIds = definition.Descendants("ConvertedWithCardId");
            var numCardIds = cardIds.Count();
            if (numCardIds > 0)
            {
                var triggerOnCardIds = new int[numCardIds];
                var i = 0;
                foreach (var cardId in cardIds)
                {
                    triggerOnCardIds[i++] = (int)cardId;
                    TriggerOnCardIds = triggerOnCardIds.ToImmutableArray();
                }
            }
            else
            {
                TriggerOnCardIds = ImmutableArray<int>.Empty;
            }
        }

        public override bool Triggered(AgricolaPlayer resolvingPlayer, AgricolaPlayer triggeringPlayer, GameEventTrigger trigger)
        {

            if (!base.Triggered(resolvingPlayer, triggeringPlayer, trigger))
                return false;

            var incomingTrigger = (ResourceConversionTrigger)trigger;
            var matches = incomingTrigger.ResourcesConverted.Where(x => TriggerOnInputTypes.Contains(x.InType));

            if (TriggerOnCardIds.Count() > 0)
                matches = matches.Where(x => TriggerOnCardIds.Contains(x.Id));

            incomingTrigger.TriggeringResourcesConverted = matches.ToArray();
            return matches.Count() > 0;
                
        }

        public void AddConvertedResources(ResourcesConvertedData converted)
        {
            resourcesConverted.Add(converted);
        }

        public ResourcesConvertedData[] ResourcesConverted
        {
            get { return resourcesConverted.ToArray(); }
        }

        public ResourcesConvertedData[] TriggeringResourcesConverted;

        private List<ResourcesConvertedData> resourcesConverted = new List<ResourcesConvertedData>();

        /// <summary>
        /// Types being converted that trigger, defaulted to all types
        /// </summary>
        private ImmutableArray<Resource> TriggerOnInputTypes { get; }
        private ImmutableArray<int> TriggerOnCardIds { get; }
    }
}