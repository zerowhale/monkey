using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class TakeSoleCachedResourceTrigger : GameEventTrigger
    {

        public TakeSoleCachedResourceTrigger(Resource type)
            : base()
        {
            this.Type = type;
        }

        public TakeSoleCachedResourceTrigger(XElement definition)
            : base(definition)
        {
            Type = (Resource)Enum.Parse(typeof(Resource), (string)definition.Attribute("ResourceType"));
        }

        public override bool Triggered(AgricolaPlayer resolvingPlayer, AgricolaPlayer triggeringPlayer, GameEventTrigger trigger)
        {
            return base.Triggered(resolvingPlayer, triggeringPlayer, trigger) 
                && this.Type == ((TakeSoleCachedResourceTrigger)trigger).Type;
        }

        public Resource Type
        {
            get;
            private set;
        }
    }
}