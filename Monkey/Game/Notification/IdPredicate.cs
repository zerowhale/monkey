using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monkey.Game.Notification
{
    public class IdPredicate : INoticePredicate
    {

        public IdPredicate(int id)
        {
            this.Id = id;
        }

        public int Id;

        public virtual string PredicateType
        {
            get { return this.GetType().Name; }
        }

    }
}