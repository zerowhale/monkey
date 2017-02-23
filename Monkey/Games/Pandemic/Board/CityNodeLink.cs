using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Pandemic.Board
{
    public struct CityNodeLink
    {
        public CityNodeLink(City A, City B){
            this.A = A;
            this.B = B;
        }

        public City A;
        public City B;
    }
}