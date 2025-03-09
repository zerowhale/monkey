using Monkey.Games.Agricola;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Conditionals
{
    public class GameEventConditional
    {
        public GameEventConditional()
        {
            //TriggerType = GameEventTriggerType.Self;
        }

        
        public GameEventConditional(XElement definition)
            : this()
        {

            /*
            if (definition.Attribute("TriggerType") != null)
                TriggerType = (GameEventTriggerType)Enum.Parse(typeof(GameEventTriggerType), (string)definition.Attribute("TriggerType"));
            */
        }
        

        public static GameEventConditional Create(XElement definition)
        {
            var cls = (string)definition.Attribute("Class");
            var type = Type.GetType(cls);

            var obj = (GameEventConditional)Activator.CreateInstance(type, definition);
            return obj;
        }

        public virtual bool IsMet(AgricolaPlayer resolvingPlayer, AgricolaPlayer triggeringPlayer)
        {
            return true;
        }
    }
}