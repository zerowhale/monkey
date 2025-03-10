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
            AnytimeAction = definition.Elements("AnytimeAction").Select(x => AnytimeAction.Create(x, this.Id)).FirstOrDefault();
            GameEndPoints = definition.Elements("VictoryPointCalculator").Select(g => PointCalculator.Create(g, this)).ToArray();
            Events = definition.Elements("Event").Select(TriggeredEvent.Create).ToArray();
            OnPlayEvents = definition.Elements("OnPlay").Select(GameEvent.Create).ToArray();

            var costs = definition.Grandchildren("Costs", "Option").Select(CardCost.Create).ToArray();
            Costs = (costs.Length == 0 ? new CardCost[] { new FreeCardCost() } : costs).ToImmutableArray<CardCost>();

            BakeProperties = definition.Elements("Bake").Select(x => ResourceConversion.Create(x, Id)).FirstOrDefault();
            ResourceConversions = definition.Grandchildren("ResourceConversions", "ResourceConversion").Select(x => ResourceConversion.Create(x, Id)).ToArray();

            Prerequisites = definition.Elements("Prerequisite").Select(Prerequisite.Create).ToArray();
            Deck deck;
            if (Enum.TryParse((string)definition.Attribute("Deck"), out deck))
                this.Deck = deck;

            CacheExchanges = definition.Grandchildren("TakeCacheExchange", "CacheExchange").Select(x => CacheExchange.Create(x, Id)).ToArray();

            bool firstEffectOnly;
            if (bool.TryParse((string)definition.Attribute("FirstEffectOnly"), out firstEffectOnly)) 
                FirstEffectOnly = firstEffectOnly;
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
        public int Id { get; }

        /// <summary>
        /// The card name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The cards cost options
        /// </summary>
        public ImmutableArray<CardCost> Costs { get; }

        /// <summary>
        /// The card effect description text
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Resource Conversions that occur on bake opperations
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ResourceConversion BakeProperties { get; }

        /// <summary>
        /// Resource Conversions that occur as any time actions
        /// (Including cooking)
        /// </summary>
        public ResourceConversion[] ResourceConversions { get; }

        /// <summary>
        /// Url of the image that represents this card
        /// </summary>
        public string Image { get; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AnytimeAction AnytimeAction { get; }

        [JsonIgnore]
        public PointCalculator[] GameEndPoints { get; }

        [JsonIgnore]
        public TriggeredEvent[] Events { get; }

        [JsonIgnore]
        public GameEvent[] OnPlayEvents { get; }

        [JsonProperty(PropertyName = "Type")]
        public string JsonType { get; }



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

        /// <summary>
        /// If the card only allows a single effect to occur, even if multiple triggers or effects are on the card.
        /// </summary>
        public readonly bool FirstEffectOnly = false;

        private readonly XElement definition;

        private ResourceConversion bakeProperties;
        private ResourceConversion[] resourceConversions;
        private PointCalculator[] gameEndPoints;
    }
}