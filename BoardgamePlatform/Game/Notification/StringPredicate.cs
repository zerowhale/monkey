using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoardgamePlatform.Game.Notification
{
    public class StringPredicate :INoticePredicate
    {
        public StringPredicate(string value)
        {
            this.Value = value;
        }

        public string Value;

        public string PredicateType
        {
            get { return this.GetType().Name; }
        }
    }
}