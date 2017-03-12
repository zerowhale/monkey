using Monkey.Games.Pandemic.Board;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.ClientState
{
    public class PartialMapUpdate
    {
        public void AddCityUpdate(PartialCityNodeUpdate update)
        {
            if (cities == null)
                cities = new Dictionary<int, PartialCityNodeUpdate>();
            cities[update.Id] = update;

        }

        public bool IsEmpty()
        {
            return this.cities == null;
        }

        public PartialCityNodeUpdate[] Cities
        {
            get { return cities.Values.ToArray(); }
        }

        private Dictionary<int, PartialCityNodeUpdate> cities;

    }
}