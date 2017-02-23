using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Cards
{
    public class CityCard: ICard
    {
        public CityCard(string name, City city)
        {
            Name = name;
            City = city;
        }

        public int Id
        {
            get
            {
                return (int)City;
            }
        }

        [JsonIgnore]
        public string Name
        {
            get;
            private set;
        }

        [JsonIgnore]
        public City City
        {
            get;
            private set;
        }
    }
}