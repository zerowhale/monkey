using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Data
{
    public struct SowData
    {
        public SowData(Int32 index, Resource type)
        {
            this.Index = index;
            this.Type = type;
        }
        public readonly Int32 Index;
        public readonly Resource Type;
    }
}