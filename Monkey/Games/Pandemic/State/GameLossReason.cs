using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.State
{
    public enum GameLossReason
    {
        None,
        DeckEmpty,
        Outbreaks,
        NoBlack,
        NoBlue,
        NoYellow,
        NoRed
    }
}