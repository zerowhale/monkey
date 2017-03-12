using Monkey.Games.Agricola.Farm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Utils
{
    public static class FenceUtils
    {
        public static PlotInfo GetPlotBorderingFenceData(int x, int y)
        {
            var plotInfo = new PlotInfo();

            plotInfo.NorthFence = new FenceInfo(x, y * 2);
            plotInfo.EastFence = new FenceInfo(x + 1, y * 2 + 1);
            plotInfo.SouthFence = new FenceInfo(x, y * 2 + 2);
            plotInfo.WestFence = new FenceInfo(x, y * 2 + 1);

            return plotInfo;
        }


    }
}