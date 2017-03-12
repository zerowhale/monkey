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
            this.x = x;
            this.y = y;
            this.index = x + y * Farmyard.FENCES_WIDTH;
        }

        public int X
        {
            get { return X; }
        }

        public int Y
        {
            get { return Y; }
        }

        public int Index
        {
            get { return index; }
        }

        private int x;
        private int y;
        private int index;
    }
}