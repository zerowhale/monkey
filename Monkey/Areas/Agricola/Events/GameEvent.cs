using BoardgamePlatform.Game.Notification;
using Monkey.Games.Agricola.Cards;
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

        }

        /// <summary>
        /// Constructor called when this object is created
        /// from an XML definition.  Custom fields on child
        /// objects should be loaded here.
        /// </summary>
        /// <param name="definition"></param>
        public GameEvent(XElement definition)
        {

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
            if(card != null)
            {
                int executionCount = 0;
                ImmutableDictionary<string, Object> metadata;
                if(player.TryGetCardMetadata(card, out metadata))
                {
                    if (metadata.ContainsKey(MetadataKeyExecutionCount))
                        executionCount = (int)metadata[MetadataKeyExecutionCount];
                }
                else
                {
                    metadata = ImmutableDictionary<string, Object>.Empty;
                }
                executionCount++;
                player.SetCardMetadata(card, metadata.SetItem(MetadataKeyExecutionCount, executionCount));
            }

            OnExecute(player, trigger, card, resultingNotices);
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

        /// <summary>
        /// Event execution code goes here.
        /// </summary>
        /// <param name="player">The player to execute the event for.</param>
        /// <param name="resultingNotices">Outgoing informational notices caused by this event.</param>
        protected abstract void OnExecute(AgricolaPlayer player, GameEventTrigger trigger, Card card, List<GameActionNotice> resultingNotices);

    }
}