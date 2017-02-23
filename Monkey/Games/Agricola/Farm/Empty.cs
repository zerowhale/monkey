using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class Empty: FarmyardEntity
    {

        public Empty()
            : base("Empty")
        {

        }

        public Boolean HasStable
        {
            get;
            set;
        }
    }
}