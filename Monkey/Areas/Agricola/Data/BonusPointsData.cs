using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Data
{
    public struct BonusPointsData
    {
        public BonusPointsData(string name, int count)
        {
            this.Name = name;
            this.Count = count;
        }

        public readonly string Name;

        public readonly int Count;
    }
}