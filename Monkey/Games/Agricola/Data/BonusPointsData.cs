using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Data
{
    public class BonusPointsData
    {
        public BonusPointsData(string name, int count)
        {
            this.Name = name;
            this.Count = count;
        }

        public string Name
        {
            get;
            set;
        }

        public int Count
        {
            get;
            set;
        }
    }
}