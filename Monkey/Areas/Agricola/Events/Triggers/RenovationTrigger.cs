using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monkey.Games.Agricola.Events.Triggers
{
    public class RenovationTrigger : GameEventTrigger
    {
        public RenovationTrigger(HouseType houseType)
            : base()
        {
            HouseType = houseType;
        }

        public RenovationTrigger(XElement definition)
            : base(definition)
        {

        }

        public HouseType HouseType { get; } = HouseType.Any;
    }
}