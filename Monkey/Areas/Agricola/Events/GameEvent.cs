using BoardgamePlatform.Game.Notification;
using Google.Protobuf.Collections;
using Monkey.Games.Agricola.Cards;
using Monkey.Games.Agricola.Cards.Costs;
using Monkey.Games.Agricola.Events.Conditionals;
using Monkey.Games.Agricola.Events.Triggers;
using Monkey.Games.Agricola.Notification;
using Monkey.Games.Agricola.Utils;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events
{
    /// <summary>
    /// Base class for all game events.
    /// </summary>
    public abstract class GameEvent
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public GameEvent()
        {
            UntilExecution = Int32.MaxValue;
        }

        /// <summary>
        /// Constructor called when this object is created
        /// from an XML definition.  Custom fields on child
        /// objects should be loaded here.
        /// </summary>
        /// <param name="definition"></param>
        public GameEvent(XElement definition)
        {
            UntilExecution = Int32.MaxValue;

            if (definition.Attribute("UntilExecution") != null)
                UntilExecution = (int)definition.Attribute("UntilExecution");
            if (definition.Attribute("FromExecution") != null)
                FromExecution = (int)definition.Attribute("FromExecution");

            var orConditionals = definition.Elements("Or").Select(q => GameEventConditional.Create(q, typeof(OrConditional))).ToArray<GameEventConditional>();
            var andConditionals = definition.Elements("And").Select(q => GameEventConditional.Create(q, typeof(AndConditional))).ToArray<GameEventConditional>();
            Conditionals = definition.Elements("Conditional").Select(GameEventConditional.Create)
                .Concat(orConditionals)
                .Concat(andConditionals)
                .ToArray();
           
        }

        /// <summary>
        /// Instantiates an instance of the event from an XML definition.
        /// </summary>
        /// <param name="definition">The XML definition.</param>
        /// <returns>A GameEvent object of the type specified in the xml definition.</returns>
        public static GameEvent Create(XElement definition)
        {
            var cls = (string)definition.Attribute("Class");
            var type = Type.GetType(cls);

            return (GameEvent)Activator.CreateInstance(type, definition);
        }

        /// <summary>
        /// Executes the event.
        /// </summary>
        /// <param name="player">The player to execute the event for.</param>
        /// <param name="resultingNotices">Outgoing informational notices caused by this event.</param>
        public void Execute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices)
        {
            var shouldExecute = true;
            ImmutableDictionary<string, Object> metadata = null;
            int executionCount = 0;
            player.TryGetCardMetadata(card, out metadata);

            if (card.FirstEffectOnly && metadata != null && metadata.ContainsKey(MetadataKeyExecutionCount))
            {
                executionCount = (int)metadata[MetadataKeyExecutionCount];
                if(executionCount > 0)
                {
                    shouldExecute = false;
                }
            }


            /*
            if (executionCount < FromExecution
                && executionCount >= UntilExecution)
            {
                shouldExecute = false;
            }*/


            if (Conditionals.Count() > 0)
            {
                foreach (var condition in Conditionals)
                {
                    if (!condition.IsMet(player, null))
                    {
                        shouldExecute = false;
                        break;
                    }
                }
            }
            if (shouldExecute)
            {
                if (card != null)
                {
                    if(metadata == null)
                        metadata = ImmutableDictionary<string, Object>.Empty;
                    
                    executionCount++;
                    player.SetCardMetadata(card, metadata.SetItem(MetadataKeyExecutionCount, executionCount));
                }

                OnExecute(player, trigger, card, resultingNotices);
            }
        }

        /// <summary>
        /// Class type output for use in javascript
        /// </summary>
        [JsonProperty(PropertyName="Type")]
        public string JavascriptType
        {
            get { return this.GetType().Name.ToString(); }            
        }

        public const string MetadataKeyExecutionCount = "ExecutionCount";

        [JsonIgnore]
        public readonly GameEventConditional[] Conditionals;

        /// <summary>
        /// Starting after execution # (inclusive)
        /// </summary>
        [JsonIgnore]
        public readonly int FromExecution;

        /// <summary>
        /// Until execution # (non inclusive)
        /// </summary>
        [JsonIgnore]
        public readonly int UntilExecution;

        /// <summary>
        /// Event execution code goes here.
        /// </summary>
        /// <param name="player">The player to execute the event for.</param>
        /// <param name="resultingNotices">Outgoing informational notices caused by this event.</param>
        protected abstract void OnExecute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices);


    }
}