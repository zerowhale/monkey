using Monkey.Game.Notification;
using Monkey.Games.Agricola.Cards;
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
        public void Execute(AgricolaPlayer player, List<GameActionNotice> resultingNotices)
        {
            ExecutionCount++;
            OnExecute(player, resultingNotices);
        }

        /// <summary>
        /// Event execution code goes here.
        /// </summary>
        /// <param name="player">The player to execute the event for.</param>
        /// <param name="resultingNotices">Outgoing informational notices caused by this event.</param>
        protected abstract void OnExecute(AgricolaPlayer player, List<GameActionNotice> resultingNotices);

        /// <summary>
        /// The trigger that caused this event to be executed.
        /// </summary>
        [JsonIgnore]
        public GameEventTrigger ActiveTrigger
        {
            get;
            set;
        }

        /// <summary>
        /// The number of times this event has executed.  Whether or not
        /// the execution has any effect is irrelevant, every call to 
        /// Execute adds one to this counter.
        /// </summary>
        [JsonIgnore]
        public int ExecutionCount
        {
            get;
            private set;
        }

        /// <summary>
        /// Class type output for use in javascript
        /// </summary>
        [JsonProperty(PropertyName="Type")]
        public string JavascriptType
        {
            get { return this.GetType().Name.ToString(); }            
        }
    }
}