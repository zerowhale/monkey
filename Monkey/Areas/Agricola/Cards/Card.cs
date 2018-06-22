using Monkey.Games.Agricola.Actions.AnytimeActions;
using Monkey.Games.Agricola.Cards.Costs;
using Monkey.Games.Agricola.Cards.GameEndPoints;
using Monkey.Games.Agricola.Cards.Prerequisites;
using Monkey.Games.Agricola.Data;
using Monkey.Games.Agricola.Events;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Monkey.Games.Agricola.Utils;
using System.Collections.Immutable;

namespace Monkey.Games.Agricola.Cards
{
    /// <summary>
    /// The base class for all Cards in the game.
    /// </summary>
    public abstract class Card
    {
        public Card(XElement definition, string jsonType)
        {
            this.definition = definition;
            this.JsonType = jsonType;

            Id = (int)definition.Attribute("Id");
            Name = (string)definition.Element("Name");
            Text = definition.Element("Text").Value;
            Image = (string)definition.Attribute("Image");
            AnytimeAction = definition.Descendants("AnytimeAction").Select(AnytimeAction.Create).FirstOrDefault();
            GameEndPoints = definition.Elements("VictoryPointCalculator").Select(g => PointCalculator.Create(g, this)).ToArray();
            Events = definition.Elements("Event").Select(TriggeredEvent.Create).ToArray();
            OnPlayEvents = definition.Descendants("OnPlay").Select(GameEvent.Create).ToArray();

            var costs = definition.Grandchildren("Costs", "Option").Select(CardCost.Create).ToArray();
            Costs = (costs.Length == 0 ? new CardCost[] { new FreeCardCost() } : costs).ToImmutableArray<CardCost>();

            var bakeProperties = definition.Descendants("Bake").Select(ResourceConversion.Create).FirstOrDefault();
            if (bakeProperties != null)
                bakeProperties.Id = this.Id;
            BakeProperties = bakeProperties;

            resourceConversions = definition.Grandchildren("ResourceConversions", "ResourceConversion").Select(ResourceConversion.Create).ToArray();
            if (resourceConversions != null)
            {
                foreach (var resourceConversion in resourceConversions)
                    resourceConversion.Id = Id;
            }
            ResourceConversions = resourceConversions;

            Prerequisites = definition.Elements("Prerequisite").Select(Prerequisite.Create).ToArray();
            Deck deck;
            if (Enum.TryParse((string)definition.Attribute("Deck"), out deck))
                this.Deck = deck;

            var cacheExchanges = definition.Grandchildren("TakeCacheExchange", "CacheExchange").Select(CacheExchange.Create).ToArray();
            if (cacheExchanges != null)
            {
                foreach (var cacheExchange in cacheExchanges)
                    cacheExchange.Id = Id;
            }
            CacheExchanges = cacheExchanges;
        }

        public static Card Create(XElement definition)
        {
            Type cls;
            switch(definition.Name.LocalName){
                case "MajorImprovement":
                    cls = typeof(MajorImprovement);
                    break;
                case "MinorImprovement":
                    cls = typeof(MinorImprovement);
                    break;
                case "Occupation":
                    cls = typeof(Occupation);
                    break;
                default:
                    throw new ArgumentException("Invalid card type");
            }

            return (Card)Activator.CreateInstance(cls, definition);
        }

        /// <summary>
        /// The cards unique identifier
        /// </summary>
        public readonly int Id;

        /// <summary>
        /// The card name
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// The cards cost options
        /// </summary>
        public readonly ImmutableArray<CardCost> Costs;

        /// <summary>
        /// The card effect description text
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// Resource Conversions that occur on bake opperations
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public readonly ResourceConversion BakeProperties;

        /// <summary>
        /// Resource Conversions that occur as any time actions
        /// (Including cooking)
        /// </summary>
        public readonly ResourceConversion[] ResourceConversions;
        
        /// <summary>
        /// Url of the image that represents this card
        /// </summary>
        public readonly string Image;

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public readonly AnytimeAction AnytimeAction;

        [JsonIgnore]
        public readonly PointCalculator[] GameEndPoints;

        [JsonIgnore]
        public readonly TriggeredEvent[] Events;

        [JsonIgnore]
        public readonly GameEvent[] OnPlayEvents;

        [JsonProperty(PropertyName = "Type")]
        public readonly string JsonType;



        /// <summary>
        /// Checks if all prerequisites of the card are met
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool PrerequisitesMet(AgricolaPlayer player)
        {
            return !Prerequisites.Any(x => !x.IsMet(player));
        }

        /// <summary>
        /// The deck this card belongs to (Basic, Intermediate, Expert)
        /// </summary>
        [JsonIgnore]
        public readonly Deck? Deck;

        /// <summary>
        /// The prerequisites to play this card
        /// </summary>
        public readonly Prerequisite[] Prerequisites;

        /// <summary>
        /// Cache exchanges that are available
        /// </summary>
        public readonly CacheExchange[] CacheExchanges;

        private readonly XElement definition;

        private ResourceConversion bakeProperties;
        private ResourceConversion[] resourceConversions;
        private PointCalculator[] gameEndPoints;
    }
}