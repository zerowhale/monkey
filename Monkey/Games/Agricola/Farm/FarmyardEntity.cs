using Monkey.Game.Utils;
using Monkey.Games.Agricola.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class FarmyardEntity
    {
        public FarmyardEntity(String type)
        {
            Location = new Point();
            Type = type;
        }

        public Point Location
        {
            get;
            protected set;
        }

        public String Type
        {
            get;
            protected set;
        }
    }
    
}