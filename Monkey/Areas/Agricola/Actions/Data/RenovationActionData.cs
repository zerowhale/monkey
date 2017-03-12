using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Actions.Data
{
    public class RenovationActionData: GameActionData
    {
        public BuildFencesActionData FenceData;
        public ImprovementActionData ImprovementData;
    }
}