using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class TakeStoneActionTrigger: GameEventTrigger
    {
        public TakeStoneActionTrigger()
            :base()
        {

        }

        public TakeStoneActionTrigger(XElement definition)
            : base(definition)
        {

        }
    
    }
}