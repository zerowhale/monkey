using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Games.Agricola.Farm
{
    public class Empty: FarmyardEntity
    {

        public Empty()
            : this(false)
        {
        }

        public Empty(bool hasStable)
            : base("Empty")
        {
            HasStable = hasStable;
        }

        public Empty(string type, bool hasStable)
            : base(type)
        {
            HasStable = hasStable;
        }

        public virtual Empty AddStable()
        {
            return new Empty(true);
        }

        public Boolean HasStable { get; }

    }
}