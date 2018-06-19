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

namespace Monkey.Games.Agricola.Cards
{
    public abstract class Card
    {
        public Card(XElement definition)
        {
            this.definition = definition;

            Id = (int)definition.Attribute("Id");
            Name = (string)definition.Element("Name");
            Text = definition.Element("Text").Value;
            Costs = definition.Grandchildren("Costs", "Option").Select(CardCost.Create).ToArray();
            Image = (string)definition.Attribute("Image");
            AnytimeAction = definition.Descendants("AnytimeAction").Select(AnytimeAction.Create).FirstOrDefault();
            GameEndPoints = definition.Elements("VictoryPointCalculator").Select(PointCalculator.Create).ToArray();
            Events = definition.Elements("Event").Select(TriggeredEvent.Create).ToArray();
            OnPlayEvents = definition.Descendants("OnPlay").Select(GameEvent.Create).ToArray();
            BakeProperties = definition.Descendants("Bake").Select(ResourceConversion.Create).FirstOrDefault();
            ResourceConversions = definition.Grandchildren("ResourceConversions", "ResourceConversion").Select(ResourceConversion.Create).ToArray();

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


        public Card Clone()
        {
            return Card.Create(this.definition);
        }

        public bool ShouldSerializeResourceConversions()
        {
            return resourceConversions.Length > 0;
        }

        /// <summary>
        /// The cards unique identifier
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// The card name
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The cards cost options
        /// </summary>
        public CardCost[] Costs
        {
            get { return costs.ToArray();  }
            set {
                if (value.Length == 0)
                    costs = new CardCost[] { new FreeCardCost() };
                else
                    costs = value;  
            }
        }

        private CardCost[] costs;



        /// <summary>
        /// The card effect description text
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Resource Conversions that occur on bake opperations
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public ResourceConversion BakeProperties
        {
            get
            {
                return bakeProperties;
            }
            set
            {
                bakeProperties = value;
                if(bakeProperties != null)
                    bakeProperties.Id = this.Id;
            }
        }

        /// <summary>
        /// Resource Conversions that occur as any time actions
        /// (Including cooking)
        /// </summary>
        public ResourceConversion[] ResourceConversions
        {
            get
            {
                return resourceConversions;
            }
            set
            {
                resourceConversions = value;
                if (resourceConversions != null)
                {
                    foreach (var resourceConversion in resourceConversions)
                        resourceConversion.Id = Id;
                }
            }
        }



        public string Image
        {
            get;
            set;
        }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AnytimeAction AnytimeAction
        {
            get;
            set;
        }

        [JsonIgnore]
        public PointCalculator[] GameEndPoints{
            get { return gameEndPoints; }
            set {
                gameEndPoints = value;
                foreach (var pointCalculator in gameEndPoints)
                {
                    pointCalculator.OwningCard = this;
                }
            }
        }

        [JsonIgnore]
        public TriggeredEvent[] Events
        {
            get;
            set;
        }

        [JsonIgnore]
        public GameEvent[] OnPlayEvents
        {
            get;
            set;
        }

        [JsonIgnore]
        public bool Dirty
        {
            get;
            set;
        }


        public abstract string JsonType
        {
            get;
        }

        /// <summary>
        /// Any cards requiring preservation of custom state data should store
        /// it in the metadata
        /// </summary>
        [JsonIgnore]
        public Dictionary<String, Object> Metadata = new Dictionary<string, object>();

        private XElement definition;

        private ResourceConversion bakeProperties;
        private ResourceConversion[] resourceConversions;
        private PointCalculator[] gameEndPoints;
    }
}