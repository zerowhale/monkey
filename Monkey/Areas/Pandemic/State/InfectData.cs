using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.State
{
    public class InfectData: GameStateData
    {
        public int TotalSteps
        {
            get;
            set;
        }

        public int Step
        {
            get;
            set;
        }
    }
}