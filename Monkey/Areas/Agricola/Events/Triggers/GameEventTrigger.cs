using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public abstract class GameEventTrigger
    {

        public GameEventTrigger()
        {
            TriggerType = GameEventTriggerType.Self;
        }

        public GameEventTrigger(XElement definition)
            : this()
        {
            if(definition.Attribute("TriggerType") != null)
                TriggerType = (GameEventTriggerType)Enum.Parse(typeof(GameEventTriggerType), (string)definition.Attribute("TriggerType"));
        }

        public static GameEventTrigger Create(XElement definition)
        {
            var cls = (string)definition.Attribute("Class");
            var type = Type.GetType(cls);

            var obj = (GameEventTrigger)Activator.CreateInstance(type, definition);
            return obj;
        }

        /// <summary>
        /// Checks if a trigger's conditions have been met
        /// </summary>
        /// <param name="resolvingPlayer"></param>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public virtual bool Triggered(AgricolaPlayer resolvingPlayer, AgricolaPlayer triggeringPlayer, GameEventTrigger trigger){
            return this.GetType().Equals(trigger.GetType())
                && ((this.TriggerType == GameEventTriggerType.Self && resolvingPlayer == triggeringPlayer)
                || (this.TriggerType == GameEventTriggerType.Other && resolvingPlayer != triggeringPlayer)
                || (this.TriggerType == GameEventTriggerType.Any));
        }

        /// <summary>
        /// Type of trigger
        /// </summary>
        public readonly GameEventTriggerType TriggerType;
    }
}