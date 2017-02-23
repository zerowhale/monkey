using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class FishingActionTrigger : GameEventTrigger
    {
        public FishingActionTrigger()
            :base()
        {

        }

        public FishingActionTrigger(XElement definition)
            : base(definition)
        {

        }
    }
}