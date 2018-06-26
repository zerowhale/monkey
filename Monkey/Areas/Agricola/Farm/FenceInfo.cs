using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public struct FenceInfo
    {
        public FenceInfo(int x, int y)
        {
            X = x;
            Y = y;
            Index = x + y * Farmyard.FENCES_WIDTH;
        }

        public int X { get; }
        public int Y { get; }

        public int Index { get; }

    }
}