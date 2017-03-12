using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Catan.Cards
{
    public interface ICard
    {
        int Id
        {
            get;
        }

        string Name
        {
            get;
        }
    }
}