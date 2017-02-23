using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Cards
{
    public interface IImprovement
    {
        /// <summary>
        /// Victory points for owning this card
        /// </summary>
        int Points { get; set; }

        bool CookingHearth{get;set;}
        bool Fireplace { get; set; }
        bool Oven { get; set; }
        bool Bakes { get; set; }

    }
}