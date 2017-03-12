using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoardgamePlatform.Game.Notification
{
    public class IntPredicate: INoticePredicate
    {
        public IntPredicate(int value)
        {
            this.Value = value;
        }

        public int Value;

        public virtual string PredicateType
        {
            get { return typeof(IntPredicate).Name; }
        }

    }
}
