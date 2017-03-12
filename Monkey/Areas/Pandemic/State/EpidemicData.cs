using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.State
{
    public class EpidemicData: GameStateData
    {
        public int RemainingEpidemics
        {
            get;
            set;
        }

        public EpidemicStep Step
        {
            get;
            set;
        }


    }
}