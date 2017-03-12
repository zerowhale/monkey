using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.Data
{
    public class BuildingResourcesActionData: GameActionData
    {
        public Boolean? Growth
        {
            get;
            set;
        }

        public Resource? Resource1
        {
            get;
            set;
        }

        public Resource? Resource2
        {
            get;
            set;
        }
    }
}