using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class BakeTrigger : GameEventTrigger
    {
        public BakeTrigger(int grainBaked = 0)
            :base()
        {
            GrainBaked = grainBaked;
        }

        public BakeTrigger(XElement definition)
            : base(definition)
        {

        }

        public int GrainBaked { get; }

    }
}