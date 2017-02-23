using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class BuildFencesTrigger : GameEventTrigger
    {
        public BuildFencesTrigger()
            :base()
        {

        }

        public BuildFencesTrigger(XElement definition)
            : base(definition)
        {

        }

        public int FencesBuilt
        {
            get;
            set;
        }
    }
}