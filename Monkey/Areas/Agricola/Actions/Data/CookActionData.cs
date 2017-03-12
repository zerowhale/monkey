using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.Data
{
    public class CookActionData: GameActionData
    {
        public ResourceConversionData[] Resources;
        public AnimalCacheActionData AnimalData;
    }
}