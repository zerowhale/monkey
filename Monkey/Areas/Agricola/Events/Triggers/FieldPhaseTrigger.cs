using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class FieldPhaseTrigger: GameEventTrigger
    {
        public FieldPhaseTrigger()
            :base()
        {

        }

        public FieldPhaseTrigger(XElement definition)
            : base(definition)
        {

        }
    
    }
}