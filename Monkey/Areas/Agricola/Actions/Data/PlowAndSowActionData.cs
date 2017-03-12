using Monkey.Games.Agricola.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.Data
{
    public class PlowAndSowActionData: GameActionData
    {
        public int[] Fields;

        public SowData[] Sow;

        public int? PlowUsed;
    }
}