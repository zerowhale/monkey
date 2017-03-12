using Monkey.Games.Agricola.Actions.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Data
{
    public class HarvestData
    {
        public ResourceConversionData[] FeedResources;

        public AnimalCacheActionData AnimalData;


        public override string ToString()
        {
            return "harvest data toString() not implemented";
        }

    }
}