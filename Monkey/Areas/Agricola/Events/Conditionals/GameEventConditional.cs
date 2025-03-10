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
        public GameEventConditional() {}

        
        public GameEventConditional(XElement definition)
            : this() {}
        

        public static GameEventConditional Create(XElement definition)
        {
            var cls = (string)definition.Attribute("Class");
            var type = Type.GetType(cls);

            var obj = (GameEventConditional)Activator.CreateInstance(type, definition);
            return obj;
        }

        public static GameEventConditional Create(XElement definition, Type type)
        {
            var obj = (GameEventConditional)Activator.CreateInstance(type, definition);
            return obj;
        }

        public virtual bool IsMet(AgricolaPlayer resolvingPlayer, AgricolaPlayer triggeringPlayer)
        {
            return true;
        }
    }
}