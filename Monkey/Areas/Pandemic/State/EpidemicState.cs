using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.State
{
    public enum EpidemicStep
    {
        Increase = 0,
        Infect = 1,
        Intensify = 2
    }
}