using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic
{
    public enum GameState
    {
        PlayerMove,
        Epidemic,
        Infect,
        Finished
    }
}