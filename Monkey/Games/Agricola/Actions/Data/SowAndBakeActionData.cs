using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.Data
{
    public class SowAndBakeActionData: GameActionData
    {
        public SowData[] Sow;

        public ResourceConversionData[] BakeData;
    }
}