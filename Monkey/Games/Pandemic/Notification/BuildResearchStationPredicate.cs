using Monkey.Game.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Notification
{
    public class BuildResearchStationPredicate: INoticePredicate
    {
        public BuildResearchStationPredicate(int cityId, bool operationsExpert = false)
        {
            City = cityId;
            OperationsExpert = operationsExpert;
        }

        public int City
        {
            get;
            set;
        }

        public bool OperationsExpert
        {
            get;
            set;
        }

        public string PredicateType
        {
            get
            {
                return typeof(BuildResearchStationPredicate).Name;
            }
        }
    }
}