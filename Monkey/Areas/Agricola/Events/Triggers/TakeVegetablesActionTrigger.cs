using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class TakeVegetablesActionTrigger: GameEventTrigger
    {
        public TakeVegetablesActionTrigger()
            :base()
        {

        }

        public TakeVegetablesActionTrigger(XElement definition)
            : base(definition)
        {

        }
    
    }
}