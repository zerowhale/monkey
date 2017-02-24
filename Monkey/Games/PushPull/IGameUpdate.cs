using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.PushPull
{
    public interface IGameUpdate
    {
        GameState? State
        {
            get;
        }

    }
}