using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class TakeGrainActionTrigger: GameEventTrigger
    {
        public TakeGrainActionTrigger()
            :base()
        {

        }

        public TakeGrainActionTrigger(XElement definition)
            : base(definition)
        {

        }
    
    }
}