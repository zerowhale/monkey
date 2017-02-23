using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class DayLaborerActionTrigger: GameEventTrigger
    {
        public DayLaborerActionTrigger()
            :base()
        {

        }

        public DayLaborerActionTrigger(XElement definition)
            : base(definition)
        {

        }
    
 
    }
}