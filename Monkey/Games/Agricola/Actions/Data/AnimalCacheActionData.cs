using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.Data
{
    public class AnimalCacheActionData: GameActionData
    {
        public AnimalHousingData[] Assignments;
        public Dictionary<AnimalResource, int> Free;
        public Dictionary<AnimalResource, int> Cook;
    }
}