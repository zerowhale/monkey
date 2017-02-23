using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    /// <summary>
    /// TakeOccupationActionTrigger is fired any time an Occupation 
    /// action is used.  This is *not* called any time an occupation enters 
    /// player (ie: as the result of a card allowing you to put an occupation into play)
    /// </summary>
    public class TakeOccupationActionTrigger: GameEventTrigger
    {
        public TakeOccupationActionTrigger()
            :base()
        {

        }

        public TakeOccupationActionTrigger(XElement definition)
            : base(definition)
        {

        }

    }
}